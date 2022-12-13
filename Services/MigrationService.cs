using System;
using WordPressMigrationTool.Utilities;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

        public Result migrate(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            try
            {
                ImportService importService = new ImportService();
                ExportService exportService = new ExportService();

                Result exporttRes = exportService.ExportDataFromSourceSite(sourceSite);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    return exporttRes;
                }

                Result importRes = importService.importDataToDestinationSite(destinationSite, sourceSite.databaseName);
                if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
                {
                    return importRes;
                }

                this.cleanLocalTempFiles();

                if (!this.cleanDestinationAppTempFiles(destinationSite))
                {
                    return new Result(Status.Failed, "Could not clean intermediary files on Destination site...");
                }

                return new Result(Status.Completed, Constants.SUCCESS_MESSAGE);
            }
            catch (Exception ex) {
                return new Result(Status.Failed, ex.Message);
            }
        }

        private void cleanLocalTempFiles()
        {
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

            string localDataExportDirectory = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            if(Directory.Exists(localDataExportDirectory))
            {
                Directory.Delete(localDataExportDirectory, true);
            }            
        }

        private bool cleanDestinationAppTempFiles(SiteInfo destinationSite)
        {
            string linAppMigrateIntermerdiateDirectory = Constants.LIN_APP_SVC_MIGRATE_DIR;
            return HelperUtils.ClearAppServiceDirectory(linAppMigrateIntermerdiateDirectory, destinationSite.ftpUsername, destinationSite.ftpPassword, destinationSite.webAppName);
        }

    }
}
