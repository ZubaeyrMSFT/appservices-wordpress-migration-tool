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

        public LinuxAppDataImportService(string appServiceName, string ftpUserName, string ftpPassword, RichTextBox? progressViewRTextBox)
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
        }

        public Result ImportData()
        {
            string uploadWpContentKuduUrl = HelperUtils.GetKuduApiForZipUpload(this._appServiceName, "site/wwwroot/wp-content");
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);

            Stopwatch timer = Stopwatch.StartNew();
            HelperUtils.WriteOutputWithNewLine("Preparing to upload App data to the destination site...", this._progressViewRTextBox);
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

            result = this._ProcessSplitZipFiles();
            if (result.status != Status.Completed)
            {
                return result;
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully uploaded App Service data to Linux App Service... " +
                "time taken= " + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

            return new Result(Status.Completed, "Successfully uploaded App Service data to destination site.");
        }

        private Result _UploadSplitZipFiles()
        {
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

            return new Result(Status.Completed, "App data split zip files uploaded successfully...");
        }

        private Result _UploadSplitZipFileToAppService(string splitZipFileName)
        {
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

            return HelperUtils.LinuxAppServiceUploadZip(zippedFileToUpload, 
                uploadWpContentKuduUrl, this._ftpUserName, this._ftpPassword);
        }

        private Result _ProcessSplitZipFiles()
        {
            string message = "Unable to porocess the uploaded App data on Linux App Services.";
            HelperUtils.WriteOutputWithNewLine("Procesing uploaded App data on Linux App Service...", this._progressViewRTextBox);

            string mergeSplitZipCommand = Constants.WPCONTENT_MERGE_SPLLIT_FILES_COMAMND;
            KuduCommandApiResult mergeSplitZipResult = HelperUtils.ExecuteKuduCommandApi(mergeSplitZipCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (mergeSplitZipResult.status != Status.Completed)
            {
                return new Result(Status.Failed, message + " Error while merging splitted zip files.");
            }

            Result result = HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_WPCONTENT_DIR, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (result.status != Status.Completed)
            {
                return result;
            }

            string unzipMergedSplitFileCommand = Constants.UNZIP_MERGED_WPCONTENT_COMMAND;
            KuduCommandApiResult unzipMergedSplitFileResult = HelperUtils.ExecuteKuduCommandApi(unzipMergedSplitFileCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (unzipMergedSplitFileResult.status != Status.Completed)
            {
                return new Result(Status.Failed, message + " Error while extracting merged zip file.");
            }

            HelperUtils.WriteOutputWithNewLine("Sucessfully processed uploaded App " +
                "data on Linux App Service...", this._progressViewRTextBox);

            return new Result(Status.Completed, "Sucessfully processed uploaded App " +
                "data on Linux App Service...");
        }

        private Result _SplitWpContentZip()
        {
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
                return new Result(Status.Completed, "App data zip file splitted successful...");
            }
            catch (Exception ex) 
            {
                return new Result(Status.Failed, "Couldn't split App data zip file. Error=" + ex.Message);
            }
        }
    }
}
