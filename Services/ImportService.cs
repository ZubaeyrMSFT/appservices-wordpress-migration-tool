using Azure.ResourceManager.AppService;
using System;
using System.Collections.Generic;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        public Result importDataToDestinationSite(SiteInfo sourceSite, SiteInfo destinationSite) {
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

            if (string.IsNullOrWhiteSpace(sourceSite.databaseName))
            {
                return new Result(Status.Failed, "Final database name should not be empty!");
            }

            WebSiteResource webAppResource = AzureManagementUtils.getWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
            IDictionary<string, string> applicationSettings = AzureManagementUtils.getApplicationSettingsForAppService(webAppResource);
            PublishingUserData publishingProfile = AzureManagementUtils.getPublishingCredentialsForAppService(webAppResource);

            destinationSite.ftpUsername = publishingProfile.PublishingUserName;
            destinationSite.ftpPassword = publishingProfile.PublishingPassword;
            destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
            destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
            destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
            destinationSite.databaseName = sourceSite.databaseName; //Database to be exported to Linux Site

            Result result = importAppServiceData(destinationSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            result = importDatbaseContent(destinationSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            AzureManagementUtils.updateApplicationSettingForAppService(webAppResource, Constants.APPSETTING_DATABASE_NAME, 
                destinationSite.databaseName);

            webAppResource.Restart();
            return new Result(Status.Completed, Constants.SUCCESS_IMPORT_MESSAGE);
        }

        private Result importAppServiceData(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return null;
        }

        private Result importDatbaseContent(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return null;
        }
    }
}
