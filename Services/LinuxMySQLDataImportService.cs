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

            result = this._StartDatabaseImportInAppContainer();
            if (result.status != Status.Completed)
            {
                return result;
            }

            result = this._WaitForDBImportInAppService();
            if (result.status != Status.Completed)
            {
                this.StopDatabaseImportInAppContainer();
                return result;
            }

            result = this.StopDatabaseImportInAppContainer();
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

        public Result _StartDatabaseImportInAppContainer()
        {
            try
            {
                HelperUtils.WriteOutputWithNewLine("Initiating MySQL import on destination site.", this._progressViewRTextBox);
                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");
                appSettings.Add(Constants.NEW_DATABASE_NAME_APP_SETTING, this._databaseName);
                appSettings.Add(Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING, String.Format("{0}{1}", Constants.MYSQL_TEMP_DIR, Constants.WIN_MYSQL_SQL_FILENAME));
                if (AzureManagementUtils.UpdateApplicationSettingForAppService(this._destinationSiteResource, appSettings))
                {
                    return new Result(Status.Completed, "");
                }
            }
            catch { }
            return new Result(Status.Failed, "Unable to initiate MySQL import process on destination site...");
        }

        public Result StopDatabaseImportInAppContainer()
        {
            int retiresCount = 1;
            while (retiresCount <= Constants.MAX_RETRIES_COMMON)
            {
                try
                {
                    string[] appSettings = { Constants.START_MIGRATION_APP_SETTING, Constants.NEW_DATABASE_NAME_APP_SETTING, Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING };
                    if (AzureManagementUtils.RemoveApplicationSettingForAppService(this._destinationSiteResource, appSettings))
                    {
                        return new Result(Status.Completed, "");
                    }
                }
                catch { }
                retiresCount++;
            }
           
            return new Result(Status.Failed, "Could not clear " +
                "App Settings used for database import trigger...");
        }

        public Result _WaitForDBImportInAppService()
        {
            string checkDbImportStatusNestedCommand = String.Format("cat {0}", Constants.LIN_APP_DB_STATUS_FILE_PATH);
            Stopwatch timer = Stopwatch.StartNew();
            int maxRetryCount = 2000;

            for (int i=0; i<maxRetryCount; i++)
            {
                KuduCommandApiResult checkDbImportStatusResult = HelperUtils.ExecuteKuduCommandApi(checkDbImportStatusNestedCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
                if (checkDbImportStatusResult.status == Status.Completed
                    && checkDbImportStatusResult.exitCode == 0
                    && checkDbImportStatusResult.output != null)
                {
                    if (checkDbImportStatusResult.output.Contains(Constants.DB_IMPORT_SUCCESS_MESSAGE))
                    {
                        HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
                        return new Result(Status.Completed, "Unable to validate MySQL import status.");
                    }
                    if (checkDbImportStatusResult.output.Contains(Constants.DB_IMPORT_FAILURE_MESSAGE))
                    {
                        HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
                        return new Result(Status.Failed, "Could not import MySQL data on destination site.");
                    }
                }

                HelperUtils.WriteOutputWithRC("Waiting for MySQL database import to finish. Elapsed time = " 
                    + (timer.ElapsedMilliseconds / 1000) + " seconds.", this._progressViewRTextBox);
                Thread.Sleep(10000);
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("", this._progressViewRTextBox);
            return new Result(Status.Failed, "Unable to validate MySQL import status on destination site.");
        }
    }
}
