using System;
using System.Net;
using System.Threading;
using System.IO;
using WordPressMigrationTool.Utilities;
using System.Diagnostics;
using System.Windows.Forms;

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
        private RichTextBox? _progressViewRTextBox;

        public WindowsAppDataExportService(string appServiceName, string ftpUserName, string ftpPassword, RichTextBox? progressViewRTextBox) {
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
            this._progressViewRTextBox = progressViewRTextBox;
        }

        public Result ExportData()
        {
            string appServiceKuduURL = HelperUtils.GetKuduApiForZipDownload(this._appServiceName);
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);

            HelperUtils.WriteOutputWithNewLine("Exporting Windows App Service data to " 
                + outputFilePath, this._progressViewRTextBox);
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
                            timer.Stop();
                            HelperUtils.WriteOutputWithNewLine("Unable to export Windows App Service data... time taken=" 
                                + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

                            HelperUtils.DeleteFileIfExists(outputFilePath);
                            return new Result(Status.Failed, this._message);
                        }
                        else
                        {
                            HelperUtils.WriteOutputWithNewLine("Retrying Windows App Service data download... " 
                                + this._retriesCount, this._progressViewRTextBox);
                            continue;
                        }
                    }
                    break;
                }
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully exported Windows App Service data... time taken=" 
                + (timer.ElapsedMilliseconds / 1000)  + " seconds\n", this._progressViewRTextBox);
            return new Result(Status.Completed, this._message);
        }

        private void Client_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                this._lastCheckpointCountForDisplay = e.BytesReceived;
                string outputString = "Download completed - " + String.Format("{0:0.0}", (e.BytesReceived / conversionFactorToMB))
                    + " MB received out of " + String.Format("{0:0.0}", (e.TotalBytesToReceive / conversionFactorToMB)) + " MB\n";
                HelperUtils.WriteOutputWithRC(outputString, this._progressViewRTextBox);
            }
            else if (e.BytesReceived  - this._lastCheckpointCountForDisplay >= displayWindowSize)
            {
                this._lastCheckpointCountForDisplay = e.BytesReceived;
                string outputString = "Download progress - " + String.Format("{0:0.0}", (e.BytesReceived / conversionFactorToMB)) + " MB received";
                HelperUtils.WriteOutputWithRC(outputString, this._progressViewRTextBox);
            }
        }
    }
}
