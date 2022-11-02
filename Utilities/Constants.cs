using System;

namespace WordPressMigrationTool.Utilities
{
    public static class Constants
    {
        public const string WIN_WPCONTENT_ZIP_FILENAME = "wpcontent.zip";
        public const string WIN_MYSQL_ZIP_FILENAME = "mysqldata.zip";
        public const string DATA_EXPORT_PATH = "%userprofile%\\AppData\\Local\\WordPressMigration\\";
        public const string WIN_APPSERVICE_DATA_EXPORT_PATH = DATA_EXPORT_PATH + WIN_WPCONTENT_ZIP_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH = DATA_EXPORT_PATH + WIN_MYSQL_ZIP_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH = DATA_EXPORT_PATH + "mysqldata.zip";
        public const string SPLIT_ZIP_FILES_DIR = DATA_EXPORT_PATH + "SplitZipFiles\\";

        public const string LIN_MYSQL_DUMP_UPLOAD_PATH_FOR_KUDU_API = "dev/migrate";

        public const int MAX_WIN_APPSERVICE_RETRIES = 3;
        public const int MAX_WIN_MYSQLDATA_RETRIES = 3;
        public const int MAX_APPDATA_UPLOAD_RETRIES = 3;
        public const int MAX_MYSQLDATA_UPLOAD_RETRIES = 3;
        public const int MAX_WPCONTENT_CLEAR_RETRIES = 10;
        public const int MAX_RETRIES_COMMON = 3;
        public const string SUCCESS_EXPORT_MESSAGE = "Successfully exported the data from Windows WordPress!";
        public const string SUCCESS_IMPORT_MESSAGE = "Successfully imported the data to Linux WordPress!";
        public const string SUCCESS_MESSAGE = "Migration has been completed successfully!";

        public const string APPSETTING_DATABASE_HOST = "DATABASE_HOST";
        public const string APPSETTING_DATABASE_NAME = "DATABASE_NAME";
        public const string APPSETTING_DATABASE_USERNAME = "DATABASE_USERNAME";
        public const string APPSETTING_DATABASE_PASSWORD = "DATABASE_PASSWORD";

        public const string MYSQL_CREATE_TEMP_DIR_COMMAND = "mkdir -p /home/dev/migrate";
        public const string MYSQL_CLEAN_WPCONTENT_DIR_COMMAND = "rm -rf /home/site/wwwroot/wp-content/*";
        public const string MYSQL_PACKAGE_INSTALL = "apk add mysql-client --no-cache";
        public const string MYSQL_DUMP_IMPORT = "mysql -u {0} -p'{1}' -h ${2} --ssl=true -e ${3} < ${4}.sql";

    }
}
