using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class SiteInfo
    {
        public String webAppName { get; set; }
        public String resourceGroupName { get; set; }
        public String wordpressPassword { get; set; }
        public String wordpressUserName { get; set; }
        public String databaseHostname { get; set; }
        public String databaseUsername { get; set; }
        public String databasePassword { get; set; }
        public String databaseName { get; set; }

    }
}
