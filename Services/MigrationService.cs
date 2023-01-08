using Azure.Identity;
using Azure.ResourceManager.AppService;
using Microsoft.VisualBasic;
using System;
using WordPressMigrationTool.Utilities;
using Constants = WordPressMigrationTool.Utilities.Constants;

namespace WordPressMigrationTool
{
    public class MigrationService
    {
        private SiteInfo _sourceSiteInfo;
        private SiteInfo _destinationSiteInfo;
        private WebSiteResource _sourceSiteResourse;
        private WebSiteResource _destinationSiteResource;
        private RichTextBox? _progressViewRTextBox;
        private MigrationUX _migrationUxForm;
        private string[] _previousMigrationStatus;
        private DefaultAzureCredential _azureCredential;

        public MigrationService(SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo, RichTextBox? progressViewRTextBox, string[] previousMigrationStatus, MigrationUX migrationUxForm) { 
            this._sourceSiteInfo = sourceSiteInfo;
            this._destinationSiteInfo = destinationSiteInfo;
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
            this._migrationUxForm = migrationUxForm;
            this._azureCredential = migrationUxForm._azureCredential;
        }

        public Result Migrate()
        {
            try
            {
                Result result = this.InitializeMigrationStatusFile();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                result = this.GetSourceSiteInfo();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                result = this.GetDestinationSiteInfo();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                ValidationService validationService = new ValidationService(this._progressViewRTextBox, this._previousMigrationStatus, this._sourceSiteResourse, this._destinationSiteResource, this._migrationUxForm);
                ExportService exportService = new ExportService(this._progressViewRTextBox, this._previousMigrationStatus);
                ImportService importService = new ImportService(this._progressViewRTextBox, this._previousMigrationStatus, this._destinationSiteResource);

                Result validationRes = validationService.ValidateMigrationInput(this._sourceSiteInfo, this._destinationSiteInfo);
                if (validationRes.status != Status.Completed)
                {
                    return validationRes;
                }

                Result exporttRes = exportService.ExportDataFromSourceSite(this._sourceSiteInfo);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    return exporttRes;
                }
                
                Result importRes = importService.ImportDataToDestinationSite(this._destinationSiteInfo, this._sourceSiteInfo.databaseName);
                if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
                {
                    return importRes;
                }

                // Clean Local Migration data
                this.CleanLocalTempFiles();

                // Clean Migration data in destination (linux) app service
                Result cleanupRes = this.CleanDestinationAppTempFiles(this._destinationSiteInfo);
                if (cleanupRes.status == Status.Failed || cleanupRes.status == Status.Cancelled)
                {
                    return cleanupRes;
                }

                return new Result(Status.Completed, Constants.SUCCESS_MESSAGE);
            }
            catch (Exception ex) {
                return new Result(Status.Failed, ex.Message);
            }
        }

