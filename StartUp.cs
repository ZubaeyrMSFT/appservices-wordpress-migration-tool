using System.Diagnostics;
using System.Text;
using Renci.SshNet;
using Ionic.Zip;
namespace WordPressMigrationTool
{
    internal static class StartUp
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            ////// To customize application configuration such as set high DPI settings or default font,
            ////// see https://aka.ms/applicationconfiguration.
            ////ApplicationConfiguration.Initialize();
            ////Application.Run(new MigrationUX());
            
            if (args.Length < 6)
            {
                Console.WriteLine("Insufficient input data! Please provide all " +
                    "the required parameters. Expected=6, Received=" + args.Length);
                return;
            }

            SiteInfo sourceSiteInfo = new SiteInfo(args[0], args[1], args[2]);
            SiteInfo destinationSiteInfo = new SiteInfo(args[3], args[4], args[5]);
            Console.WriteLine(new MigrationService().migrate(sourceSiteInfo, destinationSiteInfo));
        }
    }
}