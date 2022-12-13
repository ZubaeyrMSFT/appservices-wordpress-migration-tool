using Azure.ResourceManager.AppService;
using System;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ExportService
    {

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
                Console.WriteLine("Retrieving WebApp publishing profile and database details for Windows WordPress... ");
                Stopwatch timer = Stopwatch.StartNew();

                WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(sourceSite.subscriptionId, sourceSite.resourceGroupName, sourceSite.webAppName);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);
                string databaseConnectionString = AzureManagementUtils.GetDatabaseConnectionString(webAppResource);

                HelperUtils.ParseAndUpdateDatabaseConnectionStringForWinAppService(sourceSite, databaseConnectionString);
                sourceSite.ftpUsername = publishingProfile.PublishingUserName;
                sourceSite.ftpPassword = publishingProfile.PublishingPassword;

                Console.WriteLine("Successfully retrieved the details... time taken={0} seconds\n",
                    (timer.ElapsedMilliseconds / 1000));
                timer.Stop();


                Result result = ExportAppServiceData(sourceSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }

                result = ExportDatbaseContent(sourceSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
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
            WindowsAppDataExportService winAppExpService = new WindowsAppDataExportService(sourceSite.webAppName,
                sourceSite.ftpUsername, sourceSite.ftpPassword);
            return winAppExpService.ExportData();
        }

        private Result ExportDatbaseContent(SiteInfo sourceSite)
        {
            WindowsMySQLDataExportService winDBExpService = new WindowsMySQLDataExportService(sourceSite.databaseHostname,
                sourceSite.databaseUsername, sourceSite.databasePassword, sourceSite.databaseName, null);
            return winDBExpService.ExportData();
        }
    }
}
