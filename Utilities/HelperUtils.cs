using System;
using System.IO;

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
    }
}
