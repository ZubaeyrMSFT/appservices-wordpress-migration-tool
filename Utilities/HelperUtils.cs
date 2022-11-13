using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;
using System.Net.Http.Headers;
using Ionic.Zip;
using System.Reflection.Metadata;
using Org.BouncyCastle.Ocsp;
using System.Collections.Generic;
using System.Diagnostics;

namespace WordPressMigrationTool.Utilities
{
    public static class HelperUtils
    {

        public static string GetKuduApiForZipDownload(string appServiceName)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/zip/site/wwwroot/wp-content/";
            }
            return null;
        }

         public static string getKuduUrlForZipUpload(string appServiceName, string uploadPath)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/zip/" + uploadPath;
            }
            return null;
        }

        public static string getKuduUrlForCommandExec(string appServiceName)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/command";
            }
            return null;
        }

        public static string GetMySQLConnectionStringForExternalMySQLClientTool(string serverHostName, 
            string username, string password, string databaseName, string? charset)
        {
            if (string.IsNullOrWhiteSpace(serverHostName) || string.IsNullOrWhiteSpace(username) 
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(databaseName))
            {
                return null;
            }

            string mysqlConnectionString = "server=" + serverHostName + ";user=" + username + ";pwd="
                + password + ";database=" + databaseName + ";convertzerodatetime=true;";
            if (!string.IsNullOrWhiteSpace(charset))
            {
                return mysqlConnectionString + "charset=" + charset + ";";
            }

            return mysqlConnectionString;
        }

        public static void ParseAndUpdateDatabaseConnectionStringForWinAppService(SiteInfo sourceSite, string databaseConnectionString)
        {
            string[] splits = databaseConnectionString.Split(';');

            foreach (string record in splits)
            {
                string value = record.Substring(record.IndexOf("=") + 1);
                if (record.StartsWith("Database"))
                {
                    sourceSite.databaseName = value;
                }
                else if (record.StartsWith("Data Source"))
                {
                    sourceSite.databaseHostname = value;
                }
                else if (record.StartsWith("User Id"))
                {
                    sourceSite.databaseUsername = value;
                }
                else if (record.StartsWith("Password"))
                {
                    sourceSite.databasePassword = value;
                }
            }
        }

        public static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static KuduCommandApiResult executeKuduCommandApi(string inputCommand, string ftpUsername, string ftpPassword, string appServiceName, int maxRetryCount = 3) {
            if (maxRetryCount <=0 )
            {
                return new KuduCommandApiResult(Status.Failed);
            }
            string command = String.Format("bash -c \" {0} \"", inputCommand);
            var appServiceKuduCommandURL = getKuduUrlForCommandExec(appServiceName);
            int trycount=1;
            while(trycount <= maxRetryCount)
            {
                using (var client = new HttpClient())
                {
                    var jsonString = JsonConvert.SerializeObject(new { command = command, dir = "" });
                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ("application/json");

                    // Set Basic auth
                    var byteArray = Encoding.ASCII.GetBytes(ftpUsername + ":" + ftpPassword);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, appServiceKuduCommandURL);
                    requestMessage.Content = httpContent;

                    HttpResponseMessage response = client.Send(requestMessage);

                    // Convert response to Json
                    var responseStream = response.Content.ReadAsStream();
                    var myStreamReader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseJSON = myStreamReader.ReadToEnd();
                    var responseData = JsonConvert.DeserializeObject<KuduCommandApiResponse>(responseJSON);

                    if (responseData != null && response.IsSuccessStatusCode)
                    {
                        return new KuduCommandApiResult(Status.Completed, responseData.Output, responseData.Error, responseData.ExitCode);
                    }

                    trycount++;
                    if (trycount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        return new KuduCommandApiResult(Status.Failed);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return new KuduCommandApiResult(Status.Failed);
        }
    
        public static bool ClearAppServiceDirectory (string targetFolder, string ftpUsername, string ftpPassword, string appServiceName )
        {
            int maxRetryCount = Constants.MAX_APP_CLEAR_DIR_RETRIES;
            if (maxRetryCount <=0 )
            {
                return false;
            }

            string listTargetDirCommand = String.Format(Constants.LIST_DIR_COMMAND, targetFolder);
            string clearTargetDirCommand = String.Format(Constants.CLEAR_APP_SERVICE_DIR_COMMAND, targetFolder);
            string createTargetDirCommand = String.Format(Constants.LIN_APP_MAKE_DIR_COMMAND, targetFolder);

            int trycount = 1;
            while(trycount <= maxRetryCount)
            {
                KuduCommandApiResult checkTargetDirEmptyResult =  executeKuduCommandApi(listTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_APP_CLEAR_DIR_RETRIES );
                if (checkTargetDirEmptyResult.exitCode == 0 && String.IsNullOrEmpty(checkTargetDirEmptyResult.output))
                {
                    return true;
                }

                KuduCommandApiResult clearTargeDirResult = executeKuduCommandApi(clearTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_APP_CLEAR_DIR_RETRIES );

                KuduCommandApiResult makeTargetDir = executeKuduCommandApi(createTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_RETRIES_COMMON);
                trycount++;
            }
            return false;
        }

        public static bool LinAppServiceUploadZip(string zipFilePath, string kuduUploadUrl, string ftpUsername, string ftpPassword)
        {
            int retryCount = 1;
            while (retryCount <= Constants.MAX_APPDATA_UPLOAD_RETRIES)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/zip"));

                    ByteArrayContent content = new ByteArrayContent(System.IO.File.ReadAllBytes(zipFilePath));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                    var byteArray = Encoding.ASCII.GetBytes(ftpUsername + ":" + ftpPassword);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, kuduUploadUrl);
                    requestMessage.Content = content;

                    HttpResponseMessage response = client.Send(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    retryCount++;
                    if (retryCount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return true;
        }
    }
}
