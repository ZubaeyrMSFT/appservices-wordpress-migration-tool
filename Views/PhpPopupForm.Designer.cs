using WordPressMigrationTool.Utilities;
using System.Diagnostics;

namespace WordPressMigrationTool.Views
{
    partial class ValidationPopupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent(List<string> validationMessageTypes, SiteInfo sourceSiteInfo, SiteInfo destinationSiteInfo)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValidationPopupForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(60, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 130);
            this.label1.TabIndex = 0;
            this.label1.Text = String.Format("The destination site ({0}) doesn't use an official WordPress on Linux image. This may cause the migration to fail.", destinationSiteInfo.webAppName);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(60, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(391, 150);
            this.label2.TabIndex = 1;
            this.label2.Text = "The WordPress version of source site is different from that of desitnation site. Your plugins/themes maybe incompatible with the new site.";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(60, 400);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(391, 110);
            this.linkLabel1.Text = "The PHP versions on source site and destination site are different. This may lead to incompatibilities. Refer to PHP version compatibility chart.";
            this.linkLabel1.Links.Add(113, 31, Constants.PHP_VERSION_COMPATIBILITY_CHART_URL);
            this.linkLabel1.LinkClicked += (s, e) => System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {Constants.PHP_VERSION_COMPATIBILITY_CHART_URL}") { CreateNoWindow = true });
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(98, 20 + 160 * validationMessageTypes.Count());
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 34);
            this.button1.TabIndex = 3;
            this.button1.Text = "Continue";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ContinueButton_Clicked);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(350, 20 + 160 * validationMessageTypes.Count());
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 34);
            this.button2.TabIndex = 4;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CancelButton_Clicked);
            // 
            // PopupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 100 + 160 * validationMessageTypes.Count());
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.InitializeLabels(validationMessageTypes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ControlBox = false;
            this.Name = "PopupForm";
            this.Text = "Warning!";
            this.Load += new System.EventHandler(this.PopupForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private LinkLabel linkLabel1;
        private Button button1;
        private Button button2;
    }
}