﻿using Microsoft.VisualBasic;
using System;
using WordPressMigrationTool.Utilities;
using Constants = WordPressMigrationTool.Utilities.Constants;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

        private SiteInfo _sourceSiteInfo;
        private SiteInfo _destinationSiteInfo;
        private RichTextBox? _progressViewRTextBox;
        private string[] _previousMigrationStatus;

        public MigrationService(SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo, RichTextBox? progressViewRTextBox, string[] previousMigrationStatus) { 
            this._sourceSiteInfo = sourceSiteInfo;
            this._destinationSiteInfo = destinationSiteInfo;
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
        }

        public Result Migrate()
        {
            try
            {
                Result result = this.InitializeMigrationStatusFile();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                ExportService exportService = new ExportService(this._progressViewRTextBox, this._previousMigrationStatus);
                ImportService importService = new ImportService(this._progressViewRTextBox, this._previousMigrationStatus);

                Result exporttRes = exportService.ExportDataFromSourceSite(this._sourceSiteInfo);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    return exporttRes;
                }

                Result importRes = importService.ImportDataToDestinationSite(this._destinationSiteInfo, this._sourceSiteInfo.databaseName);
                if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
                {
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
                return new Result(Status.Failed, ex.Message);
            }
        }

        private Result InitializeMigrationStatusFile()
        {
            string statusFilePath = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
            if (this._previousMigrationStatus == null || this._previousMigrationStatus.Length == 0)
            {
                if (File.Exists(statusFilePath))
                {
                    File.Delete(statusFilePath);
                }
                File.Create(statusFilePath).Dispose();
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteName + this._sourceSiteInfo.webAppName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteResourceGroup + this._sourceSiteInfo.resourceGroupName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.sourceSiteSubscription + this._sourceSiteInfo.subscriptionId + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteName + this._destinationSiteInfo.webAppName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteResourceGroup + this._destinationSiteInfo.resourceGroupName + Environment.NewLine);
                File.AppendAllText(statusFilePath, Constants.StatusMessages.destinationSiteSubscription + this._destinationSiteInfo.subscriptionId + Environment.NewLine);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("previousmigrationstatus exists");
                if (!File.Exists(statusFilePath))
                {
                    return new Result(Status.Failed, "Could not find a migration statusfile.");
                }
            }
            return new Result(Status.Completed, "");
        }

        public void MigrateAsyncForWinUI()
        {
            try
            {
                Result res = this.Migrate();
                HelperUtils.WriteOutputWithNewLine(res.message, this._progressViewRTextBox);
                if (res.status == Status.Failed || res.status == Status.Cancelled)
                {
                    MessageBox.Show(res.message, "Failed!");
                }
                else
                {
                    MessageBox.Show(res.message, "Success!");
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
            return HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_MIGRATE_DIR, destinationSite.ftpUsername,
                destinationSite.ftpPassword, destinationSite.webAppName);
        }
    }
}
