using Azure.ResourceManager.AppService;
using MySqlX.XDevAPI.Common;
using Renci.SshNet.Security;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WordPressMigrationTool.Utilities;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static WordPressMigrationTool.Utilities.Constants;

namespace WordPressMigrationTool
{
    public class ValidationService
    {
        private RichTextBox? _progressViewRTextBox;
        private string[] _previousMigrationStatus;
        private SiteInfo _sourceSiteInfo;
        private SiteInfo _destinationSiteInfo;

        public ValidationService() { }

        public ValidationService(RichTextBox? progressViewRTextBox, string[] previousMigrationStatus)
        {
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
        }

        public Result ValidateMigrationInput(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            this._sourceSiteInfo = sourceSite;
            this._destinationSiteInfo = destinationSite;

            if (string.IsNullOrWhiteSpace(sourceSite.subscriptionId))
            {
                return new Result(Status.Failed, "Subscription Id should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.resourceGroupName))
            {
                return new Result(Status.Failed, "Resource Group should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.webAppName))
            {
                return new Result(Status.Failed, "App Service name should not be empty!");
            }

            try
            {
                Result result;

                result = this.CompareWpVersion();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                return result;
            } 
            catch (Exception ex)
            {
                return new Result(Status.Failed, ex.Message);
            }
        }

        private Result CompareWpVersion()
        {
            string message = "";

            string sourceSiteWpVersion = this.GetWpVersion("./site/wwwroot/", this._sourceSiteInfo);
            string destinationSiteWpVersion = this.GetWpVersion("/home/site/wwwroot/", this._destinationSiteInfo);
            bool isWpVersionDifferent = false;
            if (sourceSiteWpVersion != destinationSiteWpVersion)
            {
                isWpVersionDifferent = true;
            }

            if (isWpVersionDifferent)
            {
                message = String.Format("The WordPress version of source site ({0}) is different from that of desitnation site ({1}). " +
                   "Your plugins/themes maybe incompatible with the new site. Do you want to continue", sourceSiteWpVersion, destinationSiteWpVersion);
                string caption = "WordPress Version Conflict Detected!";

                var result = MessageBox.Show(message, caption,
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return new Result(Status.Failed, "Chosen to discontinue current migration.");
                }
            }

            return new Result(Status.Completed, "");
        }

        private string GetWpVersion(string wpRootDir, SiteInfo siteInfo)
        {
            KuduCommandApiResult getVersionFileResullt = HelperUtils.ExecuteKuduCommandApi("cat " + wpRootDir + "wp-includes/version.php", siteInfo.ftpUsername,
                siteInfo.ftpPassword, siteInfo.webAppName);

            if (getVersionFileResullt.status != Status.Completed || getVersionFileResullt.exitCode != 0)
            {
                return "";
            }

            System.Diagnostics.Debug.WriteLine("cat output is : " + getVersionFileResullt.output);
            string pattern = @"\$wp_version \= '(?<value>[0-9]+(\.[0-9]+)*)'";
            foreach (Match m in Regex.Matches(getVersionFileResullt.output, pattern))
            {
                System.Diagnostics.Debug.WriteLine("wordpress version is: " + m.Groups["value"].Value);
                return m.Groups["value"].Value;
            }

            return "";
        }
    }
}
