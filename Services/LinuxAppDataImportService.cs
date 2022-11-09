using System;
using System.Net;
using System.Threading;
using System.IO;
using WordPressMigrationTool.Utilities;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class LinuxAppDataImportService
    {

        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private string[] _splitZipFilesArr;
        private int _retriesCount = 0;
        private readonly SemaphoreSlim _binaryLock = new SemaphoreSlim(0);


        public LinuxAppDataImportService(string appServiceName, string ftpUserName, string ftpPassword)
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
        }

        public Result importData()
        {
            string uploadWpContentKuduUrl = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, "site/wwwroot/wp-content");
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string appContentFileName = Constants.WIN_WPCONTENT_ZIP_FILENAME;

            Console.WriteLine("Exporting App Service data to " + appContentFilePath);
            Stopwatch timer = Stopwatch.StartNew();

            if (!HelperUtils.ClearAppServiceDirectory(Constants.LIN_APP_SVC_WPCONTENT_DIR, this._ftpUserName, this._ftpPassword, this._appServiceName))
            {
                return new Result(Status.Failed, "Could not clear wp-content directory in App Service");
            }

            Result result = splitWpContentZip();
            if (result.status == Status.Failed || result.status == Status.Cancelled)
            {
                return result;
            }

            Result uploadSplitZipFilesResult = this._uploadSplitZipFiles();
            if (uploadSplitZipFilesResult.status != Status.Completed)
            {
                return uploadSplitZipFilesResult;
            }

            // merges split zip files and extracts to wp-content directory in app service
            if (! this._processSplitZipFiles())
            {
                return new Result(Status.Failed, "Could not upload wp-content to app service");
            }

            //Console.WriteLine("Sucessfully uploaded App Service data... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, "Successfully uploaded App Service data.");
        }

        private Result _uploadSplitZipFiles()
        {
            string appContentSplitZipDir = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR);
            this._splitZipFilesArr = Directory.GetFiles(appContentSplitZipDir);
            if (this._splitZipFilesArr.Length == 0)
            {
                return new Result(Status.Failed, "App Service split zip data not found at " + appContentSplitZipDir);
            }

            for (int splitInd = 0; splitInd < this._splitZipFilesArr.Length; splitInd++)
            {
                string splitZipFileName = Path.GetFileName(this._splitZipFilesArr[splitInd]);
                if (!this._uploadSplitZipFileToAppService(splitZipFileName))
                {
                    return new Result(Status.Failed, "Could not upload " + splitZipFileName + " to destination app");
                }
            }
            return new Result(Status.Completed, "WP-Content split zip file upload successful..");
        }

        private bool _uploadSplitZipFileToAppService(string splitZipFileName)
        {
            string zippedSplitZipFilesDir = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_NESTED_DIR);
            string zippedFileToUpload = zippedSplitZipFilesDir + splitZipFileName.Replace(".", "") + ".zip";
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR + splitZipFileName);
            string uploadWpContentKuduUrl = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, Constants.WPCONTENT_TEMP_DIR_KUDU_API);

            if (Directory.Exists(zippedSplitZipFilesDir))
            {
                Directory.Delete(zippedSplitZipFilesDir, true);
            }
            Directory.CreateDirectory(zippedSplitZipFilesDir);

            var zipFile = new Ionic.Zip.ZipFile(Encoding.UTF8);
            zipFile.AddFile(splitZipFilePath, "");
            zipFile.Save(zippedFileToUpload);

            if (!HelperUtils.LinAppServiceUploadZip(zippedFileToUpload, uploadWpContentKuduUrl, this._ftpUserName, this._ftpPassword))
                return false;

            return true;
        }

        private bool _processSplitZipFiles()
        {
            string mergeSplitZipCommand = Constants.WPCONTENT_MERGE_SPLLIT_FILES_COMAMND;
            KuduCommandApiResult mergeSplitZipResult = HelperUtils.executeKuduCommandApi(mergeSplitZipCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (mergeSplitZipResult.status != Status.Completed)
            {
                return false;
            }

            // Clean split zip files in app service once merged
            string linAppSplitZipFilesDir = Constants.WPCONTENT_TEMP_DIR;
            if (!HelperUtils.ClearAppServiceDirectory(linAppSplitZipFilesDir, this._ftpUserName, this._ftpPassword, this._appServiceName))
            {
                return false;
            }

            string unzipMergedSplitFileCommand = Constants.UNZIP_MERGED_WPCONTENT_COMMAND;
            KuduCommandApiResult unzipMergedSplitFileResult = HelperUtils.executeKuduCommandApi(unzipMergedSplitFileCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (unzipMergedSplitFileResult.status != Status.Completed)
            {
                return false;
            }

            return true;
        }

        private Result splitWpContentZip()
        {
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string splitZipFilesDirectory = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILES_DIR);
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WPCONTENT_SPLIT_ZIP_FILE_PATH);

            if (Directory.Exists(splitZipFilesDirectory))
            {
                Directory.Delete(splitZipFilesDirectory, true);
            }

            Directory.CreateDirectory(splitZipFilesDirectory);

            try
            {
                using (var zipFile = Ionic.Zip.ZipFile.Read(appContentFilePath))
                {
                    zipFile.MaxOutputSegmentSize = Constants.KUDU_ZIP_API_MAX_UPLOAD_LIMIT;
                    zipFile.Save(splitZipFilePath);
                }
                return new Result(Status.Completed, "Zip file split successful...");
            }
            catch
            {
                return new Result(Status.Failed, "Couldn't split zip file...");
            }
        }
    }
}
