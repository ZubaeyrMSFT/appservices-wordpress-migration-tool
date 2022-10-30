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
        private bool _result = false;
        private string _message = "";
        private int _retriesCount = 0;
        private long _lastCheckpointCountForDisplay = 0;
        private readonly SemaphoreSlim _downloadLock = new SemaphoreSlim(0);


        public WindowsAppDataExportService(string appServiceName, string ftpUserName, string ftpPassword) {
            if (string.IsNullOrWhiteSpace(appServiceName)) 
            {
                throw new ArgumentException("Invalid AppService name found! " +
                    "appServiceName=", appServiceName);
            }

            if (string.IsNullOrWhiteSpace(ftpUserName))
            {
                throw new ArgumentException("Invalid FTP username found! " +
                    "ftpUsername=" +  ftpUserName);
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

        public Result ExportData()
        {
            string appServiceKuduURL = HelperUtils.GetKuduApiForZipDownload(this._appServiceName);
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);

            Console.WriteLine("Exporting App Service data to " + outputFilePath);
            Stopwatch timer = Stopwatch.StartNew();


            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }


            while (this._retriesCount <= Constants.MAX_WIN_APPSERVICE_RETRIES)
            {
                HelperUtils.DeleteFileIfExists(outputFilePath);

                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(this._ftpUserName, this._ftpPassword);
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(appServiceKuduURL), outputFilePath);
                    this._downloadLock.Wait();

                    if (!this._result)
                    {
                        this._retriesCount++;
                        if (this._retriesCount > Constants.MAX_WIN_APPSERVICE_RETRIES)
                        {
                            HelperUtils.DeleteFileIfExists(outputFilePath);
                            return new Result(Status.Failed, this._message);
                        }
                        else
                        {
                            Console.WriteLine("Retrying App Service data download... " + this._retriesCount);
                            continue;
                        }
                    }
                    break;
                }
            }

            Console.WriteLine("Sucessfully exported App Service data... Time Taken={0} seconds", (timer.ElapsedMilliseconds / 1000));
            return new Result(Status.Completed, this._message);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this._result = false;
                this._message = e.Error.Message;

                if (e.Error.Message.ToLower().Contains("unauthorized"))
                {
                    this._retriesCount = Constants.MAX_WIN_APPSERVICE_RETRIES + 1;
                }

                this._downloadLock.Release();
                return;
            }

            this._result = !e.Cancelled;
            if (!this._result)
            {
                this._retriesCount = Constants.MAX_WIN_APPSERVICE_RETRIES + 1;
                this._message = "Download Cancelled";
            }
            else
            {
                this._message = "Download Completed";
            }

            this._downloadLock.Release();    
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            long displayWindowSize = (1024 * 1024);
            double conversionFactorToMB = (1024 * 1024);

            if (e.TotalBytesToReceive != -1 && e.BytesReceived == e.TotalBytesToReceive)
            {
                Console.WriteLine("Download Progres - " + String.Format("{0:0.0}", (e.BytesReceived / conversionFactorToMB)) 
                    + " MB received out of " + String.Format("{0:0.0}", (e.TotalBytesToReceive / conversionFactorToMB)) + " MB");
            }
            else if (e.BytesReceived  - this._lastCheckpointCountForDisplay >= displayWindowSize)
            {
                this._lastCheckpointCountForDisplay = e.BytesReceived;
                Console.WriteLine("Download Progress - " + String.Format("{0:0.0}", (e.BytesReceived / conversionFactorToMB)) + " MB received out of " 
                    + ((e.TotalBytesToReceive == -1) ? "NA" : String.Format("{0:0.0}", (e.TotalBytesToReceive / conversionFactorToMB))) + " MB");
            }
        }
    }
}
