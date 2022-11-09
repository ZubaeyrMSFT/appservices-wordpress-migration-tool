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
        private int _retriesCount = 0;


        public LinuxMySQLDataImportService(WebSiteResource destinationSiteResource, string serverHostName, string username,
            string password, string databaseName, string appServiceName, 
            string ftpUserName, string ftpPassword)
        {
            if(destinationSiteResource == null)
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
        }

        public Result importData()
        {
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputSqlFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH);
            string mySqlZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);

            Stopwatch timer = Stopwatch.StartNew();

            if (!_setupMySqlDumpPlaceholderDirectory())
            {
                return new Result(Status.Failed, "Could not setup placeholder directory in destination app service for MySQL dump...");
            }

            Result mysqlDumpUploadResult = this._uploadMySqlDump();
            if (mysqlDumpUploadResult.status != Status.Completed)
            {
                return mysqlDumpUploadResult;
            }

            if (!this._startDatabaseImportInAppContainer())
            {
                return new Result(Status.Failed, "Could not initiate MySQL Database import in Destination App Service...");
            }


            if (!this._waitForDBImportInAppService())
            {
                return new Result(Status.Failed, "Could not verify MySQL Database import completed in Destination App Service...");
            }

            if (!this._stopDatabaseImportInAppContainer())
            {
                return new Result(Status.Failed, "Could not remove App Settings that trigger MySQL Database import in Destination App Service...");
            }

            return new Result(Status.Completed, "MySQL Database import completed...");
        }

        private Result _uploadMySqlDump()
        {
            string uploadMySqlDumpKuduUrl = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, Constants.LIN_MYSQL_DUMP_UPLOAD_PATH_FOR_KUDU_API);
            string mySqlZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);

            if (!File.Exists(mySqlZipFilePath))
            {
                return new Result(Status.Failed, "MySQL dump not found at " + mySqlZipFilePath);
            }

            bool MySqlZipFileUploadResult = HelperUtils.LinAppServiceUploadZip(mySqlZipFilePath, uploadMySqlDumpKuduUrl, this._ftpUserName, this._ftpPassword);
            if (!MySqlZipFileUploadResult)
            {
                return new Result(Status.Failed, "Couldn't Upload MySql dump file..");
            }

            //Console.WriteLine("Sucessfully uploaded MySQL dump to App Service... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, "Successfully uploaded MySQL dump.");
        }

        // Creates MySQL Dump placeholder directory in the destination app service
        private bool _setupMySqlDumpPlaceholderDirectory()
        {
            // Clear existing files in MySQL placeholder directory
            if (!HelperUtils.ClearAppServiceDirectory(Constants.MYSQL_TEMP_DIR, this._ftpUserName, this._ftpPassword, this._appServiceName))
                return false;

            //Create MYSQL placeholder directory if not already exists 
            KuduCommandApiResult createMySqlDirectoryResult = HelperUtils.executeKuduCommandApi(Constants.MYSQL_CREATE_TEMP_DIR_COMMAND, this._ftpUserName, this._ftpPassword, this._appServiceName);
            if (createMySqlDirectoryResult.status != Status.Completed)
            {
                return false;
            }
            return true;
              
        }

        public bool _startDatabaseImportInAppContainer()
        {
            Dictionary<string,string> appSettings = new Dictionary<string,string>();
            appSettings.Add(Constants.START_MIGRATION_APP_SETTING, "True");
            appSettings.Add(Constants.NEW_DATABASE_NAME_APP_SETTING, this._databaseName);
            appSettings.Add(Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING, String.Format("{0}{1}.sql", Constants.MYSQL_TEMP_DIR, this._databaseName));

            try
            {
                return AzureManagementUtils.UpdateApplicationSettingForAppService(this._destinationSiteResource, appSettings);
            }
            catch
            {
                return false;
            }
        }

        public bool _stopDatabaseImportInAppContainer()
        {
            string[] appSettings = { Constants.START_MIGRATION_APP_SETTING, Constants.NEW_DATABASE_NAME_APP_SETTING, Constants.MYSQL_DUMP_FILE_PATH_APP_SETTING };

            try
            {
                return AzureManagementUtils.removeApplicationSettingForAppService(this._destinationSiteResource, appSettings);
            }
            catch
            {
                return false;
            }
        }

        public bool _waitForDBImportInAppService()
        {
            string checkDbImportStatusNestedCommand = String.Format("grep '{0}' {1}", Constants.DB_IMPORT_STATUS_MESSAGE, Constants.LIN_APP_DB_STATUS_FILE_PATH);
            
            int maxRetryCount = 10000;
            for (int i=0; i<maxRetryCount; i++)
            {
                KuduCommandApiResult checkDbImportStatusResult = HelperUtils.executeKuduCommandApi(checkDbImportStatusNestedCommand, this._ftpUserName, this._ftpPassword, this._appServiceName);
                if (checkDbImportStatusResult.status == Status.Completed
                    && checkDbImportStatusResult.exitCode == 0
                    && checkDbImportStatusResult.output != null && checkDbImportStatusResult.output.Contains(Constants.DB_IMPORT_STATUS_MESSAGE))
                {
                    return true;
                }
                // Sleep for 10s
                Thread.Sleep(10000);
            }
            return false;
        }
    }
}
