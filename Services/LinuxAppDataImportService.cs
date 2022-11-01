using System;
using System.Net;
using System.Threading;
using System.IO;
using WordPressMigrationTool.Utilities;
using System.Diagnostics;

namespace WordPressMigrationTool
{
    public class WindowsAppDataExportService
    {

        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private string _message = null;
        private int _retriesCount = 0;
        private long _lastCheckpointCountForDisplay = 0;
        private readonly SemaphoreSlim _downloadLock = new SemaphoreSlim(0);


        public LinuxAppDataExportService(string appServiceName, string ftpUserName, string ftpPassword)
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
            string appServiceKuduURL = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, "site/wwwroot/wp-content");
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);

            Console.WriteLine("Exporting App Service data to " + appContentFilePath);
            Stopwatch timer = Stopwatch.StartNew();

            if (!clearWpContentInDestinationApp)
            {
                return new Result(Status.Failed, "Could not clear wp-content directory in App Service");
            }

            if (!File.Exists(appContentFilePath))
            {
                return new Result(Status.Failed, "App Service data not found at " + appContentFilePath);
            }


            while (this._retriesCount <= Constants.MAX_APPDATA_UPLOAD_RETRIES)
            {
                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes(this._ftpUserName + ":" + this._ftpPassword);
                    var jsonString = JsonConvert.SerializeObject(new { data-binary = appContentFilePath });

                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ("application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage response = await client.PutAsync(appServiceKuduURL, httpContent);

                    if (response.isSuccessStatusCode)
                    {
                        File.Delete(appContentFilePath);
                        break;
                    }

                    this._retriesCount++;
                    if (this._retriesCount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        HelperUtils.deleteFileIfExists(appContentFilePath);
                        return new Result(Status.Failed, "App Service data upload failed...");
                    }
                    else
                    {
                        Console.WriteLine("Retrying App Service data upload... " + this._retriesCount);
                        continue;
                    }
                }
            }

            Console.WriteLine("Sucessfully uploaded App Service data... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, "Successfully uploaded App Service data.");
        }

        private bool clearWpContentInDestinationApp()
        {
            int trycount=1;
            while(trycount <= Constants.MAX_WPCONTENT_CLEAR_RETRIES)
            {
                 using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes(this._ftpUserName + ":" + this._ftpPassword);
                    var jsonString = JsonConvert.SerializeObject(new { command = Constants.MYSQL_CLEAN_WPCONTENT_DIR_COMMAND, dir = "" });

                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ("application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage response = await client.PostAsync(appServiceKuduCommandURL, httpContent);

                    if (response.isSuccessStatusCode)
                    {
                        return true;
                    }

                    trycount++;
                    if (trycount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        HelperUtils.deleteFileIfExists(mySqlZipFilePath);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Retrying to create placeholder directory for MySQL dump... " + this._retriesCount);
                        continue;
                    }
                }
            }
            return false;
        }
    }
}
