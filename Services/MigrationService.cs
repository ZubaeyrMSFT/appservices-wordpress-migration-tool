using System;
using WordPressMigrationTool.Utilities;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

         public Result migrate(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            try
            {
                ImportService importService = new ImportService();
                ExportService exportService = new ExportService();

                Result exporttRes = exportService.ExportDataFromSourceSite(sourceSite);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    return exporttRes;
                }

                Result importRes = importService.importDataToDestinationSite(destinationSite, sourceSite.databaseName);
                if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
                {
                    return importRes;
                }

                return new Result(Status.Completed, Constants.SUCCESS_MESSAGE);
            }
            catch (Exception ex) {
                return new Result(Status.Failed, ex.Message);
            }
        }
    }
}
