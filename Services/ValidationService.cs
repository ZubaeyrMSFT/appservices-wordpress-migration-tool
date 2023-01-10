using Azure.ResourceManager.AppService;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.RegularExpressions;
using WordPressMigrationTool.Utilities;
using WordPressMigrationTool.Views;

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
        private MigrationUX _migrationUxForm;
        private List<string> _validationMessages;

        public ValidationService() { }

        public ValidationService(RichTextBox? progressViewRTextBox, string[] previousMigrationStatus, WebSiteResource sourceSiteResource, WebSiteResource destinationSiteResource, MigrationUX migrationUxForm)
        {
            this._progressViewRTextBox = progressViewRTextBox;
            this._previousMigrationStatus = previousMigrationStatus;
            this._sourceSiteResource = sourceSiteResource;
            this._destinationSiteResource = destinationSiteResource;
            this._migrationUxForm = migrationUxForm;
            this._validationMessages = new List<string>();
        }

        public Result ValidateMigrationInput(SiteInfo sourceSite, SiteInfo destinationSite)
        {
            this._sourceSiteInfo = sourceSite;
            this._destinationSiteInfo = destinationSite;

            if (string.IsNullOrWhiteSpace(sourceSite.subscriptionId))
            {
                return new Result(Status.Failed, "Source site's Subscription Id should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.resourceGroupName))
            {
                return new Result(Status.Failed, "Souce site's Resource Group should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.webAppName))
            {
                return new Result(Status.Failed, "Source site's app name should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(sourceSite.ftpUsername) || string.IsNullOrWhiteSpace(sourceSite.ftpPassword))
            {
                return new Result(Status.Failed, "Source site's ftp credentials not found!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.subscriptionId))
            {
                return new Result(Status.Failed, "Destination site Subscription Id should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.resourceGroupName))
            {
                return new Result(Status.Failed, "Destination Site Resource Group should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.webAppName))
            {
                return new Result(Status.Failed, "Destiantion Site's app name should not be empty!");
            }

            if (string.IsNullOrWhiteSpace(destinationSite.ftpUsername) || string.IsNullOrWhiteSpace(sourceSite.ftpPassword))
            {
                return new Result(Status.Failed, "Destination site's ftp credentials not found!");
            }

            try
            {
                Result result;

                result = this.ValidateLinuxSite();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                // Commented out WP version comparison since the value in wp-includes/version.php isn't accurate
                /*
                result = this.CompareWpVersion();
                if (result.status != Status.Completed)
                {
                    return result;
                }*/

                result = this.ComparePhpVersion();
                if (result.status != Status.Completed)
                {
                    return result;
                }

                return ShowValidationErrorPopup();

            } 
            catch (Exception ex)
            {
                return new Result(Status.Failed, ex.Message);
            }
        }

        // Checks if Destination site's image is an official WordPress on Linux image and validates first time installation status
        private Result ValidateLinuxSite()
        {
            HelperUtils.WriteOutputWithNewLine(String.Format("Validating destination site ({0})...", this._destinationSiteInfo.webAppName), this._progressViewRTextBox);

            string linuxFxVersion = this._destinationSiteResource.Data.SiteConfig.LinuxFxVersion;
            if (string.IsNullOrWhiteSpace(linuxFxVersion))
            {
                return new Result(Status.Failed, String.Format("Destination site {0} has an invalid stack/image selected.", this._destinationSiteInfo.webAppName));
            }

            System.Diagnostics.Debug.WriteLine("linuxfxcersion is " + linuxFxVersion);
            // Verify if the destination site uses an official WordPress on Linux image.
            if (linuxFxVersion != Constants.MCR_LATEST_IMAGE_LINUXFXVERSION && !linuxFxVersion.StartsWith(Constants.LINUXFXVERSION_PREFIX))
            {
                this._validationMessages.Add("IMAGE_INVALID");
            }
            return new Result(Status.Completed, "");
        }

        private Result CompareWpVersion()
        {
            HelperUtils.WriteOutputWithNewLine("Comparing WordPress versions...", this._progressViewRTextBox);
            
            string sourceSiteWpVersion = this.GetWpVersion("./site/wwwroot/", this._sourceSiteInfo);
            string destinationSiteWpVersion = this.GetWpVersion("/home/site/wwwroot/", this._destinationSiteInfo);
            bool isWpVersionDifferent = false;
            if (sourceSiteWpVersion != destinationSiteWpVersion)
            {
                isWpVersionDifferent = true;
            }

            // Display warning message if PHP versions are different
            if (isWpVersionDifferent)
            {
                this._validationMessages.Add("WP_VERSION");
            }

            return new Result(Status.Completed, "");
        }

        // gets WordPress version from wp-includes/version.php file for the given site
        private string GetWpVersion(string wpRootDir, SiteInfo siteInfo)
        {
            KuduCommandApiResult getVersionFileResullt = HelperUtils.ExecuteKuduCommandApi("cat " + wpRootDir + "wp-includes/version.php", siteInfo.ftpUsername,
                siteInfo.ftpPassword, siteInfo.webAppName);

            if (getVersionFileResullt.status != Status.Completed || getVersionFileResullt.exitCode != 0)
            {
                return "";
            }

            string pattern = @"\$wp_version \= '(?<value>[0-9]+(\.[0-9]+)*)'";
            foreach (Match m in Regex.Matches(getVersionFileResullt.output, pattern))
            {
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
                this._validationMessages.Add("PHP_VERSION");
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

        private Result ShowValidationErrorPopup()
        {
            ValidationPopupForm validationWarningPopup = new ValidationPopupForm(this._validationMessages, this._sourceSiteInfo, this._destinationSiteInfo);
            validationWarningPopup.StartPosition = FormStartPosition.Manual;
            validationWarningPopup.Location = new Point(this._migrationUxForm.Location.X + 50, this._migrationUxForm.Location.Y + 60);
            validationWarningPopup.ShowDialog();
            validationWarningPopup.Dispose();
            if (!validationWarningPopup.GetStatusOnClose())
            {
                return new Result(Status.Failed, "Stopping current migration.");
            }

            return new Result(Status.Completed, "");
        }
    }
}
