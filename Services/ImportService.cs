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

            this.clearImportFilesDirLocal();
            if (!this.clearMigrateDirInDestinationSite(destinationSite))
            {
                return new Result(Status.Failed, "Could not clean destination site /home/dev/migrate folder...");
            }

            Result importAppServiceDataResult = importAppServiceData(destinationSite);
            if (importAppServiceDataResult.status == Status.Failed || importAppServiceDataResult.status == Status.Cancelled)
            {
                return importAppServiceDataResult;
            }

            Result importDatabaseContentResult = importDatabaseContent(destinationSite, destinationSite.databaseName,webAppResource);
            if (importDatabaseContentResult.status == Status.Failed || importDatabaseContentResult.status == Status.Cancelled)
            {
                return importDatabaseContentResult;
            }

            AzureManagementUtils.UpdateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME, 
                destinationSite.databaseName);

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
    }
}
