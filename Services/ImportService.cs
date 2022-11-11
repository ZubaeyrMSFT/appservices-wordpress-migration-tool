using Azure.ResourceManager.AppService;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using WordPressMigrationTool.Utilities;
using System.Threading.Tasks;
using Ionic.Zip;
using System.IO.Compression;
using WordPressMigrationTool.Utilities;
using System.Diagnostics;
using Renci.SshNet;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        public Result importDataToDestinationSite(SiteInfo destinationSite, string newDatabaseName) {
            if (string.IsNullOrWhiteSpace(destinationSite.subscriptionId))
            {
                return new Result(Status.Failed, "Subscription Id should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.resourceGroupName))
            {
                return new Result(Status.Failed, "Resource Group should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.webAppName))
            {
                return new Result(Status.Failed, "App Service name should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(newDatabaseName))
            {
                return new Result(Status.Failed, "Final database name should not be empty!");
            }

            WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
            IDictionary<string, string> applicationSettings = AzureManagementUtils.GetApplicationSettingsForAppService(webAppResource);
            PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);

            destinationSite.ftpUsername = publishingProfile.PublishingUserName;
            destinationSite.ftpPassword = publishingProfile.PublishingPassword;
            destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
            destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
            destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
            destinationSite.databaseName = newDatabaseName;

            this.triggerDestinationSiteMigrationState(webAppResource);

            this.clearImportFilesDirLocal();
            if (!this.clearMigrateDirInDestinationSite(destinationSite))
            {
                return new Result(Status.Failed, "Could not clean destination site /home/dev/migrate folder...");
            }

            if (!this.validateWPRootDirInDestinationSite(destinationSite))
            {
                return new Result(Status.Failed, "Could not refresh WordPress code in destination site...");
            }

            Result importAppServiceDataResult = importAppServiceData(destinationSite);
            if (importAppServiceDataResult.status == Status.Failed || importAppServiceDataResult.status == Status.Cancelled)
            {
                return importAppServiceDataResult;
            }

            Result importDatabaseContentResult = importDatabaseContent(destinationSite, destinationSite.databaseName, webAppResource);
            if (importDatabaseContentResult.status == Status.Failed || importDatabaseContentResult.status == Status.Cancelled)
            {
                return importDatabaseContentResult;
            }

            AzureManagementUtils.UpdateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME,
                destinationSite.databaseName);

            this.clearMigrateDirInDestinationSite(destinationSite);

            if (!this.revertDestinationSiteMigrationState(webAppResource))
            {
                return new Result(Status.Failed, "Could not remove MIGRATION_IN_PROGRESS app setting.");
            }

            webAppResource.Restart();
            return new Result(Status.Completed, Constants.SUCCESS_IMPORT_MESSAGE);
        }

        private Result importAppServiceData(SiteInfo destinationSite)
        {
            LinuxAppDataImportService linAppImportService = new LinuxAppDataImportService(destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword);
            return linAppImportService.importData();
        }

        private Result importDatabaseContent(SiteInfo destinationSite, String newDatabaseName, WebSiteResource destinationSiteResource)
        {
            LinuxMySQLDataImportService linDBImportService = new LinuxMySQLDataImportService(destinationSiteResource, destinationSite.databaseHostname,
                destinationSite.databaseUsername, destinationSite.databasePassword, newDatabaseName, destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword);

            return linDBImportService.importData();
        }

        private void clearImportFilesDirLocal()
        {
            string splitZipFilesDirectory = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR);
            if (Directory.Exists(splitZipFilesDirectory))
            {
                Directory.Delete(splitZipFilesDirectory, true);
            }

            string zippedSplitZipFIlesDirectory = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_NESTED_DIR);
            if (Directory.Exists(zippedSplitZipFIlesDirectory))
            {
                Directory.Delete(zippedSplitZipFIlesDirectory, true);
            }
        }

        private bool clearMigrateDirInDestinationSite(SiteInfo destinationSite)
        {
            return HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);
        }

        private bool validateWPRootDirInDestinationSite(SiteInfo destinationSite)
        {
            string validateWPRootCommand = String.Format("test -e {0} && test -e {1} && grep '{2}' {3}", Constants.LIN_APP_WP_CONFIG_PATH, Constants.LIN_APP_VERSIONPHP_FILE_PATH, Constants.FIRST_TIME_SETUP_COMPLETETED_MESSAGE, Constants.LIN_APP_WP_DEPLOYMENT_STATUS_FILE_PATH);
            KuduCommandApiResult validateWPRootDirResult = HelperUtils.executeKuduCommandApi(validateWPRootCommand, destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);
            if (validateWPRootDirResult.status != Status.Completed
                || validateWPRootDirResult.exitCode != 0
                || !validateWPRootDirResult.output.Contains(Constants.FIRST_TIME_SETUP_COMPLETETED_MESSAGE))
            {
                return false;
            }
            return true;
        }

        // Triggers Migration state in destination site which prevents WP installation.
        private bool triggerDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            Dictionary<string, string> appSettings = new Dictionary<string, string>();
            appSettings.Add(Constants.LIN_APP_PREVENT_WORDPRESS_INSTALL_APP_SETTING, "True");
            appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");

            try
            {
                return AzureManagementUtils.UpdateApplicationSettingForAppService(destinationSiteResource, appSettings);
            }
            catch
            {
                return false;
            }
        }

        private bool revertDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            string[] appSettings = { Constants.START_MIGRATION_APP_SETTING };

            try
            {
                return AzureManagementUtils.removeApplicationSettingForAppService(destinationSiteResource, appSettings);
            }
            catch
            {
                return false;
            }
        }
    }
}
