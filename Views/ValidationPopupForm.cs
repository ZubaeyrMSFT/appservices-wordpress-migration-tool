using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordPressMigrationTool.Views
{
    public partial class ValidationPopupForm : Form
    {
        private bool _continueMigration;

        public ValidationPopupForm(List<string> validationMessageTypes, SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo)
        {
            this._continueMigration = true;
            InitializeComponent(validationMessageTypes, sourceSiteInfo, destinationSiteInfo);
        }

        /// <summary>
        /// Initializes windows forms Label positions for each validation message
        /// </summary>
        /// <param name="numMessages"></param>
        private void InitializeLabels(List<string> validationMessageTypes)
        {
            int msgCount = 0;

            if (validationMessageTypes.Contains("IMAGE_INVALID"))
            {
                this.label1.Location = new System.Drawing.Point(60, 21+msgCount*140);
                this.Controls.Add(this.label1);
                msgCount++;
            }

            if (validationMessageTypes.Contains("WP_VERSION"))
            {
                this.label2.Location = new System.Drawing.Point(60, 21 + msgCount * 140);
                this.Controls.Add(this.label2);
                msgCount++;
            }

            if (validationMessageTypes.Contains("PHP_VERSION"))
            {
                this.linkLabel1.Location = new System.Drawing.Point(60, 21 + msgCount * 140);
                this.Controls.Add(this.linkLabel1);
            }
        }

        private void ContinueButton_Clicked(object sender, EventArgs e)
        {
            this._continueMigration = true;
            this.Close();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            this._continueMigration = false;
            this.Close();
        }

        private void PopupForm_Load(object sender, EventArgs e)
        {

        }

        public bool GetStatusOnClose()
        {
            return this._continueMigration;
        }
    }
}
