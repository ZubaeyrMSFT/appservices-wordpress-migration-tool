﻿using Azure.ResourceManager.AppService;
using System;
using System.Collections.Generic;
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

            WebSiteResource webAppResource = AzureManagementUtils.GetWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
            IDictionary<string, string> applicationSettings = AzureManagementUtils.GetApplicationSettingsForAppService(webAppResource);
            PublishingUserData publishingProfile = AzureManagementUtils.GetPublishingCredentialsForAppService(webAppResource);

            destinationSite.ftpUsername = publishingProfile.PublishingUserName;
            destinationSite.ftpPassword = publishingProfile.PublishingPassword;
            destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
            destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
            destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
            destinationSite.databaseName = newDatabaseName;

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

        private Result ImportAppServiceData(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return null;
        }

        private Result ImportDatbaseContent(SiteInfo destinationSite)
        {
            //TODO Needs to be implemented
            return null;
        }
    }
}
