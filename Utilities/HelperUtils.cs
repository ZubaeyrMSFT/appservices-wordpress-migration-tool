using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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

        public static string GetKuduApiForZipUpload(string appServiceName, string uploadPath)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/zip/" + uploadPath;
            }
            return null;
        }

        public static string GetKuduApiForCommandExec(string appServiceName)
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

        public static void WriteOutputWithNewLine(string message, RichTextBox? richTextBox)
        {
            if (richTextBox != null)
            {
                richTextBox.Invoke(richTextBox.AppendText, message+"\n");                 
            }

            Console.WriteLine(message);
        }

        public static void WriteOutput(string message, RichTextBox? richTextBox)
        {
            if (richTextBox != null)
            {
                richTextBox.Invoke(richTextBox.AppendText, message);
            }

            Console.Write(message);
        }


        public static void WriteOutputWithRC(string message, RichTextBox? richTextBox)
        {
            if (richTextBox != null)
            {
                richTextBox.Invoke(new Action(() =>
                {
                    string currText = richTextBox.Text;
                    richTextBox.Select(currText.LastIndexOf("\n")+1, (currText.Length - currText.LastIndexOf("\n") + 1));
                    richTextBox.Cut();
                    richTextBox.AppendText(message);
                }));

            }

            Console.Write("\r" + message);
        }

        public static KuduCommandApiResult ExecuteKuduCommandApi(string inputCommand, string ftpUsername, string ftpPassword, string appServiceName, int maxRetryCount = 3, string message = "")
        {
            if (maxRetryCount <= 0)
            {
                return new KuduCommandApiResult(Status.Failed);
            }

            string command = String.Format("bash -c \" {0} \"", inputCommand);
            string requestUserAgentValue = Constants.MigrationToolVersion + (String.IsNullOrEmpty(message) ? "" : " (" + message + ")");
            var appServiceKuduCommandURL = GetKuduApiForCommandExec(appServiceName);

            int trycount = 1;
            while (trycount <= maxRetryCount)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        var jsonString = JsonConvert.SerializeObject(new { command = command, dir = "" });
                        HttpContent httpContent = new StringContent(jsonString);
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(requestUserAgentValue);

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
                    }
                    catch { }

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

        // The following function logs current migration status 
        public static Result logMigrationStatusInKuduTable(string ftpUsername, string ftpPassword, string appServiceName, string logMessage)
        {
            Status result = Status.Failed;
            string message = "Unable to log Migration Status in Kudu Table for the app " + appServiceName;

            string listTargetDirCommand = String.Format(Constants.LIST_DIR_COMMAND, "/home");

            try
            {
                // sends out a kudu command API with message embedded in User-Agent Header value
                KuduCommandApiResult executeCommandResult = ExecuteKuduCommandApi(listTargetDirCommand, ftpUsername, ftpPassword, appServiceName, message: logMessage);
                if (executeCommandResult.status == Status.Completed)
                {
                    result = Status.Completed;
                    message = "Successfully logged Status message.";
                }
            }
            catch (Exception e)
            {
                result = Status.Failed;
            }

            return new Result(result, message);
        }

        public static Result ClearAppServiceDirectory(string targetFolder, string ftpUsername, string ftpPassword, string appServiceName, int maxRetryCount = Constants.MAX_APP_CLEAR_DIR_RETRIES)
        {
            Status result = Status.Failed;
            string message = "Unable to clear " + targetFolder 
                + " directory on " + appServiceName + " App Service.";

            if (maxRetryCount <= 0)
            {
                return new Result(Status.Failed, message);
            }

            string listTargetDirCommand = String.Format(Constants.LIST_DIR_COMMAND, targetFolder);
            string clearTargetDirCommand = String.Format(Constants.CLEAR_APP_SERVICE_DIR_COMMAND, targetFolder);
            string createTargetDirCommand = String.Format(Constants.LIN_APP_MAKE_DIR_COMMAND, targetFolder);

            int trycount = 1;
            while (trycount <= maxRetryCount)
            {
                try
                {
                    KuduCommandApiResult checkTargetDirEmptyResult = ExecuteKuduCommandApi(listTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_APP_CLEAR_DIR_RETRIES);
                    if (checkTargetDirEmptyResult.exitCode == 0 && String.IsNullOrEmpty(checkTargetDirEmptyResult.output))
                    {
                        result = Status.Completed;
                        message = "Successfully cleared " + targetFolder 
                            + " directory on " + appServiceName + " App Service.";
                        break;
                    }

                    ExecuteKuduCommandApi(clearTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_APP_CLEAR_DIR_RETRIES);
                    ExecuteKuduCommandApi(createTargetDirCommand, ftpUsername, ftpPassword, appServiceName, Constants.MAX_RETRIES_COMMON);
                }
                catch (Exception e) {
                    result = Status.Failed;
                    message = "Unable to clear " + targetFolder + " directory "
                        + "on " + appServiceName + " App Service. Error=" + e.Message;
                }
                trycount++;
            }

            return new Result(result, message);
        }


        public static Result LinuxAppServiceUploadZip(string zipFilePath, string kuduUploadUrl, string ftpUsername, string ftpPassword)
        {
            Status result = Status.Failed;
            string message = "Unable to upload " + zipFilePath + " to Linux App Service.";

            int retryCount = 1;
            while (retryCount <= Constants.MAX_APPDATA_UPLOAD_RETRIES)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/zip"));

                        ByteArrayContent content = new ByteArrayContent(System.IO.File.ReadAllBytes(zipFilePath));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                        var byteArray = Encoding.ASCII.GetBytes(ftpUsername + ":" + ftpPassword);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                            Convert.ToBase64String(byteArray));

                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, kuduUploadUrl);
                        requestMessage.Content = content;

                        HttpResponseMessage response = client.Send(requestMessage);
                        if (response.IsSuccessStatusCode)
                        {
                            result = Status.Completed;
                            message = "Sucessfully uploaded " + zipFilePath 
                                + " to Linux App Service.";
                            break;
                        }
                    }
                    catch (Exception e) {
                        result = Status.Failed;
                        message = "Unable to upload " + zipFilePath 
                            + " to Linux App Service. Error=" + e.Message;
                    }

                    retryCount++;
                    if (retryCount > Constants.MAX_APPDATA_UPLOAD_RETRIES)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return new Result(result, message);
        }

        public static List<string> GetDefaultDropdownList (string displayMsg)
        {
            return new List<string>() { displayMsg };
        }
    }
}
