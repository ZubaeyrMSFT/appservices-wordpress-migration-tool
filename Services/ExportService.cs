﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Linq.Expressions;
using System.Threading;
using System.IO;

namespace WordPressMigrationTool
{
    public class ExportService
    {

        public Result exportDataFromSourceSite(SiteInfo sourceSite)
        {
            /*
             * 1. Create or clean up the temporary data directory
             * 2. Azure Login & SSH to the App if required
             * 3. Export the wordpress data wp-content/ folder
             * 4. Export the datbase dump from MySQL server 
             */
            return null;
        }

        public Boolean exportAppServiceData()
        {
            return false;
        }

        public Boolean exportDatbaseContent()
        {
            return false;
        }
    }
}
