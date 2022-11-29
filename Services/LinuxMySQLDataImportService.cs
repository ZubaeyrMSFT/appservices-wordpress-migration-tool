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


        public LinuxMySQLDataImportService(WebSiteResource destinationSiteResource, string serverHostName, string username,
            string password, string databaseName, string appServiceName, string ftpUserName, string ftpPassword, RichTextBox? progressViewRTextBox)
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

            result = this._UploadMySqlDump();
            if (result.status != Status.Completed)
            {
                return result;
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully migrated MySQL data to destination site... " +
                "time taken= " + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

            return new Result(Status.Completed, "Successfully migrated MySQL data to destination site.");
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
