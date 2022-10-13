using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordPressMigrationTool
{
    public class ImportService
    {
        public Result importDataToDestinationSite(SiteInfo destinationSite) {
            /*
             * 1. Check if the source data exists in the directory
             * 2. Azure Login & SSH 
             * 3. Import the wordpress data wp-content/ folder to destination site
             * 4. Import the datbase dump to destination MySQL server
             * 5. Update the datbase name in application setting of webapp
             */

            return null;
        }

        private Boolean importAppServiceData()
        {
            return false;
        }

        private Boolean importDatbaseContent()
        {
            return false;
        }
    }
}
