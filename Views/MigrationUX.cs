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
        private List<string> LinResourceGroups;
        private List<string> LinAppServices;
        private List<Subscription> WinSubscriptions;
        private List<string> WinResourceGroups;
        private List<string> WinAppServices;

        public MigrationUX()
        {
            GetSubscriptions();
            InitializeComponent();
            this.progressViewUX = new ProgressUX();
            progressViewUX.Hide();
        }

        private void GetSubscriptions()
        {
            List<Subscription> subscriptions = AzureManagementUtils.GetSubscriptions();
            subscriptions.Sort((x, y) => x.Name.CompareTo(y.Name));
            subscriptions.Insert(0, new Subscription("Select a Subscription", ""));
            this.LinSubscriptions = subscriptions;
            this.WinSubscriptions = subscriptions;
        }

        private string getSubscriptionResourceId (string subscriptionId)
        {
            return "/subscriptions/" + subscriptionId;
        }

        private async Task winSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winSubscriptionComboBox_SelectedIndexChanged");
            if (this.winSubscriptionComboBox.SelectedValue == null)
                return;

            string subscriptionId = this.winSubscriptionComboBox.SelectedValue.ToString();

            var resourceGroups = await AzureManagementUtils.GetResourceGroupsInSubscription(getSubscriptionResourceId(subscriptionId));
            resourceGroups.Sort();
            resourceGroups.Insert(0, "Select a Resource Group");

            this.winResourceGroupComboBox.DataSource = resourceGroups;
        }

        private async Task linuxSubscriptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("linuxSubscriptionComboBox_SelectedIndexChanged");
            if (this.linuxSubscriptionComboBox.SelectedValue == null)
                return;

            string subscriptionId = this.linuxSubscriptionComboBox.SelectedValue.ToString();

            var resourceGroups = await AzureManagementUtils.GetResourceGroupsInSubscription(getSubscriptionResourceId(subscriptionId));
            resourceGroups.Sort();
            resourceGroups.Insert(0, "Select a Resource Group");

            this.linuxResourceGroupComboBox.DataSource = resourceGroups;
        }

        private async Task winResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("winResourceGroupComboBox_SelectedIndexChanged");

            if (this.winSubscriptionComboBox.SelectedValue == null || this.winResourceGroupComboBox.SelectedValue == null || this.winResourceGroupComboBox.SelectedIndex == 0)
                return;
            
            string subscriptionId = this.winSubscriptionComboBox.SelectedValue.ToString();
            string resourceGroupName = this.winResourceGroupComboBox.SelectedValue.ToString();

            List<string> appServices = await AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName);
            appServices.Sort();
            appServices.Insert(0, "Select a WordPress app");

            this.winAppServiceComboBox.DataSource = appServices;
        }

        private async Task linuxResourceGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("linuxResourceGroupComboBox_SelectedIndexChanged");
           
            if(this.linuxSubscriptionComboBox.SelectedValue == null || this.linuxResourceGroupComboBox.SelectedValue == null || this.linuxResourceGroupComboBox.SelectedIndex == 0)
                return;

            string subscriptionId = this.linuxSubscriptionComboBox.SelectedValue.ToString();
            string resourceGroupName = this.linuxResourceGroupComboBox.SelectedValue.ToString();

            List<string> appServices = await AzureManagementUtils.GetAppServicesInResourceGroup(subscriptionId, resourceGroupName);
            appServices.Sort();
            appServices.Insert(0, "Select a WordPress app");

            this.linuxAppServiceComboBox.DataSource = appServices;
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
                string.IsNullOrWhiteSpace(linuxSubscriptionId) || string.IsNullOrWhiteSpace(linuxResourceGroupName) || string.IsNullOrWhiteSpace(linuxAppServiceName))
            {
                MessageBox.Show("Missing information! Please enter all the details.", "Warning Message", MessageBoxButtons.OKCancel);
                return;
            }

            this.mainPanelTableLayout1.Hide();
            this.migrateButton.Enabled = false;
            this.mainFlowLayoutPanel1.Controls.Add(progressViewUX);
            progressViewUX.Show();

            SiteInfo sourceSiteInfo = new SiteInfo(winSubscriptionId, winResourceGroupName, winAppServiceName);
            SiteInfo destinationSiteInfo = new SiteInfo(linuxSubscriptionId, linuxResourceGroupName, linuxAppServiceName);

            MigrationService migrationService = new MigrationService(sourceSiteInfo, destinationSiteInfo, progressViewUX.progressViewRTextBox);
            ThreadStart childref = new(migrationService.MigrateAsyncForWinUI);
            this._childThread = new Thread(childref);
            this._childThread.Start();
        }
    }
}