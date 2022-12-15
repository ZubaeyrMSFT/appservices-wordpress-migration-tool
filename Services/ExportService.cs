using Azure.ResourceManager.AppService;
using MySqlX.XDevAPI.Common;
using System;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ExportService
    {
        private RichTextBox? _progressViewRTextBox;
        private string[] _previousMigrationStatus;

        public ExportService() { }

        public ExportService(RichTextBox? progressViewRTextBox, string[] previousMigrationStatus)
        {
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
        }

        public Result ExportDataFromSourceSite(SiteInfo sourceSite)
        {
            if (string.IsNullOrWhiteSpace(sourceSite.subscriptionId))
            {
                return new Result(Status.Failed, "Subscription Id should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.resourceGroupName))
            {
                return new Result(Status.Failed, "Resource Group should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.webAppName))
            {
                return new Result(Status.Failed, "App Service name should not be empty!");
            }

            try
            {

                string migrationStatusFile = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);

                HelperUtils.WriteOutputWithNewLine("Retrieving WebApp publishing profile and database details " +
                    "for Windows WordPress... ", this._progressViewRTextBox);
                Stopwatch timer = Stopwatch.StartNew();

                WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(sourceSite.subscriptionId, sourceSite.resourceGroupName, sourceSite.webAppName);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);
                string databaseConnectionString = AzureManagementUtils.GetDatabaseConnectionString(webAppResource);
                HelperUtils.ParseAndUpdateDatabaseConnectionStringForWinAppService(sourceSite, databaseConnectionString);
                sourceSite.ftpUsername = publishingProfile.PublishingUserName;
                sourceSite.ftpPassword = publishingProfile.PublishingPassword;

                if (this._previousMigrationStatus.Contains(Constants.StatusMessages.exportCompleted))
                {
                    HelperUtils.WriteOutputWithNewLine("Source Site Data downloaded in previous migration.", this._progressViewRTextBox);
                    return new Result(Status.Completed, Constants.SUCCESS_EXPORT_MESSAGE);
                }

                HelperUtils.WriteOutputWithNewLine("Successfully retrieved the details... time taken=" + (timer.ElapsedMilliseconds / 1000) 
                    + " seconds\n", this._progressViewRTextBox);
                timer.Stop();

                Result result = ExportAppServiceData(sourceSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }
                System.Diagnostics.Debug.WriteLine("after export app data");
                result = ExportDatbaseContent(sourceSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }
                System.Diagnostics.Debug.WriteLine("after export database data");
                if (!this._previousMigrationStatus.Contains(Constants.StatusMessages.exportCompleted))
                {
                    File.AppendAllText(migrationStatusFile, Constants.StatusMessages.exportCompleted + Environment.NewLine);
                }
                return new Result(Status.Completed, Constants.SUCCESS_EXPORT_MESSAGE);

            } 
            catch (Exception ex)
            {
                return new Result(Status.Failed, ex.Message);
            }
        }

        private Result ExportAppServiceData(SiteInfo sourceSite)
        {
            string migrationStatusFile = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.exportAppDataCompleted))
            {
                HelperUtils.WriteOutputWithNewLine("Source Site App Data downloaded in previous migration.", this._progressViewRTextBox);
                return new Result(Status.Completed, "App Service Data exported in previous migration attempt.");
            }

            WindowsAppDataExportService winAppExpService = new WindowsAppDataExportService(sourceSite.webAppName,
                sourceSite.ftpUsername, sourceSite.ftpPassword, this._progressViewRTextBox);

            Result result = winAppExpService.ExportData();
            if (result.status == Status.Completed)
            {
                System.Diagnostics.Debug.WriteLine("before appendall error");
                File.AppendAllText(migrationStatusFile, Constants.StatusMessages.exportAppDataCompleted + Environment.NewLine);
                System.Diagnostics.Debug.WriteLine("after appendall error");
            }
            return result;
        }

        private Result ExportDatbaseContent(SiteInfo sourceSite)
        {
            string migrationStatusFile = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.exportDbDataCompleted))
            {
                HelperUtils.WriteOutputWithNewLine("Source Site Database Data downloaded in previous migration.", this._progressViewRTextBox);
                return new Result(Status.Completed, "Database exported in previous migration attempt.");
            }
            System.Diagnostics.Debug.WriteLine("starting db export");
            WindowsMySQLDataExportService winDBExpService = new WindowsMySQLDataExportService(sourceSite.databaseHostname,
                sourceSite.databaseUsername, sourceSite.databasePassword, sourceSite.databaseName, null, 
                this._progressViewRTextBox);
            Result result = winDBExpService.ExportData();
            if (result.status == Status.Completed)
            {
                File.AppendAllText(migrationStatusFile, Constants.StatusMessages.exportDbDataCompleted + Environment.NewLine);
            }
            return result;
        }
    }
}
