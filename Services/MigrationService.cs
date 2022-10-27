﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

        public Result migrate(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            ImportService importService = new ImportService();
            ExportService exportService = new ExportService();

            Result exporttRes = exportService.exportDataFromSourceSite(sourceSite);
            if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
            {
                return exporttRes;
            }

            Result importRes = importService.importDataToDestinationSite(destinationSite);
            if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
            {
                return importRes;
            }

            return new Result(Status.Completed, Constants.SUCESS_MESSAGE);
        }
    }
}
