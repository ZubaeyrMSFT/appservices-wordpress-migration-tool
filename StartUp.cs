using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Ionic.Zip;

namespace WordPressMigrationTool
{
    internal class StartUp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Directory.CreateDirectory("C:/wpmigrate1");
            //C: \Users\saisubodh\Documents\wpmigrate\Screenshots.zip
            using (var zipFile = new Ionic.Zip.ZipFile(Encoding.UTF8))
            {
                zipFile.AddDirectory("C:\\Users\\saisubodh\\Documents\\wpmigrate", directoryPathInArchive: string.Empty);
                zipFile.MaxOutputSegmentSize = 10 * 1000000;
                zipFile.Save("C:\\Users\\saisubodh\\Documents\\test.zip");
            }
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MigrationUX());
        }
    }
}
