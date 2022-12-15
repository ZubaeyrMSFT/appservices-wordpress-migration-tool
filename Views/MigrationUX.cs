using Azure.ResourceManager.Resources;
using MySqlX.XDevAPI.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WordPressMigrationTool.Utilities;
using WordPressMigrationTool.Views;

namespace WordPressMigrationTool
{
    public partial class MigrationUX : Form
    {
        private Thread? _childThread;
        private ProgressUX progressViewUX;
        private List<Subscription> LinSubscriptions;
        private List<Subscription> WinSubscriptions;
        private BackgroundWorker _linSubscriptionChangeWorker;
        private BackgroundWorker _winSubscriptionChangeWorker;
        private BackgroundWorker _linRgChangeWorker;
        private BackgroundWorker _winRgChangeWorker;
        private SubscriptionCollection _subscriptions;

        public MigrationUX()
        {
            GetSubscriptions();
            InitializeBackgroundWorkers();
            InitializeComponent();
            this.progressViewUX = new ProgressUX();
            progressViewUX.Hide();

            this.Show();
            this.InitializeMigrationStatusFile();
        }

        public void InitializeBackgroundWorkers()
        {
            this._linSubscriptionChangeWorker = new BackgroundWorker();
            this._linSubscriptionChangeWorker.WorkerSupportsCancellation = true;
            this._linSubscriptionChangeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.linSubscriptionChangeWorker_DoWork);
            this._linSubscriptionChangeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.linSubscriptionChangeWorker_RunWorkerCompleted);

