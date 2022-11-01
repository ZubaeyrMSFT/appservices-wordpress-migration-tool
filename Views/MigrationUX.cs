using MySqlX.XDevAPI.Common;
using System.ComponentModel;
using WordPressMigrationTool.Views;

namespace WordPressMigrationTool
{
    public partial class MigrationUX : Form
    {
        private Thread? _childThread;
        private ProgressUX progressViewUX;

        public MigrationUX()
        {
            InitializeComponent();
            this.progressViewUX = new ProgressUX();
            progressViewUX.Hide();
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
            string winSubscriptionId = this.winSubscriptionTextBox.Text;
            string winResourceGroupName = this.winResourceGroupTextBox.Text;
            string winAppServiceName = this.winAppServiceTextBox.Text;

            string linuxSubscriptionId = this.linuxSubscriptionIdTextBox.Text;
            string linuxResourceGroupName = this.linuxResourceGroupTextBox.Text;
            string linuxAppServiceName = this.linuxAppServiceTextBox.Text;

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