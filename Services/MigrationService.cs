using System;
using System.Threading.Tasks;
using WordPressMigrationTool.Utilities;


namespace WordPressMigrationTool
{
    public class MigrationService
    {

        public async Task<Result> migrate(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            ImportService importService = new ImportService();
            ExportService exportService = new ExportService();

            Result exporttRes = exportService.exportDataFromSourceSite(sourceSite);
            if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
            {
                return exporttRes;
            }

            Result importRes = await importService.importDataToDestinationSite(destinationSite, sourceSite.databaseName);
            if (importRes.status == Status.Failed || importRes.status == Status.Cancelled)
            {
                return importRes;
            }

            return new Result(Status.Completed, Constants.SUCCESS_MESSAGE);
        }
    }
}
