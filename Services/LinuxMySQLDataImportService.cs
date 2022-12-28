using System;
using System.IO;
using WordPressMigrationTool.Utilities;
using MySql.Data.MySqlClient;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.ResourceManager.AppService;
using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualBasic.ApplicationServices;
using System.Runtime.ConstrainedExecution;

namespace WordPressMigrationTool
{
    public class LinuxMySQLDataImportService
    {
        WebSiteResource _destinationSiteResource;
        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private string _serverHostName;
        private string _username;
        private string _password;
        private string _databaseName;
        private RichTextBox? _progressViewRTextBox;
        private string[] _previousMigrationStatus;
        private string _migrationStatusFilePath;


        public LinuxMySQLDataImportService(WebSiteResource destinationSiteResource, string serverHostName, string username,
            string password, string databaseName, string appServiceName, string ftpUserName, string ftpPassword, RichTextBox? progressViewRTextBox, string[] previousMigrationStatus, string migrationStatusFilePath)
        {
            if (destinationSiteResource == null)
            {
                throw new ArgumentException("Invalid Destination website resource found! ");
            }

            if (string.IsNullOrWhiteSpace(serverHostName))
            {
                throw new ArgumentException("Invalid MySQL servername found! " +
                    "serverHostName=", serverHostName);
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Invalid MySQL username found! " +
                    "username=" + username);
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Invalid MySQL password found! " +
                    "password=" + password);
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException("Invalid database name found! " +
                    "databaseName=" + databaseName);
            }

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

