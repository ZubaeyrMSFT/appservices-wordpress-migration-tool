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

namespace WordPressMigrationTool
{
    public class LinuxMySQLDataImportService
    {
        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private string _serverHostName;
        private string _username;
        private string _password;
        private string _databaseName;
        private string _charset;
        private int _retriesCount = 0;


        public LinuxMySQLDataImportService(string serverHostName, string username,
            string password, string databaseName, string charset, string appServiceName, 
            string ftpUserName, string ftpPassword)
        {

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

            this._serverHostName = serverHostName;
            this._username = username;
            this._password = password;
            this._databaseName = databaseName;
            this._charset = charset;
            this._appServiceName = appServiceName;
            this._ftpUserName = ftpUserName;
            this._ftpPassword = ftpPassword;
        }

        public async Task<Result> importData()
        {
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputSqlFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH);
            string mySqlZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);
            //Console.WriteLine("Exporting MySQL database dump to " + outputZipFilePath);
            Stopwatch timer = Stopwatch.StartNew();

            bool createMySqlDirectoryResult = await this._createMySqlDumpPlaceholderDirectory();
            if (createMySqlDirectoryResult)
            {
                return new Result(Status.Failed, "Could not create placeholder directory in destination app service for MySQL dump...");
            }

            Result mysqlDumpUploadResult = await this._uploadMySqlDump();
            if (mysqlDumpUploadResult.status != Status.Completed)
            {
                return mysqlDumpUploadResult;
            }

            KuduCommandApiResult installMysqlPackageOnLinuxSiteResult = await HelperUtils.executeKuduCommandApi(Constants.MYSQL_PACKAGE_INSTALL, this._ftpUserName, this._appServiceName, this._ftpPassword, 3);
            if (installMysqlPackageOnLinuxSiteResult.status != Status.Completed)
            {
                return new Result(installMysqlPackageOnLinuxSiteResult.status, "Could not install mysql-client package on destination site...");
            }

            string mysqlImportCommand = String.Format(Constants.MYSQL_DUMP_IMPORT , this._username, this._password, this._serverHostName, this._databaseName, this._databaseName);
            KuduCommandApiResult importMysqlDumpResult = await HelperUtils.executeKuduCommandApi(Constants.MYSQL_PACKAGE_INSTALL, this._ftpUserName, this._appServiceName, this._ftpPassword, 3);
            if (importMysqlDumpResult.status != Status.Completed)
            {
                return new Result(importMysqlDumpResult.status, "Could not import MySQL dump...");
            }

            return new Result(Status.Completed, "MySQL Database import completed...");
        }

        private async Task<Result> _uploadMySqlDump()
        {
            string uploadMySqlDumpKuduUrl = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, Constants.LIN_MYSQL_DUMP_UPLOAD_PATH_FOR_KUDU_API);
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputSqlFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH);
            string mySqlZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);
            string mySqlZipFileName = Constants.WIN_MYSQL_ZIP_FILENAME;

            if (!File.Exists(mySqlZipFilePath))
            {
                return new Result(Status.Failed, "MySQL dump not found at " + mySqlZipFilePath);
            }

            while (this._retriesCount <= Constants.MAX_APPDATA_UPLOAD_RETRIES)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    MultipartFormDataContent content = new MultipartFormDataContent();
                    ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(mySqlZipFilePath));

                    content.Add(fileContent, "file", mySqlZipFileName);

                    var byteArray = Encoding.ASCII.GetBytes(this._ftpUserName + ":" + this._ftpPassword);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var response = await client.PutAsync(uploadMySqlDumpKuduUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    this._retriesCount++;
                    if (this._retriesCount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        return new Result(Status.Failed, "MySQL dump upload failed...");
                    }
                    else
                    {
                        Console.WriteLine("Retrying MySQL dump upload... " + this._retriesCount);
                        continue;
                    }
                }
            }

            //Console.WriteLine("Sucessfully uploaded MySQL dump to App Service... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, "Successfully uploaded MySQL dump.");
        }

        private async Task<bool> _createMySqlDumpPlaceholderDirectory()
        {
            string appServiceKuduCommandURL = HelperUtils.getKuduUrlForCommandExec(this._appServiceName);
            int trycount=1;
            while(trycount <= Constants.MAX_RETRIES_COMMON)
            {
                 using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes(this._ftpUserName + ":" + this._ftpPassword);
                    var jsonString = JsonConvert.SerializeObject(new { command = Constants.MYSQL_CREATE_TEMP_DIR_COMMAND, dir = "" });

                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ("application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage response = await client.PostAsync(appServiceKuduCommandURL, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    trycount++;
                }
            }
            return false;
        }
    }
}
