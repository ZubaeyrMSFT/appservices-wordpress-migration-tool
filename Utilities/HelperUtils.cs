using System;
using System.IO;

namespace WordPressMigrationTool.Utilities
{
    public static class HelperUtils
    {

        public static string getKuduApiForZipDownload(string appServiceName)
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

        public static string getMySQLConnectionStringForExternalMySQLClientTool(string serverHostName, 
            string username, string password, string databaseName, string charset)
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

        public static void parseAndUpdateDatabaseConnectionStringForWinAppService(SiteInfo sourceSite, string databaseConnectionString)
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

        public static void deleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public KuduCommandApiResult executeKuduCommandApi(string command, string ftpUsername, string ftpPassword, string appServiceName, int maxRetryCount = 3) {
            var appServiceKuduCommandURL = getKuduUrlForCommandExec(appServiceName);
            int trycount=1;
            while(trycount <= maxRetryCount)
            {
                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes(ftpUserName + ":" + ftpPassword);
                    var jsonString = JsonConvert.SerializeObject(new { command = command, dir = "" });

                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ("application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage response = await client.PostAsync(appServiceKuduCommandURL, httpContent);

                    if (response.isSuccessStatusCode)
                    {
                        var response = await ReadFromJsonAsync(response.content, KuduCommandApiResponse);
                        return new KuduCommandApiResult(Status.Completed, response.Output, response.Error, response.ExistCode);
                    }

                    trycount++;
                    if (trycount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        return new KuduCommandApiResult(Status.Failed);
                    }
                    else
                    {
                        Console.WriteLine("Retrying to create placeholder directory for MySQL dump... " + this._retriesCount);
                        continue;
                    }
                }
            }
            return new KuduCommandApiResult(Status.Failed);
        }
    }
}