            this._linRgChangeWorker = new BackgroundWorker();
            this._linRgChangeWorker.WorkerSupportsCancellation = true;
            this._linRgChangeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.linRgChangeWorker_DoWork);
            this._linRgChangeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.linRgChangeWorker_RunWorkerCompleted);

            this._winSubscriptionChangeWorker = new BackgroundWorker();
            this._winSubscriptionChangeWorker.WorkerSupportsCancellation = true;
            this._winSubscriptionChangeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.winSubscriptionChangeWorker_DoWork);
            this._winSubscriptionChangeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.winSubscriptionChangeWorker_RunWorkerCompleted); 

            this._winRgChangeWorker = new BackgroundWorker();
            this._winRgChangeWorker.WorkerSupportsCancellation = true;
            this._winRgChangeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.winRgChangeWorker_DoWork);
            this._winRgChangeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.winRgChangeWorker_RunWorkerCompleted);
        }

        private void GetSubscriptions()
        {
            this._subscriptions = AzureManagementUtils.GetSubscriptions();

            this.LinSubscriptions = new List<Subscription>();
            this.WinSubscriptions = new List<Subscription>();

            foreach (SubscriptionResource subscription in this._subscriptions)
            {
                if (subscription == null || !subscription.HasData)
                {
                    continue;
                }
                System.Diagnostics.Debug.WriteLine("displayname is " + subscription.Data.DisplayName);
                this.LinSubscriptions.Add(new Subscription(subscription.Data.DisplayName, subscription.Data.SubscriptionId));
                this.WinSubscriptions.Add(new Subscription(subscription.Data.DisplayName, subscription.Data.SubscriptionId));
            }
            System.Diagnostics.Debug.WriteLine("number of subscriptionNames is " + this.LinSubscriptions.Count().ToString());

            this.LinSubscriptions.Sort((x, y) => x.Name.CompareTo(y.Name));
            this.LinSubscriptions.Insert(0, new Subscription("Select a Subscription", ""));

            this.WinSubscriptions.Sort((x, y) => x.Name.CompareTo(y.Name));
            this.WinSubscriptions.Insert(0, new Subscription("Select a Subscription", ""));
        }

        private string getSubscriptionResourceId (string subscriptionId)
        {
            return "/subscriptions/" + subscriptionId;
        }

        private void winSubscriptionChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string subscriptionId = e.Argument as string;

            var resourceGroups = AzureManagementUtils.GetResourceGroupsInSubscription(subscriptionId, this._subscriptions);
            resourceGroups.Sort();
            resourceGroups.Insert(0, "Select a Resource Group");
            
            e.Result = resourceGroups;
        }
        private void winSubscriptionChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winSubscriptionChangeWorker_RunWorkerCompleted result is " + e.Result.ToString());
            if (e.Result != null)
            {
                this.winResourceGroupComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.winResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            }

            this.enableWindowsDropdowns(true);
        }

        private void winSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winSubscriptionComboBox_SelectedIndexChanged");
            if (this.winSubscriptionComboBox.SelectedValue == null)
                return;

            string subscriptionId = this.winSubscriptionComboBox.SelectedValue.ToString();

            this.enableWindowsDropdowns(false);
            this._winSubscriptionChangeWorker.RunWorkerAsync(subscriptionId);
        }

        private void linSubscriptionChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string subscriptionId = e.Argument as string;

            var resourceGroups = AzureManagementUtils.GetResourceGroupsInSubscription(subscriptionId, this._subscriptions);
            resourceGroups.Sort();
            resourceGroups.Insert(0, "Select a Resource Group");

            e.Result = resourceGroups;
        }

        private void linSubscriptionChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winSubscriptionChangeWorker_RunWorkerCompleted result is " + e.Result.ToString());
            if (e.Result != null)
            {
                this.linuxResourceGroupComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.linuxResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            }

            this.enableLinuxDropdowns(true);
        }

        private void linuxSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("winSubscriptionComboBox_SelectedIndexChanged");
            if (this.linuxSubscriptionComboBox.SelectedValue == null)
                return;

            string subscriptionId = this.linuxSubscriptionComboBox.SelectedValue.ToString();

            this.enableLinuxDropdowns(false);
            this._linSubscriptionChangeWorker.RunWorkerAsync(subscriptionId);
        }

        private void winRgChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> arguments = e.Argument as List<string>;
            string subscriptionId = arguments[0];
            string resourceGroupName = arguments[1];

            List<string> appServices = AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName, this._subscriptions);
            appServices.Sort();
            appServices.Insert(0, "Select a WordPress on Windows app");

            e.Result = appServices;
        }

        private void winRgChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winRgChangeWorker_RunWorkerCompleted result is " + e.Result.ToString());
            if (e.Result != null)
            {
                this.winAppServiceComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.linuxResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            }

            this.enableWindowsDropdowns(true);
        }

        private void winResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winResourceGroupComboBox_SelectedIndexChanged");

            if (this.winSubscriptionComboBox.SelectedValue == null || this.winResourceGroupComboBox.SelectedValue == null)
                return;

            if (this.winResourceGroupComboBox.SelectedIndex == 0)
            {
                this.winAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Windows app");
                return;
            }

            List<string> parameters = new List<string>() { this.winSubscriptionComboBox.SelectedValue.ToString(), this.winResourceGroupComboBox.SelectedValue.ToString() };

            this.enableWindowsDropdowns(false);
            this._winRgChangeWorker.RunWorkerAsync(parameters);
        }

        private void linRgChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> arguments = e.Argument as List<string>;
            string subscriptionId = arguments[0];
            string resourceGroupName = arguments[1];

            List<string> appServices = AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName, this._subscriptions);
            appServices.Sort();
            appServices.Insert(0, "Select a WordPress on Linux app");

            e.Result = appServices;
        }

        private void linRgChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winRgChangeWorker_RunWorkerCompleted result is " + e.Result.ToString());
            if (e.Result != null)
            {
                this.linuxAppServiceComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.linuxAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            }

            this.enableLinuxDropdowns(true);
        }

        private void linuxResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("linuxResourceGroupComboBox_SelectedIndexChanged");

            if (this.linuxSubscriptionComboBox.SelectedValue == null || this.linuxResourceGroupComboBox.SelectedValue == null)
                return;

            if (this.linuxResourceGroupComboBox.SelectedIndex == 0)
            {
                this.linuxResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Linux app");
                return;
            }

            List<string> parameters = new List<string>() { this.linuxSubscriptionComboBox.SelectedValue.ToString(), this.linuxResourceGroupComboBox.SelectedValue.ToString() };

            this.enableLinuxDropdowns(false);
            this._linRgChangeWorker.RunWorkerAsync(parameters);
        }

        public void enableWindowsDropdowns(bool isEnabled)
        {
            this.winSubscriptionComboBox.Enabled = isEnabled;
            this.winResourceGroupComboBox.Enabled = isEnabled;
            this.winAppServiceComboBox.Enabled = isEnabled;
            this.migrateButton.Enabled = isEnabled;
        }

        public void enableLinuxDropdowns(bool isEnabled)
        {
            this.linuxAppServiceComboBox.Enabled = isEnabled;
            this.linuxSubscriptionComboBox.Enabled = isEnabled;
            this.linuxResourceGroupComboBox.Enabled = isEnabled;
            this.migrateButton.Enabled = isEnabled;
        }


        private void cancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                this._childThread?.Interrupt();
            }
            catch { }
            this.Close();
        }

        private void migrateButton_Click(object sender, EventArgs e)
        {
            string winSubscriptionId = this.winSubscriptionComboBox.SelectedValue?.ToString();
            string winResourceGroupName = this.winResourceGroupComboBox.SelectedValue?.ToString();
            string winAppServiceName = this.winAppServiceComboBox.SelectedValue?.ToString();

            string linuxSubscriptionId = this.linuxSubscriptionComboBox.SelectedValue?.ToString();
            string linuxResourceGroupName = this.linuxResourceGroupComboBox.SelectedValue?.ToString();
            string linuxAppServiceName = this.linuxAppServiceComboBox.SelectedValue?.ToString();

            if (string.IsNullOrWhiteSpace(winSubscriptionId) || string.IsNullOrWhiteSpace(winResourceGroupName) || string.IsNullOrWhiteSpace(winAppServiceName) ||
                string.IsNullOrWhiteSpace(linuxSubscriptionId) || string.IsNullOrWhiteSpace(linuxResourceGroupName) || string.IsNullOrWhiteSpace(linuxAppServiceName) ||
                this.winResourceGroupComboBox.SelectedIndex == 0 || this.winAppServiceComboBox.SelectedIndex == 0 || this.linuxResourceGroupComboBox.SelectedIndex == 0 ||
                this.linuxAppServiceComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Missing information! Please select all the details.", "Warning Message", MessageBoxButtons.OKCancel);
                return;
            }

            this.mainPanelTableLayout1.Hide();
            this.migrateButton.Enabled = false;
            this.mainFlowLayoutPanel1.Controls.Add(progressViewUX);
            progressViewUX.Show();

            SiteInfo sourceSiteInfo = new SiteInfo(winSubscriptionId, winResourceGroupName, winAppServiceName);
            SiteInfo destinationSiteInfo = new SiteInfo(linuxSubscriptionId, linuxResourceGroupName, linuxAppServiceName);

            MigrationService migrationService = new MigrationService(sourceSiteInfo, destinationSiteInfo, progressViewUX.progressViewRTextBox, Array.Empty<string>());
            ThreadStart childref = new(migrationService.MigrateAsyncForWinUI);
            this._childThread = new Thread(childref);
            this._childThread.Start();
        }

        private void InitializeMigrationStatusFile()
        {
            SiteInfo sourceSiteInfo = null;
            SiteInfo destinationSiteInfo = null;
            string directoryPath = Environment.ExpandEnvironmentVariables(Constants.DATA_EXPORT_PATH);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string statusFilePath = Environment.ExpandEnvironmentVariables(Constants.MIGRATION_STATUSFILE_PATH);
            if (File.Exists(statusFilePath))
            {
                string[] statusMessages = File.ReadAllLines(statusFilePath);

                if (this.isMigrationStatusFileValid(statusMessages, out sourceSiteInfo, out destinationSiteInfo))
                {
                    this.showResumeMigrationDialogBox(statusMessages, sourceSiteInfo, destinationSiteInfo);
                    return;
                }
                else
                {
                    File.Delete(statusFilePath);
                    File.Create(statusFilePath).Dispose();
                    return;
                }
            }
            else
            {
                File.Create(statusFilePath).Dispose();
                return;
            }
        }

        private void showResumeMigrationDialogBox(string[] statusMessages, SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo)
        {
            const string message =
            "Detected a previous unfinished migration.. Do you want to resume?";
            const string caption = "Resume Previous Migration";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            // If the yes button was pressed ...
            if (result == DialogResult.Yes)
            {
                this.mainPanelTableLayout1.Hide();
                this.migrateButton.Enabled = false;
                this.mainFlowLayoutPanel1.Controls.Add(progressViewUX);
                progressViewUX.Show();

                System.Diagnostics.Debug.WriteLine("sourcesite name is |" + sourceSiteInfo.webAppName + "|");
                MigrationService migrationService = new MigrationService(sourceSiteInfo, destinationSiteInfo, progressViewUX.progressViewRTextBox, statusMessages);
                ThreadStart childref = new(migrationService.MigrateAsyncForWinUI);
                this._childThread = new Thread(childref);
                this._childThread.Start();
            }
        }

        private bool isMigrationStatusFileValid(string[] statusMessages, out SiteInfo sourceSiteInfo, out SiteInfo destinationSiteInfo)
        {
            string sourceSite = null;
            string destinationSite = null;
            string sourceResourceGroup = null;
            string destinationResourceGroup = null;
            string sourceSubscription = null;
            string destinationSubscription = null;

            foreach (string statusMsg in statusMessages)
            {
                if (statusMsg == Constants.StatusMessages.migrationFailed || statusMsg == Constants.StatusMessages.migrationCompleted)
                {
                    sourceSiteInfo = null;
                    destinationSiteInfo = null;
                    return false;
                }
                if (statusMsg.StartsWith(Constants.StatusMessages.sourceSiteName))
                {
                    sourceSite = statusMsg.Split()[4];
                    continue;
                }

                if (statusMsg.StartsWith(Constants.StatusMessages.destinationSiteName))
                {
                    destinationSite = statusMsg.Split()[4];
                    continue;
                }

                if (statusMsg.StartsWith(Constants.StatusMessages.sourceSiteResourceGroup))
                {
                    sourceResourceGroup = statusMsg.Split()[4];
                    continue;
                }

                if (statusMsg.StartsWith(Constants.StatusMessages.destinationSiteResourceGroup))
                {
                    destinationResourceGroup = statusMsg.Split()[4];
                    continue;
                }

                if (statusMsg.StartsWith(Constants.StatusMessages.sourceSiteSubscription))
                {
                    sourceSubscription = statusMsg.Split()[4];
                    continue;
                }

                if (statusMsg.StartsWith(Constants.StatusMessages.destinationSiteSubscription))
                {
                    destinationSubscription = statusMsg.Split()[4];
                    continue;
                }
            }

            if (String.IsNullOrEmpty(sourceSite) || String.IsNullOrEmpty(sourceResourceGroup) || String.IsNullOrEmpty(sourceSubscription) ||
                String.IsNullOrEmpty(destinationSite) || String.IsNullOrEmpty(destinationResourceGroup) || String.IsNullOrEmpty(destinationSubscription))
            {
                sourceSiteInfo = null;
                destinationSiteInfo = null;
                return false;
            }

            sourceSiteInfo = new SiteInfo(sourceSubscription, sourceResourceGroup, sourceSite);
            destinationSiteInfo = new SiteInfo(destinationSubscription, destinationResourceGroup, destinationSite);
            return true;
        }
    }
}