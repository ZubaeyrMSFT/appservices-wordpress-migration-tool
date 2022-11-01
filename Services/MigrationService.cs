using System;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    public class MigrationService
    {

        private SiteInfo _sourceSiteInfo;
        private SiteInfo _destinationSiteInfo;
        private RichTextBox? _progressViewRTextBox;

        public MigrationService(SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo, RichTextBox? progressViewRTextBox) { 
            this._sourceSiteInfo = sourceSiteInfo;
            this._destinationSiteInfo = destinationSiteInfo;
            this._progressViewRTextBox = progressViewRTextBox;
        }

        public Result Migrate()
        {
            try
            {
                ExportService exportService = new ExportService(this._progressViewRTextBox);
                ImportService importService = new ImportService(this._progressViewRTextBox);

                Result exporttRes = exportService.ExportDataFromSourceSite(this._sourceSiteInfo);
                if (exporttRes.status == Status.Failed || exporttRes.status == Status.Cancelled)
                {
                    return exporttRes;
                }

                Result importRes = importService.ImportDataToDestinationSite(this._destinationSiteInfo, this._sourceSiteInfo.databaseName);
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

        public void MigrateAsyncForWinUI()
        {
            try
            {
                Result res = this.Migrate();
                if (res.status == Status.Failed || res.status == Status.Cancelled)
                {
                    HelperUtils.WriteOutputWithNewLine(res.message, this._progressViewRTextBox);
                    MessageBox.Show(res.message, "Failed!");
                }
            }
            catch (Exception ex)
            {
                HelperUtils.WriteOutputWithNewLine(ex.Message, this._progressViewRTextBox);
                MessageBox.Show(ex.Message, "Failed!");
            }
        }
    }
}
