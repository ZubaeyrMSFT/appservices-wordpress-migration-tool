using System;
using System.Net;
using System.Threading;
using System.IO;
using WordPressMigrationTool.Utilities;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class LinuxAppDataImportService
    {

        private string _ftpUserName;
        private string _ftpPassword;
        private string _appServiceName;
        private int _retriesCount = 0;


        public LinuxAppDataImportService(string appServiceName, string ftpUserName, string ftpPassword)
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

        public async Task<Result> importData()
        {
            string uploadWpContentKuduUrl = HelperUtils.getKuduUrlForZipUpload(this._appServiceName, "site/wwwroot/wp-content");
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string appContentFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_APPSERVICE_DATA_EXPORT_PATH);
            string appContentFIleName = Constants.WIN_WPCONTENT_ZIP_FILENAME;

            Console.WriteLine("Exporting App Service data to " + appContentFilePath);
            Stopwatch timer = Stopwatch.StartNew();

            if (! await clearWpContentInDestinationApp())
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
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    MultipartFormDataContent content = new MultipartFormDataContent();
                    ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(appContentFilePath));

                    content.Add(fileContent, "file", appContentFIleName);

                    var byteArray = Encoding.ASCII.GetBytes(this._ftpUserName + ":" + this._ftpPassword);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage response = await client.PutAsync(uploadWpContentKuduUrl, content);

                    if (response.IsSuccessStatusCode)
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

        private async Task<bool> clearWpContentInDestinationApp()
        {
            KuduCommandApiResult installMysqlPackageOnLinuxSiteResult = await HelperUtils.executeKuduCommandApi(Constants.MYSQL_CLEAN_WPCONTENT_DIR_COMMAND, this._ftpUserName, this._appServiceName, this._ftpPassword, 3);
            if (installMysqlPackageOnLinuxSiteResult.status != Status.Completed)
            {
                return false;
            }
            return true;
        }
    }
}
