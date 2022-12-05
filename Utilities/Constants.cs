using System;

namespace WordPressMigrationTool.Utilities
{
    public static class Constants
    {
        public const string AZURE_PORTAL_URL = "https://ms.portal.azure.com/#create/WordPress.WordPress";

        public const int KUDU_ZIP_API_MAX_UPLOAD_LIMIT = 100000000;     // 100 Million Bytes
        public const string WIN_WPCONTENT_ZIP_FILENAME = "wpcontent.zip";
        public const string WIN_MYSQL_ZIP_FILENAME = "mysqldata.zip";
        public const string WIN_MYSQL_SQL_FILENAME = "mysqldata.sql";

        public const string DATA_EXPORT_PATH = "%userprofile%\\AppData\\Local\\WordPressMigration\\";
        public const string WIN_APPSERVICE_DATA_EXPORT_PATH = DATA_EXPORT_PATH + WIN_WPCONTENT_ZIP_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_SQLFILE_PATH = DATA_EXPORT_PATH + WIN_MYSQL_SQL_FILENAME;
        public const string WIN_MYSQL_DATA_EXPORT_COMPRESSED_SQLFILE_PATH = DATA_EXPORT_PATH + WIN_MYSQL_ZIP_FILENAME;
        public const string MIGRATION_STATUSFILE_PATH = DATA_EXPORT_PATH + "migration_status.txt";

        public const int MAX_WIN_APPSERVICE_RETRIES = 3;
        public const int MAX_WIN_MYSQLDATA_RETRIES = 3;
        public const int MAX_APPDATA_UPLOAD_RETRIES = 3;
        public const int MAX_MYSQLDATA_UPLOAD_RETRIES = 3;
        public const int MAX_WPCONTENT_CLEAR_RETRIES = 3;
        public const int MAX_RETRIES_COMMON = 3;
        public const int MAX_APP_CLEAR_DIR_RETRIES = 3;

        public const string SUCCESS_EXPORT_MESSAGE = "Successfully exported the data from Windows WordPress!";
        public const string SUCCESS_IMPORT_MESSAGE = "Successfully imported the data to Linux WordPress!";
        public const string SUCCESS_MESSAGE = "Migration has been completed successfully!";

        public const string APPSETTING_DATABASE_HOST = "DATABASE_HOST";
        public const string APPSETTING_DATABASE_NAME = "DATABASE_NAME";
        public const string APPSETTING_DATABASE_USERNAME = "DATABASE_USERNAME";
        public const string APPSETTING_DATABASE_PASSWORD = "DATABASE_PASSWORD";

        public const string LIN_APP_WP_CONFIG_PATH = "/home/site/wwwroot/wp-config.php";
        public const string LIN_APP_VERSIONPHP_FILE_PATH = "/home/site/wwwroot/wp-includes/version.php";
        public const string LIN_APP_WP_DEPLOYMENT_STATUS_FILE_PATH = "/home/wp-locks/wp_deployment_status.txt";
        // below value should not end with '/'
        public const string LIN_APP_SVC_WPCONTENT_DIR = "/home/site/wwwroot/wp-content";
        public const string LIN_APP_SVC_ROOT_DIR = "/home/site/wwwroot/";
        public const string LIN_APP_WORDPRESS_SRC_CODE_DIR = "/usr/src/wordpress/wordpress-azure/";
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

        public const string FIRST_TIME_SETUP_COMPLETETED_MESSAGE = "FIRST_TIME_SETUP_COMPLETED";
        public const string IMPORT_SUCCESS_MESSAGE = "IMPORT_POST_PROCESSING_COMPLETED";
        public const string IMPORT_FAILURE_MESSAGE = "IMPORT_POST_PROCESSING_FAILED";
        public const string LIN_APP_DB_STATUS_FILE_PATH = "/home/dev/migrate/import_status.txt";

        public static class StatusMessages
        {
            public const string sourceSiteName = "Source Site name : ";
            public const string sourceSiteResourceGroup = "Source Site ResourceGroup : ";
            public const string sourceSiteSubscription = "Source Site Subscription : ";
            public const string destinationSiteName = "Destination Site Name : ";
            public const string destinationSiteResourceGroup = "Destination Site ResourceGroup : ";
            public const string destinationSiteSubscription = "Destination Site Subscription : ";
            public const string migrationFailed = "MIGRATION_FAILED";
            public const string migrationCompleted = "MIGRATION_COMPLETED";
            public const string exportCompleted = "EXPORT_COMPLETED";
            public const string importCompleted = "IMPORT_COMPLETED";
            public const string exportAppDataCompleted = "EXPORT_APP_DATA_COMPLETED";
            public const string exportDbDataCompleted = "EXPORT_DB_DATA_COMPLETED";
            public const string clearImportFilesLocalDir = "CLEAR_LOCAL_FILES_IMPORT_DIR_COMPLETED";
            public const string triggerDestinationSiteMigrationState = "TRIGGER_DESTINATION_SITE_MIGRATION_STATE";
            public const string clearMigrationDirInDestinationSite = "CLEAR_MIGRATION_DIR_IN_DESTINATION_SITE_{0}_COMPLETED";
            public const string validateWPRootDirInDestinationSite = "VALIDATED_WP_INSTALLATION_IN_DESTINATION_SITE";
            public const string importAppServiceDataCompleted = "IMPORT_APP_SERVICE_DATA_COMPLETED";
            public const string importDatabaseContentCompleted = "IMPORT_DATABASE_COMPLETED";
            public const string updateDatabaseNameAppSetting = "UPDATED_DATABASE_NAME_APP_SETTING";
            public const string postProcessingImportCompleted = "POST_PROCESSING_IMPORT_COMPLETED";
            public const string splitWpContentZipCompleted = "SPLIT_WP_CONTENT_ZIP_COMPLETED";
            public const string uploadAppDataSplitZipFilesCompleted = "UPLOAD_APP_DATA_SPLIT_ZIP_FILES_COMPLETED";
            public const string uploadAppDataSplitZipFileCompleted = "UPLOADED_APP_DATA_SPLIT_ZIP_FILE_{0}";
            public const string deleteAppDataSplitZipFilesInDestinationApp = "DELETE_APP_DATA_SPLIT_ZIP_FILES_IN_DESTINATION_APP";
            public const string extractAppDataZipInDestinationApp = "EXTRACT_APP_DATA_ZIP_IN_DESTINATION_APP";
            
        }
    }
}
