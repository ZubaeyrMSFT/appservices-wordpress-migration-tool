using Azure.ResourceManager.AppService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ImportService
    {
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

            try
            {
                Console.WriteLine("Retrieving WebApp publishing profile and database details for Linux WordPress... ");
                Stopwatch timer = Stopwatch.StartNew();

                WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
                IDictionary<string, string> applicationSettings = AzureManagementUtils.GetApplicationSettingsForAppService(webAppResource);
                PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);

                destinationSite.ftpUsername = publishingProfile.PublishingUserName;
                destinationSite.ftpPassword = publishingProfile.PublishingPassword;
                destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
                destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
                destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
                destinationSite.databaseName = newDatabaseName;

                Console.WriteLine("Successfully retrieved the details... time taken={0} seconds\n",
                    (timer.ElapsedMilliseconds / 1000));
                timer.Stop();


                Result result = ImportAppServiceData(destinationSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }

                result = ImportDatbaseContent(destinationSite);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }

                AzureManagementUtils.UpdateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME,
                    destinationSite.databaseName);

                webAppResource.Restart();
                return new Result(Status.Completed, Constants.SUCCESS_IMPORT_MESSAGE);

            }
            catch (Exception ex)
            {
                return new Result(Status.Failed, ex.Message);
            }
        }

        private Result ImportAppServiceData(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return new Result(Status.Failed, "Unable to import the file data to Linux App Services."); ;
        }

        private Result ImportDatbaseContent(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return new Result(Status.Failed, "Unable to import the MySQL data to Linux WordPress."); ;
        }
    }
}
