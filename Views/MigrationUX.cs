using Azure.ResourceManager.Resources;
using MySqlX.XDevAPI.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WordPressMigrationTool.Utilities;
using WordPressMigrationTool.Views;
using static System.Net.Mime.MediaTypeNames;

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
            InitializeComponent();

            // Dsiable dropdowns and migrate button while initializing window and ARM api calls
            ToggleDropdownStates(false);
            InitializeMigrationWindow();
            GetSubscriptions();

            // Initialize background workers for running async operations on dropdown value change to unblock UI
            InitializeBackgroundWorkers();
            
            this.progressViewUX = new ProgressUX();
            progressViewUX.Hide();

            //enable dropdowns and migrate button
            ToggleDropdownStates(true);
            InitializeMigrationStatusFile();
        }

        private void InitializeMigrationWindow()
        {
            // show Migration window
            this.Show();

            // display popup message box
            MessageBox.Show("Azure login is required prior to launching migration tool. Click OK to open Azure authentication page.", "", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                 MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            
            // One-time azure login
            ShellExecute.Login("az login");
        }

        private void ToggleDropdownStates(bool state)
        {
            enableLinuxDropdowns(state);
            enableWindowsDropdowns(state);
        }

        // Initialize background workers to call ARM APIs asynchronously
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

        // Retreives Subscriptions using the default Azure credentials via ARM API
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
                this.LinSubscriptions.Add(new Subscription(subscription.Data.DisplayName, subscription.Data.SubscriptionId));
                this.WinSubscriptions.Add(new Subscription(subscription.Data.DisplayName, subscription.Data.SubscriptionId));
            }

            this.LinSubscriptions.Sort((x, y) => x.Name.CompareTo(y.Name));
            this.LinSubscriptions.Insert(0, new Subscription("Select a Subscription", " "));

            this.WinSubscriptions.Sort((x, y) => x.Name.CompareTo(y.Name));
            this.WinSubscriptions.Insert(0, new Subscription("Select a Subscription", " "));

            this.winSubscriptionComboBox.DataSource = this.WinSubscriptions;
            this.winSubscriptionComboBox.DisplayMember = "Name";
            this.winSubscriptionComboBox.ValueMember = "Id";

            this.linuxSubscriptionComboBox.DataSource = this.LinSubscriptions;
            this.linuxSubscriptionComboBox.DisplayMember = "Name";
            this.linuxSubscriptionComboBox.ValueMember = "Id";
        }

        private string getSubscriptionResourceId (string subscriptionId)
        {
            return "/subscriptions/" + subscriptionId;
        }

        private void winSubscriptionChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> resourceGroups = new List<string>();
            if (e.Argument != null)
            {
                string subscriptionId = e.Argument as string;
                resourceGroups = AzureManagementUtils.GetResourceGroupsInSubscription(subscriptionId, this._subscriptions);
                resourceGroups.Sort();
            }
            resourceGroups.Insert(0, "Select a Resource Group");
            e.Result = resourceGroups;
        }

        // Updates Windows resource group dropdown with those in the selected subscription
        private void winSubscriptionChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
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

        // This function is called on windows subscription dropdown value change
        // Asynchrounously retrieves resource groups for the selected subscription (windows)
        private void winSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
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

        // Updates Windows resource group dropdown with those in the selected subscriptio
        private void linSubscriptionChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
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

        // This function is called on linux subscription dropdown value change
        // Asynchrounously retrieves resource groups for the selected subscription (linux)
        private void linuxSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.linuxSubscriptionComboBox.SelectedValue == null)
                return;

            string subscriptionId = this.linuxSubscriptionComboBox.SelectedValue.ToString();

            this.enableLinuxDropdowns(false);
            this._linSubscriptionChangeWorker.RunWorkerAsync(subscriptionId);
        }

        private void winRgChangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> appServices = new List<string>();
            if (e.Argument != null)
            {
                List<string> arguments = e.Argument as List<string>;
                string subscriptionId = arguments[0];
                string resourceGroupName = arguments[1];

                appServices = AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName, this._subscriptions);
            }
            appServices.Sort();
            appServices.Insert(0, "Select a WordPress on Windows app");

            e.Result = appServices;
        }

        // Updates Windows app service dropdown with those in the selected resource group
        private void winRgChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.winAppServiceComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.winAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Windows app");
            }

            this.enableWindowsDropdowns(true);
        }

        // This function is called on windows resource group dropdown value change
        // Asynchrounously retrieves app services for the selected resource group (windows)
        private void winResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            List<string> appServices = new List<string>();
            
            if (e.Argument != null)
            {
                List<string> arguments = e.Argument as List<string>;
                string subscriptionId = arguments[0];
                string resourceGroupName = arguments[1];

                appServices = AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName, this._subscriptions);
                appServices.Sort();
            }
            appServices.Insert(0, "Select a WordPress on Linux app");

            e.Result = appServices;
        }

        // Updates Linux app service dropdown with those in the selected resource group
        private void linRgChangeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.linuxAppServiceComboBox.DataSource = e.Result as List<string>;
            }
            else
            {
                this.linuxAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Linux app");
            }

            this.enableLinuxDropdowns(true);
        }

        // This function is called on linux resource group dropdown value change
        // Asynchrously gets app services for the selected resource groups (linux)
        private void linuxResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.linuxSubscriptionComboBox.SelectedValue == null || this.linuxResourceGroupComboBox.SelectedValue == null)
                return;

            if (this.linuxResourceGroupComboBox.SelectedIndex == 0)
            {
                this.linuxAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Linux app");
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

            MigrationService migrationService = new MigrationService(sourceSiteInfo, destinationSiteInfo, progressViewUX.progressViewRTextBox, Array.Empty<string>(), this);
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
            string message =
            String.Format("Detected a previous unfinished migration from source site {0} to destination site {1}.. Do you want to resume?", sourceSiteInfo.webAppName, destinationSiteInfo.webAppName);
            const string caption = "Resume Previous Migration";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.mainPanelTableLayout1.Hide();
                this.migrateButton.Enabled = false;
                this.mainFlowLayoutPanel1.Controls.Add(progressViewUX);
                progressViewUX.Show();

                MigrationService migrationService = new MigrationService(sourceSiteInfo, destinationSiteInfo, progressViewUX.progressViewRTextBox, statusMessages, this);
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