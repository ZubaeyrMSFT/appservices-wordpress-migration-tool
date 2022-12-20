using System;

namespace WordPressMigrationTool
{
    public class Subscription
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public Subscription(string name, string Id)
        {
            this.Name = name;
            this.Id = Id.ToLower();
        }
    }

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
        public string stackVersion { get; set; }

        public SiteInfo(string subscriptionId, string resourceGroupName, string webAppName)
        {
            this.subscriptionId = subscriptionId;
            this.resourceGroupName = resourceGroupName;
            this.webAppName = webAppName;
        }

        public override string ToString()
        {
            return "[subscriptionId=" + subscriptionId + ", resourceGroupName=" + resourceGroupName + ", webAppName=" + webAppName
                + "ftpUsername=" + ftpUsername + ", ftpPassword=" + ftpPassword + ", databaseHostName=" + databaseHostname 
                + ", databaseUsername=" + databaseUsername + ", databasePassword=" + databasePassword + ", databaseName=" + databaseName + "]";
        }
    }
}
