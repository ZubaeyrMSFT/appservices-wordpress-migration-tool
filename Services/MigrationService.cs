using System;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

        private SiteInfo _sourceSiteInfo;
        private SiteInfo _destinationSiteInfo;
        private RichTextBox? _progressViewRTextBox;

        public MigrationService(SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo, RichTextBox? progressViewRTextBox) { 
            this._sourceSiteInfo = sourceSiteInfo;
            this._destinationSiteInfo = destinationSiteInfo;
            this._progressViewRTextBox = progressViewRTextBox;
        }

        public Result Migrate()
        {
            try
            {
                ExportService exportService = new ExportService(this._progressViewRTextBox);
                ImportService importService = new ImportService(this._progressViewRTextBox);

                Result exporttRes = exportService.ExportDataFromSourceSite(this._sourceSiteInfo);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    this.CleanLocalTempFiles();
                    this.CleanDestinationAppTempFiles(this._destinationSiteInfo);
                    return exporttRes;
                }

                Result importRes = importService.ImportDataToDestinationSite(this._destinationSiteInfo, this._sourceSiteInfo.databaseName);
                if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
                {
                    this.CleanLocalTempFiles();
                    this.CleanDestinationAppTempFiles(this._destinationSiteInfo);
                    return importRes;
                }

                this.CleanLocalTempFiles();
                Result cleanupRes = this.CleanDestinationAppTempFiles(this._destinationSiteInfo);
                if (cleanupRes.status == Status.Failed || cleanupRes.status == Status.Cancelled)
                {
                    return cleanupRes;
                }

                return new Result(Status.Completed, Constants.SUCCESS_MESSAGE);
            }
            catch (Exception ex) {
                this.CleanLocalTempFiles();
                this.CleanDestinationAppTempFiles(this._destinationSiteInfo);
                return new Result(Status.Failed, ex.Message);
            }
        }

        public void MigrateAsyncForWinUI()
        {
            try
            {
                Result res = this.Migrate();
                if (res.status == Status.Failed || res.status == Status.Cancelled)
                {
                    HelperUtils.WriteOutputWithNewLine(res.message, this._progressViewRTextBox);
                    MessageBox.Show(res.message, "Failed!");
                }
            }
            catch (Exception ex)
            {
                HelperUtils.WriteOutputWithNewLine(ex.Message, this._progressViewRTextBox);
                MessageBox.Show(ex.Message, "Failed!");
            }
        }

        private void CleanLocalTempFiles()
        {
            HelperUtils.WriteOutputWithNewLine("Cleaning up intermediate files on local machine.", this._progressViewRTextBox);
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
            if (Directory.Exists(localDataExportDirectory))
            {
                Directory.Delete(localDataExportDirectory, true);
            }
        }

        private Result CleanDestinationAppTempFiles(SiteInfo destinationSite)
        {
            HelperUtils.WriteOutputWithNewLine("Cleaning up intermediate files on Linux App Service.", this._progressViewRTextBox);
            return HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, destinationSite.ftpUsername, 
                destinationSite.ftpPassword, destinationSite.webAppName);
        }
    }
}
