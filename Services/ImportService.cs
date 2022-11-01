using Azure.ResourceManager.AppService;
using System;
using System.Collections.Generic;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        public Result importDataToDestinationSite(SiteInfo destinationSite, string newDatabaseName) {
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

            WebSiteResource webAppResource = AzureManagementUtils.getWebSiteResource(destinationSite.subscriptionId, destinationSite.resourceGroupName, destinationSite.webAppName);
            IDictionary<string, string> applicationSettings = AzureManagementUtils.getApplicationSettingsForAppService(webAppResource);
            PublishingUserData publishingProfile = AzureManagementUtils.getPublishingCredentialsForAppService(webAppResource);

            destinationSite.ftpUsername = publishingProfile.PublishingUserName;
            destinationSite.ftpPassword = publishingProfile.PublishingPassword;
            destinationSite.databaseHostname = applicationSettings[Constants.APPSETTING_DATABASE_HOST];
            destinationSite.databaseUsername = applicationSettings[Constants.APPSETTING_DATABASE_USERNAME];
            destinationSite.databasePassword = applicationSettings[Constants.APPSETTING_DATABASE_PASSWORD];
            destinationSite.databaseName = newDatabaseName;

            Result result = splitWpContentZip(destinationSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            Result result = importAppServiceData(destinationSite);
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            result = importDatabaseContent(destinationSite);
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
            LinuxAppServiceImportService linAppImportService = new LinuxAppServiceImportService(destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword);
            return linAppImportService.importData();
        }

        private Result importDatabaseContent(SiteInfo destinationSite)
        {
            WindowsMySQLDataExportService linDBImportService = new WindowsMySQLDataExportService(destinationSite.databaseHostname,
                destinationSite.databaseUsername, destinationSite.databasePassword, destinationSite.databaseName, null, destinationSite.webAppName,
                destinationSite.ftpUsername, destinationSite.ftpPassword);
            
            return linDBImportService.importData();
        }

        private Result splitWpContentZip(SiteInfo destinationSite)
        {
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string splitZipFilesDirectory = Constants.SPLIT_ZIP_FILES_DIR;
            if (!Directory.Exists(splitZipFilesDirectory))
            {
                Directory.CreateDirectory(splitZipFilesDirectory);
            }
            try 
            {
                using (var zipFile = new Ionic.Zip.ZipFile(Encoding.UTF8))
                {
                    zipFile.AddDirectory(appContentFilePath, directoryPathInArchive: string.Empty);
                    zipFile.MaxOutputSegmentSize = 100 * 1000000;
                    zipFile.Save(splitZipFilesDirectory + "test.zip");
                }
                return Result(Status.Completed, "Zip file split successful...");
            }
            catch (Exception e)
            {
                return Result(Status.Failed, "Couldn't split zip file...");
            }
           
        }
    }
}
