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

        public SiteInfo() { }

        public SiteInfo(string subscriptionId, string resourceGroupName, string webAppName)
        {
            this.subscriptionId = subscriptionId;
            this.resourceGroupName = resourceGroupName;
            this.webAppName = webAppName;
        }

        override
        public string ToString()
        {
            return "[subscriptionId=" + subscriptionId + ", resourceGroupName=" + resourceGroupName + ", webAppName=" + webAppName
                + "ftpUsername=" + ftpUsername + ", ftpPassword=" + ftpPassword + ", databaseHostName=" + databaseHostname 
                + ", databaseUsername=" + databaseUsername + ", databasePassword=" + databasePassword + ", databaseName=" + databaseName + "]";
        }
    }
}
