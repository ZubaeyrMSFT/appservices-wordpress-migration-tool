using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool.Utilities
{
    public static class Constants
    {
        public const string DATA_EXPORT_PATH = "%userprofile%\\AppData\\Local\\WordPressMigration\\";
        public const string WIN_APPSERVICE_DATA_EXPORT_PATH = DATA_EXPORT_PATH + "wpcontent.zip";
        public const int MAX_WIN_APPSERVICE_RETRIES = 3;
        public const string SUCESS_MESSAGE = "Migration has been completed successfully!";


        public static string getKuduApiForZipDownload(string appServiceName)
        {
            if (!string.IsNullOrWhiteSpace(appServiceName))
            {
                return "https://" + appServiceName + ".scm.azurewebsites.net/api/zip/site/wwwroot/wp-content/";
            }
            return null;
        }
    }
}
