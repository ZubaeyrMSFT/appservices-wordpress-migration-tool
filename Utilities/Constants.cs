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

    }
}
