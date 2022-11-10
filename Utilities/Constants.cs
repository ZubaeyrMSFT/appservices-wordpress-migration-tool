using System;
using System.Reflection.Metadata;

namespace WordPressMigrationTool.Utilities
{
    public static class Constants
    {
        public const int KUDU_ZIP_API_MAX_UPLOAD_LIMIT = 100000000;     // 100 Million Bytes

        public const string WIN_WPCONTENT_ZIP_FILENAME = "wpcontent.zip";
        public const string WIN_MYSQL_ZIP_FILENAME = "mysqldata.zip";

        public const string DATA_EXPORT_PATH = "%userprofile%\\AppData\\Local\\WordPressMigration\\";
        public const string WIN_APPSERVICE_DATA_EXPORT_PATH = DATA_EXPORT_PATH + WIN_WPCONTENT_ZIP_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH = DATA_EXPORT_PATH + WIN_MYSQL_ZIP_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH = DATA_EXPORT_PATH + "mysqldata.zip";

        public const int MAX_WIN_APPSERVICE_RETRIES = 3;
        public const int MAX_WIN_MYSQLDATA_RETRIES = 3;
        public const int MAX_APPDATA_UPLOAD_RETRIES = 3;
        public const int MAX_MYSQLDATA_UPLOAD_RETRIES = 3;
        public const int MAX_WPCONTENT_CLEAR_RETRIES = 10;
        public const int MAX_RETRIES_COMMON = 3;
        public const int MAX_APP_CLEAR_DIR_RETRIES = 10;

        public const string SUCCESS_EXPORT_MESSAGE = "Successfully exported the data from Windows WordPress!";
        public const string SUCCESS_IMPORT_MESSAGE = "Successfully imported the data to Linux WordPress!";
        public const string SUCCESS_MESSAGE = "Migration has been completed successfully!";

        public const string APPSETTING_DATABASE_HOST = "DATABASE_HOST";
        public const string APPSETTING_DATABASE_NAME = "DATABASE_NAME";
        public const string APPSETTING_DATABASE_USERNAME = "DATABASE_USERNAME";
        public const string APPSETTING_DATABASE_PASSWORD = "DATABASE_PASSWORD";

        // below value should not end with '/'
        public const string LIN_APP_SVC_WPCONTENT_DIR = "/home/site/wwwroot/wp-content";
        public const string LIN_APP_SVC_ROOT_DIR = "/home/site/wwwroot/";
        public const string LIN_APP_WORDPRESS_SRC_CODE_DIR = "/usr/src/wordpress/";
        public const string LIN_MYSQL_DUMP_UPLOAD_PATH_FOR_KUDU_API = "dev/migrate/mysql";

        public const string WPCONTENT_SPLIT_ZIP_FILES_DIR = DATA_EXPORT_PATH + "wpContentSplitDir\\";
        public const string WPCONTENT_SPLIT_ZIP_FILE_NAME_PREFIX = "WpContentSplit";
        public const string WPCONTENT_SPLIT_ZIP_FILE_NAME = WPCONTENT_SPLIT_ZIP_FILE_NAME_PREFIX + ".zip";
        public const string WPCONTENT_SPLIT_ZIP_FILE_PATH = WPCONTENT_SPLIT_ZIP_FILES_DIR + WPCONTENT_SPLIT_ZIP_FILE_NAME;
        public const string WPCONTENT_SPLIT_ZIP_NESTED_DIR = DATA_EXPORT_PATH + "ZippedWpContentSplitFiles\\";

        public const string MYSQL_TEMP_DIR = LIN_APP_SVC_MIGRATE_DIR + "mysql/";

        public const string LIN_APP_SVC_MIGRATE_DIR = "/home/dev/migrate/";
        public const string WPCONTENT_TEMP_DIR = LIN_APP_SVC_MIGRATE_DIR + "wpcontentsplit/";
        public const string WPCONTENT_TEMP_DIR_KUDU_API = "dev/migrate/wpcontentsplit/";
        public const string WPCONTENT_TEMP_ZIP_PATH = LIN_APP_SVC_MIGRATE_DIR + "wp-content-temp.zip";
        public const string WPCONTENT_CREATE_TEMP_DIR_COMMAND = "mkdir -p " + WPCONTENT_TEMP_DIR;
        public const string WPCONTENT_MERGE_SPLLIT_FILES_COMAMND = "zip -FF " + WPCONTENT_TEMP_DIR + WPCONTENT_SPLIT_ZIP_FILE_NAME_PREFIX + ".zip --out " + WPCONTENT_TEMP_ZIP_PATH;
        public const string UNZIP_MERGED_WPCONTENT_COMMAND = "yes | unzip " + WPCONTENT_TEMP_ZIP_PATH + " -d " + LIN_APP_SVC_WPCONTENT_DIR;

        public const string MYSQL_CREATE_TEMP_DIR_COMMAND = "mkdir -p " + MYSQL_TEMP_DIR;
        public const string CLEAR_APP_SERVICE_DIR_COMMAND = "rm -rf {0}";
        public const string LIST_DIR_COMMAND = "ls {0}";
        public const string LIN_APP_MAKE_DIR_COMMAND = "mkdir -p {0}";

        public const string START_MIGRATION_APP_SETTING = "MIGRATION_IN_PROGRESS";
        public const string NEW_DATABASE_NAME_APP_SETTING = "MIGRATE_NEW_DATABASE_NAME";
        public const string MYSQL_DUMP_FILE_PATH_APP_SETTING = "MIGRATE_MYSQL_DUMP_PATH";
        public const string LIN_APP_PREVENT_WORDPRESS_INSTALL_APP_SETTING = "SKIP_WP_INSTALLATION";

        public const string DB_IMPORT_SUCCESS_MESSAGE = "MYSQL_DB_IMPORT_COMPLETED";
        public const string DB_IMPORT_FAILURE_MESSAGE = "MYSQL_DB_IMPORT_FAILED";
        public const string LIN_APP_DB_STATUS_FILE_PATH = "/home/dev/migrate/mysql/mysql_import_status.txt";

    }
}
