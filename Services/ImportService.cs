using Azure.ResourceManager.AppService;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        private RichTextBox? _progressViewRTextBox;

        public ImportService() { }

        public ImportService(RichTextBox? progressViewRTextBox)
        {
            this._progressViewRTextBox = progressViewRTextBox;
        }

        public Result ImportDataToDestinationSite(SiteInfo destinationSite, string newDatabaseName) {
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
            WebSiteResource webAppResource = null;


            try
            {
                HelperUtils.WriteOutputWithNewLine("Retrieving WebApp publishing profile and database " 
                    + "details for Linux WordPress... ", this._progressViewRTextBox);
                Stopwatch timer = Stopwatch.StartNew();

                webAppResource = AzureManagementUtils.GetWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
                IDictionary<string, string> applicationSettings = AzureManagementUtils.GetApplicationSettingsForAppService(webAppResource);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);

                destinationSite.ftpUsername = publishingProfile.PublishingUserName;
                destinationSite.ftpPassword = publishingProfile.PublishingPassword;
                destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
                destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
                destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
                destinationSite.databaseName = newDatabaseName;

                HelperUtils.WriteOutputWithNewLine("Successfully retrieved the details... time taken="
                    + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);
                timer.Stop();


                this.ClearImportFilesDirLocal();
                Result result = this.TriggerDestinationSiteMigrationState(webAppResource);
                if (result.status != Status.Completed)
                {
                    return result;
                }

                result = this.ClearMigrateDirInDestinationSite(destinationSite);
                if (result.status != Status.Completed) 
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                result = this.ValidateWPRootDirInDestinationSite(destinationSite);
                if (result.status != Status.Completed)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                result = ImportAppServiceData(destinationSite);
                if (result.status != Status.Completed)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                result = ImportDatabaseContent(destinationSite, destinationSite.databaseName, webAppResource);
                if (result.status != Status.Completed)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                HelperUtils.WriteOutputWithNewLine("Updating database details for Linux WordPress", this._progressViewRTextBox);
                AzureManagementUtils.UpdateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME,
                    destinationSite.databaseName);

                result = this.ClearMigrateDirInDestinationSite(destinationSite);
                if (result.status != Status.Completed)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                result = this.RevertDestinationSiteMigrationState(webAppResource);
                if (result.status != Status.Completed)
                {
                    return result;
                }

                webAppResource.Restart();
                return new Result(Status.Completed, Constants.SUCCESS_IMPORT_MESSAGE);
            }
            catch (Exception ex)
            {
                if (webAppResource != null)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                }
                return new Result(Status.Failed, ex.Message);
            }
        }

        private Result ImportAppServiceData(SiteInfo destinationSite)
        {
            LinuxAppDataImportService linAppImportService = new LinuxAppDataImportService(destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword, this._progressViewRTextBox);
            return linAppImportService.ImportData();
        }

        private Result ImportDatabaseContent(SiteInfo destinationSite, string newDatabaseName, WebSiteResource destinationSiteResource)
        {
            LinuxMySQLDataImportService linDBImportService = new LinuxMySQLDataImportService(destinationSiteResource, destinationSite.databaseHostname,
                destinationSite.databaseUsername, destinationSite.databasePassword, newDatabaseName, destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword, this._progressViewRTextBox);

            return linDBImportService.ImportData();
        }

        private void ClearImportFilesDirLocal()
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

        private Result ClearMigrateDirInDestinationSite(SiteInfo destinationSite)
        {
            HelperUtils.WriteOutputWithNewLine("Cleaning up migration " +
                "data on Linux App Service...", this._progressViewRTextBox);

            return HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, 
                destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);
        }

        private Result ValidateWPRootDirInDestinationSite(SiteInfo destinationSite)
        {

            HelperUtils.WriteOutputWithNewLine("Validating WordPress installation on Linux App Service...", this._progressViewRTextBox);
            string validateWPRootCommand = String.Format("test -e {0} && test -e {1} && grep '{2}' {3}", Constants.LIN_APP_WP_CONFIG_PATH, 
                Constants.LIN_APP_VERSIONPHP_FILE_PATH, Constants.FIRST_TIME_SETUP_COMPLETETED_MESSAGE, Constants.LIN_APP_WP_DEPLOYMENT_STATUS_FILE_PATH);

            KuduCommandApiResult validateWPRootDirResult = HelperUtils.ExecuteKuduCommandApi(validateWPRootCommand, destinationSite.ftpUsername, 
                destinationSite.ftpPassword, destinationSite.webAppName);

            if (validateWPRootDirResult.status != Status.Completed || validateWPRootDirResult.exitCode != 0
                || !validateWPRootDirResult.output.Contains(Constants.FIRST_TIME_SETUP_COMPLETETED_MESSAGE))
            {
                return new Result(Status.Failed, "Could not refresh WordPress code on destination site.");
            }

            return new Result(Status.Completed, "");
        }

        private Result TriggerDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            try
            {
                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add(Constants.LIN_APP_PREVENT_WORDPRESS_INSTALL_APP_SETTING, "True");
                appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");
                if (AzureManagementUtils.UpdateApplicationSettingForAppService(destinationSiteResource, appSettings))
                {
                    return new Result(Status.Completed, "");
                }
            }
            catch {}
            return new Result(Status.Failed, "Could not add migration in progress application setting.");
        }

        private Result RevertDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            int retriesCount = 1;
            while (retriesCount <= Constants.MAX_RETRIES_COMMON)
            {
                try
                {
                    string[] appSettings = { Constants.START_MIGRATION_APP_SETTING };
                    if (AzureManagementUtils.RemoveApplicationSettingForAppService(destinationSiteResource, appSettings))
                    {
                        return new Result(Status.Completed, "");
                    }
                }
                catch { }
                retriesCount++;
            }
            return new Result(Status.Failed, "Unable to revert migration " +
                "application settings on Linux App Service.");
        }
    }
}
