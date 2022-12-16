using Azure.ResourceManager.AppService;
using MySqlX.XDevAPI.Common;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        private RichTextBox? _progressViewRTextBox;
        private string[]  _previousMigrationStatus;
        private string _migrationStatusFilePath;

        public ImportService() { }

        public ImportService(RichTextBox? progressViewRTextBox, string[] previousMigrationStatus)
        {
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
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

            this._migrationStatusFilePath= Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);

            try
            {
                Stopwatch timer = Stopwatch.StartNew();

                webAppResource = AzureManagementUtils.GetWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
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

                result = this.ClearMigrateDirInDestinationSite(destinationSite, "1");
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

                if (!this.UpdateDatabaseNameAppSetting(webAppResource, destinationSite))
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return new Result(Status.Failed, "Couldn't update Database name application setting.");
                }

                result = this.PostProcessingImport(destinationSite, destinationSite.databaseName, webAppResource);
                if (result.status != Status.Completed)
                {
                    this.RevertDestinationSiteMigrationState(webAppResource);
                    return result;
                }

                result = this.ClearMigrateDirInDestinationSite(destinationSite, "2");
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
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.importAppServiceDataCompleted))
            {
                return new Result(Status.Completed, "Imported App service Data in previous migration attempt.");
            }
            LinuxAppDataImportService linAppImportService = new LinuxAppDataImportService(destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword, this._progressViewRTextBox, this._previousMigrationStatus);
            
            Result result = linAppImportService.ImportData();
            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.importAppServiceDataCompleted + Environment.NewLine);
            }
            return result;
        }

        private Result ImportDatabaseContent(SiteInfo destinationSite, string newDatabaseName, WebSiteResource destinationSiteResource)
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.importDatabaseContentCompleted))
            {
                return new Result(Status.Completed, "Imported Database in previous migration attempt.");
            }
            LinuxMySQLDataImportService linDBImportService = new LinuxMySQLDataImportService(destinationSiteResource, destinationSite.databaseHostname,
                destinationSite.databaseUsername, destinationSite.databasePassword, newDatabaseName, destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword, this._progressViewRTextBox);

            Result result = linDBImportService.ImportData();
            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.importDatabaseContentCompleted + Environment.NewLine);
            }
            return result;
        }

        private void ClearImportFilesDirLocal()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.clearImportFilesLocalDir))
            {
                return;
            }

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
            File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.clearImportFilesLocalDir + Environment.NewLine);
        }

        private Result ClearMigrateDirInDestinationSite(SiteInfo destinationSite, string callOrder)
        {
            if (this._previousMigrationStatus.Contains(String.Format(Constants.StatusMessages.clearMigrationDirInDestinationSite, callOrder)))
            {
                return new Result(Status.Completed, "Cleared destination migrate Dir in previous migration attempt.");
            }
            HelperUtils.WriteOutputWithNewLine("Cleaning up migration " +
                "data on Linux App Service...", this._progressViewRTextBox);

            Result result = HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, 
                destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);

            System.Diagnostics.Debug.WriteLine("clear migration dir after clearappservicedirectory..");

            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, String.Format(Constants.StatusMessages.clearMigrationDirInDestinationSite, callOrder) + Environment.NewLine);
            }
            return result;
        }

        private Result ValidateWPRootDirInDestinationSite(SiteInfo destinationSite)
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.validateWPRootDirInDestinationSite))
            {
                return new Result(Status.Completed, "Validated WordPress installation on Destination site in previous migration attempt.");
            }

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

            File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.validateWPRootDirInDestinationSite + Environment.NewLine);
            return new Result(Status.Completed, "");
        }

        private bool UpdateDatabaseNameAppSetting(WebSiteResource webAppResource, SiteInfo destinationSite)
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.updateDatabaseNameAppSetting))
            {
                return true;
            }

            HelperUtils.WriteOutputWithNewLine("Updating database details for Linux WordPress", this._progressViewRTextBox);
            
            if (AzureManagementUtils.UpdateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME,
                destinationSite.databaseName))
            {
                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.updateDatabaseNameAppSetting + Environment.NewLine);
                return true;
            }

            return false;
        }

        private Result TriggerDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            try
            {
                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");
                if (!AzureManagementUtils.UpdateApplicationSettingForAppService(destinationSiteResource, appSettings))
                {
                    return new Result(Status.Failed, "Could not update App Settings of Destination site");
                }

                string[] appSettingsToRemove = { Constants.NEW_DATABASE_NAME_APP_SETTING, Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING };
                if (!AzureManagementUtils.RemoveApplicationSettingForAppService(destinationSiteResource, appSettingsToRemove))
                {
                    return new Result(Status.Failed, "Could not update App Settings of Destination site");
                }
            }
            catch 
            {
                return new Result(Status.Failed, "Could not update App Settings of Destination site");
            }
            return new Result(Status.Completed, "");
        }

        private Result RevertDestinationSiteMigrationState(WebSiteResource destinationSiteResource)
        {
            int retriesCount = 1;
            while (retriesCount <= Constants.MAX_RETRIES_COMMON)
            {
                try
                {
                    string[] appSettings = { Constants.START_MIGRATION_APP_SETTING, Constants.NEW_DATABASE_NAME_APP_SETTING, Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING };
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

        public Result PostProcessingImport(SiteInfo destinationSite, string databaseName, WebSiteResource webAppResource)
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.postProcessingImportCompleted))
            {
                return new Result(Status.Completed, "Completed post processing of Import data in previous migration attempt.");
            }

            Result result = this._StartPostProcessing(destinationSite, databaseName, webAppResource);
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._WaitForPostProcessing(destinationSite, databaseName, webAppResource);
            if (result.status != Status.Completed)
            {
                this.StopPostProcessing(destinationSite, databaseName, webAppResource);
                return result;
            }

            result = this.StopPostProcessing(destinationSite, databaseName, webAppResource);
            if (result.status != Status.Completed)
            {
                return result;
            }

            File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.postProcessingImportCompleted + Environment.NewLine);
            
            return result;
        }

        public Result _StartPostProcessing(SiteInfo destinationSite, string databaseName, WebSiteResource destinationSiteResource)
        {
            try
            {
                HelperUtils.WriteOutputWithNewLine("Initiating MySQL import on destination site.", this._progressViewRTextBox);
                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");
                appSettings.Add(Constants.NEW_DATABASE_NAME_APP_SETTING, databaseName);
                appSettings.Add(Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING, String.Format("{0}{1}", Constants.MYSQL_TEMP_DIR, Constants.WIN_MYSQL_SQL_FILENAME));
                if (AzureManagementUtils.UpdateApplicationSettingForAppService(destinationSiteResource, appSettings))
                {
                    return new Result(Status.Completed, "");
                }
            }
            catch { }
            return new Result(Status.Failed, "Unable to initiate MySQL import process on destination site...");
        }

        public Result StopPostProcessing(SiteInfo destinationSite, string databaseName, WebSiteResource destinationSiteResource)
        {
            int retiresCount = 1;
            while (retiresCount <= Constants.MAX_RETRIES_COMMON)
            {
                try
                {
                    string[] appSettings = { Constants.START_MIGRATION_APP_SETTING, Constants.NEW_DATABASE_NAME_APP_SETTING, Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING };
                    if (AzureManagementUtils.RemoveApplicationSettingForAppService(destinationSiteResource, appSettings))
                    {
                        return new Result(Status.Completed, "");
                    }
                }
                catch { }
                retiresCount++;
            }

            return new Result(Status.Failed, "Could not clear " +
                "App Settings used for database import trigger...");
        }

        public Result _WaitForPostProcessing(SiteInfo destinationSite, string databaseName, WebSiteResource webAppResource)
        {
            string checkDbImportStatusNestedCommand = String.Format("cat {0}", Constants.LIN_APP_DB_STATUS_FILE_PATH);
            Stopwatch timer = Stopwatch.StartNew();
            int maxRetryCount = 2000;

            for (int i = 0; i < maxRetryCount; i++)
            {
                KuduCommandApiResult checkDbImportStatusResult = HelperUtils.ExecuteKuduCommandApi(checkDbImportStatusNestedCommand, destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);
                if (checkDbImportStatusResult.status == Status.Completed
                    && checkDbImportStatusResult.exitCode == 0
                    && checkDbImportStatusResult.output != null)
                {
                    if (checkDbImportStatusResult.output.Contains(Constants.IMPORT_SUCCESS_MESSAGE))
                    {
                        HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
                        return new Result(Status.Completed, "Completed Post processing on destination site.");
                    }
                    if (checkDbImportStatusResult.output.Contains(Constants.IMPORT_FAILURE_MESSAGE))
                    {
                        HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
                        return new Result(Status.Failed, "Could not complete post processing on destination site.");
                    }
                }

                HelperUtils.WriteOutputWithRC("Waiting for post processing of import data. Elapsed time = "
                    + (timer.ElapsedMilliseconds / 1000) + " seconds.", this._progressViewRTextBox);
                Thread.Sleep(10000);
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
            return new Result(Status.Failed, "Unable to complete post processing of Import on destination site.");
        }
    }
}
