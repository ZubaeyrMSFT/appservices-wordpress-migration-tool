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
        private WebSiteResource _sourceSiteResource;
        private WebSiteResource _destinationSiteResource;

        public ValidationService() { }

        public ValidationService(RichTextBox? progressViewRTextBox, string[] previousMigrationStatus, WebSiteResource sourceSiteResource, WebSiteResource destinationSiteResource)
        {
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
            this._sourceSiteResource = sourceSiteResource;
            this._destinationSiteResource = destinationSiteResource;
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

                result = this.ValidateLinuxSite();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                result = this.CompareWpVersion();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                result = this.ComparePhpVersion();
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

        private Result ValidateLinuxSite()
        {
            HelperUtils.WriteOutputWithNewLine(String.Format("Validating destination site ({0})...", this._destinationSiteInfo.webAppName), this._progressViewRTextBox);

            string linuxFxVersion = this._destinationSiteResource.Data.SiteConfig.LinuxFxVersion;
            if (string.IsNullOrWhiteSpace(linuxFxVersion))
            {
                return new Result(Status.Failed, String.Format("Destination site {0} has an invalid stack/image selected.", this._destinationSiteInfo.webAppName));
            }

            // Verify if the destination site uses an official WordPress on Linux image.
            if (linuxFxVersion != Constants.MCR_LATEST_IMAGE_LINUXFXVERSION && !linuxFxVersion.StartsWith(Constants.LINUXFXVERSION_PREFIX))
            {
                string message = String.Format("The destination site ({0}) doesn't use an official WordPress on Linux image. This may cause the migration to fail. Do you want to continue?", this._destinationSiteInfo.webAppName);

                string caption = "Invalid Image Detected!";

                var result = MessageBox.Show(message, caption,
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return new Result(Status.Failed, "Chosen to discontinue current migration.");
                }
            }

            // Verify if the destination WordPress on Linux site has finished first time installation of WordPress
            string getStatusFileCommand = String.Format("cat {0}", Constants.LIN_APP_WP_DEPLOYMENT_STATUS_FILE_PATH);
            KuduCommandApiResult kuduCommandApiResult= HelperUtils.ExecuteKuduCommandApi(getStatusFileCommand, this._destinationSiteInfo.ftpUsername, this._destinationSiteInfo.ftpPassword, this._destinationSiteInfo.webAppName);
            if (kuduCommandApiResult.status != Status.Completed || kuduCommandApiResult.exitCode != 0 || !kuduCommandApiResult.output.Contains(Constants.FIRST_TIME_SETUP_COMPLETETED_MESSAGE))
            {
                string message = String.Format("The destination site ({0}) hasn't finished installing WordPress. It is advised to restart the site and wait for 5-10 minutes before trying again. Do you still want to continue?", this._destinationSiteInfo.webAppName);

                string caption = "Incomplete WordPress installation detected!";

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

        private Result CompareWpVersion()
        {
            HelperUtils.WriteOutputWithNewLine("Comparing WordPress versions...", this._progressViewRTextBox);
            
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

        private Result ComparePhpVersion()
        {
            HelperUtils.WriteOutputWithNewLine("Comparing PHP versions...", this._progressViewRTextBox);

            string sourceSitePhpVersion = this._sourceSiteResource.HasData ? this._sourceSiteResource.Data.SiteConfig.PhpVersion : null;
            string destinationSitePhpVersion = this.GetWpLinuxAppPhpVersion(this._destinationSiteResource);

            if (String.IsNullOrEmpty(sourceSitePhpVersion) || String.IsNullOrEmpty(destinationSitePhpVersion) || sourceSitePhpVersion != destinationSitePhpVersion)
            {
                string message = String.Format("Source site ({0}) and destination site use different PHP versions. This may lead to incompatibilities with themes/plugins after migration. Do you want continue?", this._destinationSiteInfo.webAppName);

                string caption = "Different PHP versions detected!";

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

        private string GetWpLinuxAppPhpVersion(WebSiteResource wpLinuxSiteResource)
        {
            if (!wpLinuxSiteResource.HasData)
            {
                return null;
            }

            string linuxFxVersion = wpLinuxSiteResource.Data.SiteConfig.LinuxFxVersion;
            if (linuxFxVersion == Constants.MCR_LATEST_IMAGE_LINUXFXVERSION)
            {
                // PHP version of "latest" tag in MCR will always be 8.0
                return "8.0";
            }

            if (linuxFxVersion.StartsWith(Constants.LINUXFXVERSION_PREFIX))
            {
                return linuxFxVersion.Substring(Constants.LINUXFXVERSION_PREFIX.Length);
            }

            return null;
        }
    }
}