            this._destinationSiteResource = destinationSiteResource;
            this._serverHostName = serverHostName;
            this._username = username;
            this._password = password;
            this._databaseName = databaseName;
            this._appServiceName = appServiceName;
            this._ftpUserName = ftpUserName;
            this._ftpPassword = ftpPassword;
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
            this._migrationStatusFilePath = migrationStatusFilePath;
        }

        public Result ImportData()
        {
            Stopwatch timer = Stopwatch.StartNew();
            HelperUtils.WriteOutputWithNewLine("Preparing to upload MySQL data to destination site...", this._progressViewRTextBox);

            Result result = this._SetupMySqlDumpPlaceholderDirectory();
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._SplitMysqlZip();
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._UploadMysqlSplitZipFiles();
            if (result.status != Status.Completed)
            {
                return result;
            }
            /*
            result = this._ProcessUploadedMysqlSplitZipFiles();
            if (result.status != Status.Completed)
            {
                return result;
            }*/

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully migrated MySQL data to destination site... " +
                "time taken= " + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

            return new Result(Status.Completed, "Successfully migrated MySQL data to destination site.");
        }

        private Result _SplitMysqlZip()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.splitMysqlZipCompleted))
            {
                return new Result(Status.Completed, "");
            }

            HelperUtils.WriteOutputWithNewLine("Splitting MySQL zip file...", this._progressViewRTextBox);
            string mysqlFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);
            string splitZipFilesDirectory = Environment.ExpandEnvironmentVariables(Constants.MYSQL_SPLIT_ZIP_FILES_DIR);
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.MYSQL_SPLIT_ZIP_FILE_PATH);

            if (Directory.Exists(splitZipFilesDirectory))
            {
                Directory.Delete(splitZipFilesDirectory, true);
            }

            try
            {
                Directory.CreateDirectory(splitZipFilesDirectory);
                using (var zipFile = Ionic.Zip.ZipFile.Read(mysqlFilePath))
                {
                    zipFile.MaxOutputSegmentSize = Constants.KUDU_ZIP_API_MAX_UPLOAD_LIMIT;
                    zipFile.Save(splitZipFilePath);
                }

                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.splitMysqlZipCompleted + Environment.NewLine);
                return new Result(Status.Completed, "MySQL zip file splitted successful...");
            }
            catch (Exception ex)
            {
                return new Result(Status.Failed, "Couldn't split MySQL zip file. Error=" + ex.Message);
            }
        }

        private Result _UploadMysqlSplitZipFiles()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.uploadMysqlSplitZipFilesCompleted))
            {
                return new Result(Status.Completed, "");
            }

            string mysqlSplitZipDir = Environment.ExpandEnvironmentVariables(Constants.MYSQL_SPLIT_ZIP_FILES_DIR);
            string[] splitZipFilesArr = Directory.GetFiles(mysqlSplitZipDir);
            if (splitZipFilesArr.Length == 0)
            {
                return new Result(Status.Failed, "Could not find MySQL split zip data at " + mysqlSplitZipDir);
            }

            HelperUtils.WriteOutputWithRC("MySQL upload progress - Finished uploading 0 out of "
                + splitZipFilesArr.Length + " files.", this._progressViewRTextBox);

            for (int splitInd = 0; splitInd < splitZipFilesArr.Length; splitInd++)
            {
                string splitZipFileName = Path.GetFileName(splitZipFilesArr[splitInd]);
                Result result = this._UploadSplitZipFileToAppService(splitZipFileName);
                if (result.status == Status.Failed || result.status == Status.Cancelled)
                {
                    return result;
                }

                HelperUtils.WriteOutputWithRC("MySQL upload progress - Finished uploading " + (splitInd + 1) + " out of "
                    + splitZipFilesArr.Length + " files." + ((splitInd + 1 == splitZipFilesArr.Length) ? "\n" : ""),
                    this._progressViewRTextBox);
            }

            File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.uploadMysqlSplitZipFilesCompleted + Environment.NewLine);
            return new Result(Status.Completed, "MySQL split zip files uploaded successfully...");
        }

        private Result _UploadSplitZipFileToAppService(string splitZipFileName)
        {
            if (this._previousMigrationStatus.Contains(String.Format(Constants.StatusMessages.uploadMysqlSplitZipFileCompleted, splitZipFileName)))
            {
                return new Result(Status.Completed, "");
            }

            string zippedSplitZipFilesDir = Environment.ExpandEnvironmentVariables(Constants.MYSQL_SPLIT_ZIP_NESTED_DIR);
            string zippedFileToUpload = zippedSplitZipFilesDir + splitZipFileName.Replace(".", "") + ".zip";
            string splitZipFilePath = Environment.ExpandEnvironmentVariables(Constants.MYSQL_SPLIT_ZIP_FILES_DIR + splitZipFileName);
            string uploadMysqlKuduUrl = HelperUtils.GetKuduApiForZipUpload(this._appServiceName, Constants.MYSQL_TEMP_DIR_KUDU_API);

            if (Directory.Exists(zippedSplitZipFilesDir))
            {
                Directory.Delete(zippedSplitZipFilesDir, true);
            }
            Directory.CreateDirectory(zippedSplitZipFilesDir);

            var zipFile = new Ionic.Zip.ZipFile(Encoding.UTF8);
            zipFile.AddFile(splitZipFilePath, "");
            zipFile.Save(zippedFileToUpload);

            Result result = HelperUtils.LinuxAppServiceUploadZip(zippedFileToUpload,
                uploadMysqlKuduUrl, this._ftpUserName, this._ftpPassword);

            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, String.Format(Constants.StatusMessages.uploadMysqlSplitZipFileCompleted, splitZipFileName) + Environment.NewLine);
            }

            return result;
        }

        private Result _ProcessUploadedMysqlSplitZipFiles()
        {
            string message = "Unable to porocess the uploaded MySQL file on desitnation Linux App Service.";
            HelperUtils.WriteOutputWithNewLine("Procesing uploaded MySQL dump on Linux App Service...", this._progressViewRTextBox);

            // Merge MySQL Multi-part zip files in Destination App service
            if (!this.mergeSplitZipFiles())
            {
                return new Result(Status.Failed, message + " Error while merging MySQL split zip files.");
            }

            // Extract app data to /home/site/wwwroot/wp-content/ directory
            if (!this.extractAppDataZipInDestinationApp())
            {
                return new Result(Status.Failed, message + "Error while extracting merged MySQL zip file.");
            }

            HelperUtils.WriteOutputWithNewLine("Sucessfully processed uploaded MySQL " +
                "dump on Linux App Service...", this._progressViewRTextBox);

            return new Result(Status.Completed, "Sucessfully processed uploaded MySQL " +
                "dump on Linux App Service...");
        }

        private bool mergeSplitZipFiles()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.mergedMysqlSplitZipFiles))
            {
                return true;
            }

            string mergeSplitZipCommand = Constants.MYSQL_MERGE_SPLLIT_FILES_COMAMND;
            KuduCommandApiResult result = HelperUtils.ExecuteKuduCommandApi(mergeSplitZipCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);

            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.mergedMysqlSplitZipFiles + Environment.NewLine);
                return true;
            }
            return false;
        }

        private bool extractAppDataZipInDestinationApp()
        {
            if (this._previousMigrationStatus.Contains(Constants.StatusMessages.extractMysqlZipInDestinationApp))
            {
                return true;
            }

            string unzipMergedSplitFileCommand = Constants.UNZIP_MERGED_MYSQL_COMMAND;
            KuduCommandApiResult result = HelperUtils.ExecuteKuduCommandApi(unzipMergedSplitFileCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);

            if (result.status == Status.Completed)
            {
                File.AppendAllText(this._migrationStatusFilePath, Constants.StatusMessages.extractMysqlZipInDestinationApp + Environment.NewLine);
                return true;
            }
            return false;
        }

        private Result _UploadMySqlDump()
        {
            HelperUtils.WriteOutputWithNewLine("Uploading MySQL dump to destination app.", this._progressViewRTextBox);
            string uploadMySqlDumpKuduUrl = HelperUtils.GetKuduApiForZipUpload(this._appServiceName, Constants.LIN_MYSQL_DUMP_UPLOAD_PATH_FOR_KUDU_API);
            string mySqlZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);

            if (!File.Exists(mySqlZipFilePath))
            {
                return new Result(Status.Failed, "MySQL dump not found at " + mySqlZipFilePath);
            }

            Result result = HelperUtils.LinuxAppServiceUploadZip(mySqlZipFilePath, uploadMySqlDumpKuduUrl, this._ftpUserName, this._ftpPassword);
            if (result.status != Status.Completed)
            {
                return result;
            }

            HelperUtils.WriteOutputWithNewLine("Sucessfully uploaded MySQL dump.", this._progressViewRTextBox);
            return new Result(Status.Completed, "Successfully uploaded MySQL dump.");
        }


        private Result _SetupMySqlDumpPlaceholderDirectory()
        {
            string errMsg = "Could not setup placeholder directory in destination app service for MySQL dump...";
            Result result = HelperUtils.ClearAppServiceDirectory(Constants.MYSQL_TEMP_DIR, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (result.status != Status.Completed)
            {
                return new Result(Status.Failed, errMsg);
            }

            KuduCommandApiResult createMySqlDirectoryResult = HelperUtils.ExecuteKuduCommandApi(Constants.MYSQL_CREATE_TEMP_DIR_COMMAND, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (createMySqlDirectoryResult.status != Status.Completed)
            {
                return new Result(Status.Failed, errMsg);
            }

            return new Result(Status.Completed, "Successfully created a " +
                "temp MySQL data dump directory on Linux App Service.");
        }
    }
}
