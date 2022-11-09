using MySqlX.XDevAPI.Common;

namespace WordPressMigrationTool
{
    public partial class MigrationUX : Form
    {
        public MigrationUX()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
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
                DialogResult warningDialogRes = MessageBox.Show("Missing information! Please enter all the details.", "Warning Message", MessageBoxButtons.OKCancel);                
                if (warningDialogRes == DialogResult.OK)
                {
                    return;
                }
            }

            SiteInfo sourceSiteInfo = new SiteInfo(winSubscriptionId, winResourceGroupName, winAppServiceName);
            SiteInfo destinationSiteInfo = new SiteInfo(linuxSubscriptionId, linuxResourceGroupName, linuxAppServiceName);
            
            try
            {
                Result result =  new MigrationService().migrate(sourceSiteInfo, destinationSiteInfo);
                DialogResult errorDialogRes = MessageBox.Show(result.message, "Error Message", MessageBoxButtons.OKCancel);
                if (errorDialogRes == DialogResult.OK)
                {
                    return;
                }

            } catch (Exception ex)
            {
                DialogResult errorDialogRes = MessageBox.Show(ex.Message, "Error Message", MessageBoxButtons.OKCancel);
                if (errorDialogRes == DialogResult.OK)
                {
                    return;
                }
            }
        }
    }
}