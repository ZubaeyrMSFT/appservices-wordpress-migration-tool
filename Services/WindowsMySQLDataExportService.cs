using System;
using System.IO;
using WordPressMigrationTool.Utilities;
using MySql.Data.MySqlClient;
using System.IO.Compression;
using System.Diagnostics;

namespace WordPressMigrationTool
{
    public class WindowsMySQLDataExportService
    {
        private string _serverHostName;
        private string _username;
        private string _password;
        private string _databaseName;
        private string? _charset;
        private bool _result = false;
        private string _message = "";
        private int _retriesCount = 0;
        private long _lastCheckpointCountForDisplay = 0;
        private RichTextBox? _progressViewRTextBox;


        public WindowsMySQLDataExportService(string serverHostName, string username, 
            string password, string databaseName, string? charset, RichTextBox? progressViewRTextBox) {

            if (string.IsNullOrWhiteSpace(serverHostName)) 
            {
                throw new ArgumentException("Invalid MySQL servername found! " +
                    "serverHostName=", serverHostName);
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Invalid MySQL username found! " +
                    "username=" +  username);
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

            this._serverHostName = serverHostName;
            this._username = username;
            this._password = password;
            this._databaseName = databaseName;
            this._charset = charset;
            this._progressViewRTextBox = progressViewRTextBox;
        }

        public Result ExportData()
        {
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            string outputSqlFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH);
            string outputZipFilePath = Environment.ExpandEnvironmentVariables(Constants.WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH);
            string mysqlConnectionString = HelperUtils.GetMySQLConnectionStringForExternalMySQLClientTool(this._serverHostName, this._username, 
                this._password, this._databaseName, this._charset);

            HelperUtils.WriteOutputWithNewLine("Exporting MySQL database from Windows WordPress to " + outputZipFilePath, this._progressViewRTextBox);
            Stopwatch timer = Stopwatch.StartNew();


            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }


            while (_retriesCount <= Constants.MAX_WIN_MYSQLDATA_RETRIES)
            {
                HelperUtils.DeleteFileIfExists(outputSqlFilePath);
                HelperUtils.DeleteFileIfExists(outputZipFilePath);

                using (MySqlConnection mConnection = new MySqlConnection(mysqlConnectionString))
                {
                    using (MySqlCommand mCommand = new MySqlCommand())
                    {
                        using (MySqlBackup mBackup = new MySqlBackup(mCommand))
                        {
                            try
                            {
                                mCommand.Connection = mConnection;
                                mConnection.Open();
                                mBackup.ExportInfo.AddDropDatabase = true;
                                mBackup.ExportInfo.AddCreateDatabase = true;
                                mBackup.ExportProgressChanged += MBackup_ExportProgressChanged;
                                mBackup.ExportCompleted += MBackup_ExportCompleted;
                                mBackup.ExportToFile(outputSqlFilePath);
                                mConnection.Close();
                            }
                            catch (Exception ex) when (ex is MySqlException | ex is InvalidOperationException)
                            {
                                this._result = false;
                                this._message = ex.Message;
                                mConnection.Close();
                            }

                            if (!this._result)
                            {
                                this._retriesCount++;
                                if (this._retriesCount > Constants.MAX_WIN_MYSQLDATA_RETRIES)
                                {
                                    timer.Stop();
                                    HelperUtils.WriteOutputWithNewLine("Unable to export MySQL data from Windows WordPress... time taken= " 
                                        + (timer.ElapsedMilliseconds / 1000)  + " seconds\n", this._progressViewRTextBox);
                                    HelperUtils.DeleteFileIfExists(outputSqlFilePath);
                                    HelperUtils.DeleteFileIfExists(outputZipFilePath);
                                    return new Result(Status.Failed, this._message);
                                }
                                else
                                {
                                    HelperUtils.WriteOutputWithNewLine("Retrying MySQL data download from Windows " +
                                        "WordPress... " + _retriesCount, this._progressViewRTextBox);
                                    continue;
                                }
                            }

                            using (ZipArchive archive = ZipFile.Open(outputZipFilePath, ZipArchiveMode.Create))
                            {
                                archive.CreateEntryFromFile(outputSqlFilePath, Path.GetFileName(outputSqlFilePath));
                            }

                            HelperUtils.DeleteFileIfExists(outputSqlFilePath);
                            break;
                        }
                    }
                }
            }

            timer.Stop();
            HelperUtils.WriteOutputWithNewLine("Sucessfully exported MySQL database from Windows WordPress..." +
                "time taken= " + (timer.ElapsedMilliseconds / 1000) + " seconds\n", this._progressViewRTextBox);

            return new Result(Status.Completed, this._message);
        }

        private void MBackup_ExportCompleted(object sender, ExportCompleteArgs e)
        {
            if (e.HasError || e.CompletionType.Equals(MySqlBackup.ProcessEndType.Error))
            {
                this._result = false;
                this._message = e.LastError.Message;
            }
            else if (e.CompletionType.Equals(MySqlBackup.ProcessEndType.Complete))
            {
                this._result = true;
                this._message = "Download Completed";
            }
            else if (e.CompletionType.Equals(MySqlBackup.ProcessEndType.Cancelled))
            {
                this._result = false;
                this._message = "Download Cancelled";
                this._retriesCount = Constants.MAX_WIN_MYSQLDATA_RETRIES + 1;
            }
            else
            {
                this._result = false;
                this._message = "Unknown Download Status";
            }
        }

        private void MBackup_ExportProgressChanged(object sender, ExportProgressArgs e)
        {
            long displayWindowSize = 1;
            if (e.CurrentTableIndex - this._lastCheckpointCountForDisplay >= displayWindowSize)
            {
                this._lastCheckpointCountForDisplay = e.CurrentTableIndex;
                if (e.CurrentTableIndex == e.TotalTables)
                {
                    string outputString = "Download completed - Finished exporting " 
                        + (e.CurrentTableIndex) + " out of " + e.TotalTables + " tables\n";
                    HelperUtils.WriteOutputWithRC(outputString, this._progressViewRTextBox);
                }
                else
                {
                    string outputString = "Download progress - Finished exporting " 
                        + (e.CurrentTableIndex) + " out of " + e.TotalTables + " tables";
                    HelperUtils.WriteOutputWithRC(outputString, this._progressViewRTextBox);
                }
            }
        }
    }
}
