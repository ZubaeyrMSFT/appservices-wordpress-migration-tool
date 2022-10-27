using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Linq.Expressions;
using System.Threading;
using System.IO;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class ExportWindowsAppServiceData
    {

        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private bool _result = false;
        private string _message = null;
        private int _retriesCount = 0;
        private long _lastCheckpointBytesForDisplay = 0;
        private readonly SemaphoreSlim _downloadLock = new SemaphoreSlim(0);


        public ExportWindowsAppServiceData(string appServiceName, string ftpUserName, string ftpPassword) {
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

        public Result exportData()
        {
            string appServiceKuduURL = Constants.getKuduApiForZipDownload(this._appServiceName);
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }


            while (_retriesCount <= Constants.MAX_WIN_APPSERVICE_RETRIES)
            {
                 if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }
               
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(this._ftpUserName, this._ftpPassword);
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(appServiceKuduURL), outputFilePath);
                    this._downloadLock.Wait();

                    if (!_result)
                    {
                        _retriesCount++;
                        if (_retriesCount > Constants.MAX_WIN_APPSERVICE_RETRIES)
                        {
                            return new Result(Status.Failed, this._message);
                        }
                        else
                        {
                            Console.WriteLine("Retrying Download... " + _retriesCount);
                            continue;
                        }
                    }
                    break;
                }
            }

            return new Result(Status.Success, this._message);
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

                _downloadLock.Release();
                return;
            }

            _result = !e.Cancelled;
            if (!_result)
            {
                this._message = e.Error.Message;
            }
            else
            {
                this._message = "Download Completed";
            }

            _downloadLock.Release();    
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
            else if (e.BytesReceived  - this._lastCheckpointBytesForDisplay >= displayWindowSize)
            {
                this._lastCheckpointBytesForDisplay = e.BytesReceived;
                Console.WriteLine("Download Progress - " + String.Format("{0:0.0}", (e.BytesReceived / conversionFactorToMB)) + " MB received out of " 
                    + ((e.TotalBytesToReceive == -1) ? "NA" : String.Format("{0:0.0}", (e.TotalBytesToReceive / conversionFactorToMB))) + " MB");
            }
        }
    }
}
