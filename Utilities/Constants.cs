using System;

namespace WordPressMigrationTool.Utilities
{
    public static class Constants
    {
        public const string DATA_EXPORT_PATH = "%userprofile%\\AppData\\Local\\WordPressMigration\\";
        public const string WIN_APPSERVICE_DATA_EXPORT_PATH = DATA_EXPORT_PATH + "wpcontent.zip";
        public const string WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH = DATA_EXPORT_PATH + "mysqldata.sql";
        public const string WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH = DATA_EXPORT_PATH + "mysqldata.zip";

        public const int MAX_WIN_APPSERVICE_RETRIES = 3;
        public const int MAX_WIN_MYSQLDATA_RETRIES = 3;
        public const string SUCESS_MESSAGE = "Migration has been completed successfully!";


        public static string getKuduApiForZipDownload(string appServiceName)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/zip/site/wwwroot/wp-content/";
            }
            return null;
        }

        public static string getMySQLConnectionString(string serverHostName, string username, string password, string databaseName, string charset)
        {
            if (string.IsNullOrWhiteSpace(serverHostName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(databaseName))
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
    }
}
