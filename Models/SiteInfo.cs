using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class SiteInfo
    {
        public string subscriptionId { get; set; }
        public string resourceGroupName { get; set; }
        public string webAppName { get; set; }
        public string ftpUsername { get; set; }
        public string ftpPassword { get; set; }
        public string databaseHostname { get; set; }
        public string databaseUsername { get; set; }
        public string databasePassword { get; set; }
        public string databaseName { get; set; }

    }
}
