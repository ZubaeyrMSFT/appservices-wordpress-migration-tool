namespace WordPressMigrationTool
{
    internal static class StartUp
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new MigrationUX());

            SiteInfo siteInfo = new SiteInfo();
            siteInfo.subscriptionId = "b57e15e1-5109-4368-b4e1-ad5230344ad8";
            siteInfo.resourceGroupName = "wpwinmig1-rg";
            siteInfo.webAppName = "wpwinmig1-app";
            Console.WriteLine(new ExportService().ExportDataFromSourceSite(siteInfo));
        }
    }
}