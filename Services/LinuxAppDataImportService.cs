using WordPressMigrationTool.Utilities;
using System.Diagnostics;
using System.Text;
namespace WordPressMigrationTool
{
    public class LinuxAppDataImportService
    {
        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private RichTextBox? _progressViewRTextBox;
        private string[] _previousMigrationStatus;
        private string _migrationStatusFilePath;

        public LinuxAppDataImportService(string appServiceName, string ftpUserName, string ftpPassword, RichTextBox? progressViewRTextBox, string[] previousMigrationStatus)
        {
            if (string.IsNullOrWhiteSpace(appServiceName))
            {
                throw new ArgumentException("Invalid AppService name found! " +
                    "appServiceName=", appServiceName);
            }

            if (string.IsNullOrWhiteSpace(ftpUserName))
            {
                throw new ArgumentException("Invalid FTP username found! " +
                    "ftpUsername=" + ftpUserName);
            }

            if (string.IsNullOrWhiteSpace(appServiceName))
            {
                throw new ArgumentException("Invalid FTP password found! " +
                    "ftpPassword=" + ftpPassword);
            }

            this._appServiceName = appServiceName;
            this._ftpUserName = ftpUserName;
            this._ftpPassword = ftpPassword;
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
            this._migrationStatusFilePath = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
        }

        public Result ImportData()
        {
            Stopwatch timer = Stopwatch.StartNew();
            HelperUtils.WriteOutputWithNewLine("Preparing to upload wp-content data to the destination site...", this._progressViewRTextBox);
            
            Result result = HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_WPCONTENT_DIR, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._SplitWpContentZip();
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._UploadSplitZipFiles();
            if (result.status != Status.Completed)
            {
                return result;
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully uploaded wp-content data to Linux App Service... " +
                "time taken= " + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

            return new Result(Status.Completed, "Successfully uploaded App Service data to destination site.");
        }

        // Uploads App data split-zip files to destination app
        private Result _UploadSplitZipFiles()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.uploadAppDataSplitZipFilesCompleted))
            {
                return new Result(Status.Completed, "");
            }

            string appContentSplitZipDir = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR);
            string[] splitZipFilesArr = Directory.GetFiles(appContentSplitZipDir);
            if (splitZipFilesArr.Length == 0)
            {
                return new Result(Status.Failed, "Could not find splitted zip data at " + appContentSplitZipDir);
            }

            HelperUtils.WriteOutputWithRC("App data upload progress - Finished uploading 0 out of "
                + splitZipFilesArr.Length + " files.", this._progressViewRTextBox);

            for (int splitInd = 0; splitInd < splitZipFilesArr.Length; splitInd++)
            {
                string splitZipFileName = Path.GetFileName(splitZipFilesArr[splitInd]);
                Result result = this._UploadSplitZipFileToAppService(splitZipFileName);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }

                HelperUtils.WriteOutputWithRC("App data upload progress - Finished uploading " + (splitInd + 1) + " out of " 
                    + splitZipFilesArr.Length + " files." + ((splitInd + 1 == splitZipFilesArr.Length) ? "\n" : ""), 
                    this._progressViewRTextBox);
            }

            File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.uploadAppDataSplitZipFilesCompleted + Environment.NewLine);
            return new Result(Status.Completed, "App data split zip files uploaded successfully...");
        }

        // Uploads given split-zip file to destination app
        private Result _UploadSplitZipFileToAppService(string splitZipFileName)
        {
            if (this._previousMigrationStatus.Contains(String.Format(Constants.StatusMessages.uploadAppDataSplitZipFileCompleted, splitZipFileName)))
            {
                return new Result(Status.Completed, "");
            }

            string zippedSplitZipFilesDir = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_NESTED_DIR);
            string zippedFileToUpload = zippedSplitZipFilesDir + splitZipFileName.Replace(".", "") + ".zip";
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR + splitZipFileName);
            string uploadWpContentKuduUrl = HelperUtils.GetKuduApiForZipUpload(this._appServiceName, Constants.WPCONTENT_TEMP_DIR_KUDU_API);

            if (Directory.Exists(zippedSplitZipFilesDir))
            {
                Directory.Delete(zippedSplitZipFilesDir, true);
            }
            Directory.CreateDirectory(zippedSplitZipFilesDir);
            
            var zipFile = new Ionic.Zip.ZipFile(Encoding.UTF8);
            zipFile.AddFile(splitZipFilePath, "");
            zipFile.Save(zippedFileToUpload);

            Result result =  HelperUtils.LinuxAppServiceUploadZip(zippedFileToUpload, 
                uploadWpContentKuduUrl, this._ftpUserName, this._ftpPassword);

            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, String.Format(Constants.StatusMessages.uploadAppDataSplitZipFileCompleted, splitZipFileName) + Environment.NewLine);
            }

            return result;
        }

        // Splits exported App data zip into smaller split-zip files of ~25MB each
        private Result _SplitWpContentZip()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.splitWpContentZipCompleted))
            {
                return new Result(Status.Completed, "");
            }

            HelperUtils.WriteOutputWithNewLine("Splitting App data zip file...", this._progressViewRTextBox);
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string splitZipFilesDirectory = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR);
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILE_PATH);

            if (Directory.Exists(splitZipFilesDirectory))
            {
                Directory.Delete(splitZipFilesDirectory, true);
            }

            try
            {
                Directory.CreateDirectory(splitZipFilesDirectory);
                using (var zipFile = Ionic.Zip.ZipFile.Read(appContentFilePath))
                {
                    zipFile.MaxOutputSegmentSize = Constants.KUDU_ZIP_API_MAX_UPLOAD_LIMIT;
                    zipFile.Save(splitZipFilePath);
                }

                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.splitWpContentZipCompleted + Environment.NewLine);
                return new Result(Status.Completed, "App data zip file splitted successful...");
            }
            catch (Exception ex) 
            {
                return new Result(Status.Failed, "Couldn't split App data zip file. Error=" + ex.Message);
            }
        }
    }
}
