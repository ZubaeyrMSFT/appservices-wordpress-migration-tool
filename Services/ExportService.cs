using Azure.ResourceManager.AppService;
using System;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ExportService
    {

        public Result exportDataFromSourceSite(SiteInfo sourceSite)
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


            Console.WriteLine("Retreiving publishing profile and database connection details ... ");
            Stopwatch timer = Stopwatch.StartNew();

            WebSiteResource webAppResource = AzureManagementUtils.getWebSiteResource(sourceSite.subscriptionId, sourceSite.resourceGroupName, sourceSite.webAppName);
            PublishingUserData publishingProfile = AzureManagementUtils.getPublishingCredentialsForAppService(webAppResource);
            string databaseConnectionString = AzureManagementUtils.getDatabaseConnectionString(webAppResource);
            HelperUtils.parseAndUpdateDatabaseConnectionStringForWinAppService(sourceSite, databaseConnectionString);
            sourceSite.ftpUsername = publishingProfile.PublishingUserName;
            sourceSite.ftpPassword = publishingProfile.PublishingPassword;

            Console.WriteLine("Successfully retrieved the details... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            Console.WriteLine("Exporting App Service data to " + Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH));
            timer.Restart();

            Result result = exportAppServiceData(sourceSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            Console.WriteLine("Successfully exported App Service data... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            Console.WriteLine("Exporting MySql database dump to " + Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH));
            timer.Restart();

            result = exportDatbaseContent(sourceSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            Console.WriteLine("Successfully exported MySQL database dump... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, Constants.SUCCESS_EXPORT_MESSAGE);
        }

        private Result exportAppServiceData(SiteInfo sourceSite)
        {
            WindowsAppDataExportService winAppExpService = new WindowsAppDataExportService(sourceSite.webAppName,
                sourceSite.ftpUsername, sourceSite.ftpPassword);
            return winAppExpService.exportData();
        }

        private Result exportDatbaseContent(SiteInfo sourceSite)
        {
            WindowsMySQLDataExportService winDBExpService = new WindowsMySQLDataExportService(sourceSite.databaseHostname,
                sourceSite.databaseUsername, sourceSite.databasePassword, sourceSite.databaseName, null);
            return winDBExpService.exportData();
        }
    }
}