        // retrieves source site's ftp and database credentials (from database connection 'defaultConnection')
        private Result GetSourceSiteInfo()
        {
            HelperUtils.WriteOutputWithNewLine(String.Format("Retrieving WebApp publishing profile and database details " +
                    "for {0} app... ", this._sourceSiteInfo.webAppName), this._progressViewRTextBox);
            try
            {
                WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(this._sourceSiteInfo.subscriptionId, this._sourceSiteInfo.resourceGroupName, this._sourceSiteInfo.webAppName, this._azureCredential);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);
                string databaseConnectionString = AzureManagementUtils.GetDatabaseConnectionString(webAppResource);
                HelperUtils.ParseAndUpdateDatabaseConnectionStringForWinAppService(this._sourceSiteInfo, databaseConnectionString);
                this._sourceSiteInfo.ftpUsername = publishingProfile.PublishingUserName;
                this._sourceSiteInfo.ftpPassword = publishingProfile.PublishingPassword;
                this._sourceSiteInfo.stackVersion = webAppResource.Data.SiteConfig.PhpVersion;
                this._sourceSiteResourse = webAppResource;

                return new Result(Status.Completed, "");
            }
            catch
            {
                return new Result(Status.Failed, "Could not retrieve publishing profile and database connection string of " + this._sourceSiteInfo.webAppName + " appservice. " +
                    "Please select a WordPress on Windows source site with the default database connection string.");
            }
        }

        // retrieves destination site's ftp credentials and other app settings
        private Result GetDestinationSiteInfo()
        {
            HelperUtils.WriteOutputWithNewLine(String.Format("Retrieving WebApp publishing profile and database "
                    + "details for {0}...", this._destinationSiteInfo.webAppName), this._progressViewRTextBox);
            try
            {
                WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(this._destinationSiteInfo.subscriptionId, this._destinationSiteInfo.resourceGroupName, this._destinationSiteInfo.webAppName, this._azureCredential);
                IDictionary<string, string> applicationSettings = AzureManagementUtils.GetApplicationSettingsForAppService(webAppResource);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);

                this._destinationSiteInfo.ftpUsername = publishingProfile.PublishingUserName;
                this._destinationSiteInfo.ftpPassword = publishingProfile.PublishingPassword;
                this._destinationSiteInfo.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
                this._destinationSiteInfo.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
                this._destinationSiteInfo.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
                this._destinationSiteInfo.stackVersion = webAppResource.Data.SiteConfig.LinuxFxVersion;
                this._destinationSiteResource = webAppResource;

                return new Result(Status.Completed, "");
            }
            catch
            {
                return new Result(Status.Failed, "Could not retrieve publishing profile and database app-settings of " + this._destinationSiteInfo.webAppName + " appservice. " +
                    "Please ensure the destination site is a WordPress on Linux app with the default database app settings before retrying.");
            }
        }

        // Adds source and destination site info to migration status file 
        // which is used when detecting and resuming a previous migration
        private Result InitializeMigrationStatusFile()
        {
            string statusFilePath = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
            if (this._previousMigrationStatus == null || this._previousMigrationStatus.Length == 0)
            {
                if (File.Exists(statusFilePath))
                {
                    File.Delete(statusFilePath);
                }
                File.Create(statusFilePath).Dispose();
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteName + this._sourceSiteInfo.webAppName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteResourceGroup + this._sourceSiteInfo.resourceGroupName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteSubscription + this._sourceSiteInfo.subscriptionId + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteName + this._destinationSiteInfo.webAppName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteResourceGroup + this._destinationSiteInfo.resourceGroupName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteSubscription + this._destinationSiteInfo.subscriptionId + Environment.NewLine);
            }
            else
            {
                if (!File.Exists(statusFilePath))
                {
                    return new Result(Status.Failed, "Could not find a migration statusfile.");
                }
            }
            return new Result(Status.Completed, "");
        }

        public void MigrateAsyncForWinUI()
        {
            try
            {
                Result res = this.Migrate();
                HelperUtils.WriteOutputWithNewLine(res.message, this._progressViewRTextBox);

                // logs Migration status
                string logMessage = String.Format("({0}; {1}; {2}; {3}; {4}; {5}; {6}; {7})", (res.status == Status.Completed ? "MIGRATION_SUCCESSFUL" : "MIGRATION_FAILED"), 
                    this._sourceSiteInfo.webAppName, this._sourceSiteInfo.subscriptionId, this._sourceSiteInfo.resourceGroupName, this._destinationSiteInfo.webAppName, 
                    this._destinationSiteInfo.subscriptionId, this._destinationSiteInfo.resourceGroupName, res.message);
                this.LogMigrationStatusMessage(logMessage);
                
                
                if (res.status == Status.Failed || res.status == Status.Cancelled)
                {
                    MessageBox.Show(res.message, "Failed!");
                }
                else
                {
                    MessageBox.Show(res.message, "Success!");
                }
            }
            catch (Exception ex)
            {
                HelperUtils.WriteOutputWithNewLine(ex.Message, this._progressViewRTextBox);
                MessageBox.Show(ex.Message, "Failed!");
            }
        }

        // Sends a dummy Kudu api with input log message embedded in userAgent field
        // this kudu api call is logged into Kusto.
        private void LogMigrationStatusMessage(string logMessage)
        {
            KuduCommandApiResult result = HelperUtils.ExecuteKuduCommandApi(String.Format(Constants.LIST_DIR_COMMAND, "/home"), this._destinationSiteInfo.ftpUsername, this._destinationSiteInfo.ftpPassword, this._destinationSiteInfo.webAppName, message: logMessage);
            System.Diagnostics.Debug.WriteLine(String.Format("logging status is {0}; exitcode is {1}; output is {2}", result.status.ToString(), result.exitCode.ToString(), result.output));
        }

        // clears local migration data
        private void CleanLocalTempFiles()
        {
            string localDataExportDirectory = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            HelperUtils.RecursiveDeleteDirectory(localDataExportDirectory);
        }

        // Clears /home/dev/migrate directory of destination app service
        private Result CleanDestinationAppTempFiles(SiteInfo destinationSite)
        {
            return HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, destinationSite.ftpUsername,
                destinationSite.ftpPassword, destinationSite.webAppName);
        }
    }
}