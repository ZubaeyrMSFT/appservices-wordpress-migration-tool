using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace WordPressMigrationTool.Utilities
{
    public static class AzureManagementUtils
    {
        public static WebSiteResource GetWebSiteResource(string subscriptionId, string resourceGroupName, string webAppName, DefaultAzureCredential azureCredential)
        {
            ArmClient client = new ArmClient(azureCredential);
            if (client == null)
            {
                throw new InvalidCredentialException("Unable to authenticated to Azure Services");
            }

            SubscriptionCollection subscriptions = client.GetSubscriptions();
            SubscriptionResource subscription = subscriptions.Get(subscriptionId);
            if (subscription == null)
            {
                throw new ArgumentException("Could not find the subscription " + subscriptionId);
            }
           
            ResourceGroupCollection resourceGroups = subscription.GetResourceGroups();
            ResourceGroupResource resourceGroup = resourceGroups.Get(resourceGroupName);
            if (resourceGroup == null)
            {
                throw new ArgumentException("Could not find the resource group " + resourceGroupName);
            }

            WebSiteCollection webSites = resourceGroup.GetWebSites();
            WebSiteResource webSite = webSites.Get(webAppName);
            if (webSite == null)
            {
                throw new ArgumentException("Could not find the app service resource " + webAppName);
            }

            return webSite;
        }

        public static SubscriptionCollection GetSubscriptions(DefaultAzureCredential azureCredential)
        {
            ArmClient client = new ArmClient(azureCredential);
            if (client == null)
            {
                throw new InvalidCredentialException("Unable to authenticated to Azure Services");
            }

            return client.GetSubscriptions();
        }

        public static List<String> GetResourceGroupsInSubscription(string subscriptionId, SubscriptionCollection subscriptions)
        {
            SubscriptionResource subscription = subscriptions.Get(subscriptionId);
            ResourceGroupCollection resourceGroups = subscription.GetResourceGroups();
            List<string> resourceGroupNames = new List<string>();
            
            foreach(ResourceGroupResource resourceGroup in resourceGroups)
            {
                if (resourceGroup == null || !resourceGroup.HasData)
                    continue;
                resourceGroupNames.Add(resourceGroup.Data.Name);
            }

            return resourceGroupNames;
        }

        public static List<string> GetAppServicesInResourceGroup(string subscriptionId, string resourceGroupName, SubscriptionCollection subscriptions)
        {
            SubscriptionResource subscription = subscriptions.Get(subscriptionId);
            ResourceGroupResource resourceGroup = subscription.GetResourceGroup(resourceGroupName);
            WebSiteCollection webSites = resourceGroup.GetWebSites();
            List<string> webSiteNames = new List<string>();

            foreach (WebSiteResource webSite in webSites)
            {
                if (webSite == null || !webSite.HasData)
                    continue;
                webSiteNames.Add(webSite.Data.Name);
            }

            return webSiteNames;
        }


        public static string GetDatabaseConnectionString(WebSiteResource webSiteResource)
        {
            Response<ConnectionStringDictionary> connStrDictionary = webSiteResource.GetConnectionStrings();
            if (connStrDictionary == null || connStrDictionary.Value == null || connStrDictionary.Value.Properties == null)
            {
                throw new ArgumentException("Could not find MySQL database connection string in App Service.");
            }

            string databaseStringKey = "defaultConnection";
            IDictionary<string, ConnStringValueTypePair> connStrValue = connStrDictionary.Value.Properties;
            if (connStrValue == null || !connStrValue.ContainsKey(databaseStringKey) || connStrValue[databaseStringKey] == null ||
                string.IsNullOrWhiteSpace(connStrValue[databaseStringKey].Value) || !connStrValue[databaseStringKey].ConnectionStringType.Equals(ConnectionStringType.MySql))
            {
                throw new ArgumentException("Could not find MySQL database connection string in App Service.");
            }

            return connStrValue[databaseStringKey].Value;
        }

        public static PublishingUserData GetPublishingCredentialsForAppService(WebSiteResource webSiteResource)
        {
            ArmOperation<PublishingUserResource> publishingResource = webSiteResource.GetPublishingCredentials(WaitUntil.Completed);
            if (publishingResource == null || publishingResource.Value == null || publishingResource.Value.Data == null 
                || string.IsNullOrEmpty(publishingResource.Value.Data.PublishingUserName) || string.IsNullOrEmpty(publishingResource.Value.Data.PublishingPassword))
            {
                throw new ArgumentException("Could not find publishing credentials for App Service.");
            }

            return publishingResource.Value.Data;
        }

        public static IDictionary<string, string> GetApplicationSettingsForAppService(WebSiteResource webSiteResource)
        {
            Response<AppServiceConfigurationDictionary> appSettings = webSiteResource.GetApplicationSettings();
            if (appSettings == null || appSettings.Value == null || appSettings.Value.Properties == null)
            {
                throw new ArgumentException("Could not find application settings for App Service.");
            }

            return new Dictionary<string, string>(appSettings.Value.Properties);
        }

        public static IDictionary<string, string> GetWebSiteApplicationSettings(WebSiteResource webSiteResource)
        {
            return webSiteResource.GetApplicationSettings().Value.Properties;
        }

        public static bool UpdateApplicationSettingForAppService(WebSiteResource webSiteResource, string appSettingKey, string appSettingValue)
        {
            Response<AppServiceConfigurationDictionary> appSettings = webSiteResource.GetApplicationSettings();
            if (appSettings == null || appSettings.Value == null || appSettings.Value.Properties == null)
            {
                throw new ArgumentException("Unable to configure application settings for App Service.");
            }

            appSettings.Value.Properties.Remove(appSettingKey);
            appSettings.Value.Properties.Add(appSettingKey, appSettingValue);
            webSiteResource.UpdateApplicationSettings(appSettings);
            return true;
        }


        public static bool UpdateApplicationSettingForAppService(WebSiteResource webSiteResource, IDictionary<string, string> inputAppSettings)
        {
            Response<AppServiceConfigurationDictionary> appSettings = webSiteResource.GetApplicationSettings();
            if (appSettings == null || appSettings.Value == null || appSettings.Value.Properties == null)
            {
                throw new ArgumentException("Unable to configure application settings for App Service.");
            }

            foreach (KeyValuePair<string, string> keyValuePair in inputAppSettings) {
                appSettings.Value.Properties.Remove(keyValuePair.Key);
                appSettings.Value.Properties.Add(keyValuePair.Key, keyValuePair.Value);
            }
            webSiteResource.UpdateApplicationSettings(appSettings);
            return true;
        }

        public static bool RemoveApplicationSettingForAppService(WebSiteResource webSiteResource, string[] appSettingsToRemove)
        {
            Response<AppServiceConfigurationDictionary> appSettings = webSiteResource.GetApplicationSettings();
            if (appSettings == null || appSettings.Value == null || appSettings.Value.Properties == null)
            {
                throw new ArgumentException("Unable to configure application settings for App Service.");
            }

            foreach (string appSettingToRemove in appSettingsToRemove)
            {
                appSettings.Value.Properties.Remove(appSettingToRemove);
            }
            webSiteResource.UpdateApplicationSettings(appSettings);
            return true;
        }
    }
}
