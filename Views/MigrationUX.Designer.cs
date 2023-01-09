using System.Diagnostics;
using System.Security.Policy;
using WordPressMigrationTool.Utilities;

namespace WordPressMigrationTool
{
    partial class MigrationUX
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MigrationUX));
            this.mainFlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.mainPanelTableLayout1 = new System.Windows.Forms.TableLayoutPanel();
            this.windowsDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this.windowsDetailsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.winSubscriptionIdLabel = new System.Windows.Forms.Label();
            this.winResourceGroupNameLabel = new System.Windows.Forms.Label();
            this.winAppServiceNameLabel = new System.Windows.Forms.Label();
            this.winSubscriptionComboBox = new System.Windows.Forms.ComboBox();
            this.winResourceGroupComboBox = new System.Windows.Forms.ComboBox();
            this.winAppServiceComboBox = new System.Windows.Forms.ComboBox();
            this.linuxDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this.createNewLabel = new System.Windows.Forms.Label();
            this.createNewLinkLabel = new System.Windows.Forms.LinkLabel();
            this.linuxDetailsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.linuxSubscriptionIdLabel = new System.Windows.Forms.Label();
            this.linuxResourceGroupLabel = new System.Windows.Forms.Label();
            this.linuxAppServiceNameLabel = new System.Windows.Forms.Label();
            this.linuxSubscriptionComboBox = new System.Windows.Forms.ComboBox();
            this.linuxResourceGroupComboBox = new System.Windows.Forms.ComboBox();
            this.linuxAppServiceComboBox = new System.Windows.Forms.ComboBox();
            this.featureCheckBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.bottomTableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.bottomTableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.migrateButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainFlowLayoutPanel1.SuspendLayout();
            this.mainPanelTableLayout1.SuspendLayout();
            this.windowsDetailsGroupBox.SuspendLayout();
            this.windowsDetailsTableLayout.SuspendLayout();
            this.linuxDetailsGroupBox.SuspendLayout();
            this.linuxDetailsTableLayout.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.bottomTableLayoutPanel1.SuspendLayout();
            this.bottomTableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainFlowLayoutPanel1
            // 
            this.mainFlowLayoutPanel1.Controls.Add(this.mainPanelTableLayout1);
            this.mainFlowLayoutPanel1.Location = new System.Drawing.Point(1, 3);
            this.mainFlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainFlowLayoutPanel1.Name = "mainFlowLayoutPanel1";
            this.mainFlowLayoutPanel1.Size = new System.Drawing.Size(649, 637);
            this.mainFlowLayoutPanel1.TabIndex = 0;
            // 
            // mainPanelTableLayout1
            // 
            this.mainPanelTableLayout1.ColumnCount = 1;
            this.mainPanelTableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.Controls.Add(this.windowsDetailsGroupBox, 0, 0);
            this.mainPanelTableLayout1.Controls.Add(this.linuxDetailsGroupBox, 0, 1);
            this.mainPanelTableLayout1.Location = new System.Drawing.Point(4, 5);
            this.mainPanelTableLayout1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainPanelTableLayout1.Name = "mainPanelTableLayout1";
            this.mainPanelTableLayout1.RowCount = 2;
            this.mainPanelTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.Size = new System.Drawing.Size(644, 627);
            this.mainPanelTableLayout1.TabIndex = 0;
            // 
            // windowsDetailsGroupBox
            // 
            this.windowsDetailsGroupBox.Controls.Add(this.windowsDetailsTableLayout);
            this.windowsDetailsGroupBox.Location = new System.Drawing.Point(4, 5);
            this.windowsDetailsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windowsDetailsGroupBox.Name = "windowsDetailsGroupBox";
            this.windowsDetailsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windowsDetailsGroupBox.Size = new System.Drawing.Size(630, 265);
            this.windowsDetailsGroupBox.TabIndex = 0;
            this.windowsDetailsGroupBox.TabStop = false;
            this.windowsDetailsGroupBox.Text = "Source Site (Windows App Service)";
            // 
            // windowsDetailsTableLayout
            // 
            this.windowsDetailsTableLayout.ColumnCount = 2;
            this.windowsDetailsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.windowsDetailsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.windowsDetailsTableLayout.Controls.Add(this.winSubscriptionIdLabel, 0, 0);
            this.windowsDetailsTableLayout.Controls.Add(this.winResourceGroupNameLabel, 0, 1);
            this.windowsDetailsTableLayout.Controls.Add(this.winAppServiceNameLabel, 0, 2);
            this.windowsDetailsTableLayout.Controls.Add(this.winSubscriptionComboBox, 1, 0);
            this.windowsDetailsTableLayout.Controls.Add(this.winResourceGroupComboBox, 1, 1);
            this.windowsDetailsTableLayout.Controls.Add(this.winAppServiceComboBox, 1, 2);
            this.windowsDetailsTableLayout.Location = new System.Drawing.Point(9, 60);
            this.windowsDetailsTableLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windowsDetailsTableLayout.Name = "windowsDetailsTableLayout";
            this.windowsDetailsTableLayout.RowCount = 3;
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.Size = new System.Drawing.Size(609, 188);
            this.windowsDetailsTableLayout.TabIndex = 0;
            // 
            // winSubscriptionIdLabel
            // 
            this.winSubscriptionIdLabel.AutoSize = true;
            this.winSubscriptionIdLabel.Location = new System.Drawing.Point(4, 0);
            this.winSubscriptionIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.winSubscriptionIdLabel.Name = "winSubscriptionIdLabel";
            this.winSubscriptionIdLabel.Size = new System.Drawing.Size(132, 25);
            this.winSubscriptionIdLabel.TabIndex = 0;
            this.winSubscriptionIdLabel.Text = "Subscription";
            // 
            // winResourceGroupNameLabel
            // 
            this.winResourceGroupNameLabel.AutoSize = true;
            this.winResourceGroupNameLabel.Location = new System.Drawing.Point(4, 62);
            this.winResourceGroupNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.winResourceGroupNameLabel.Name = "winResourceGroupNameLabel";
            this.winResourceGroupNameLabel.Size = new System.Drawing.Size(138, 25);
            this.winResourceGroupNameLabel.TabIndex = 1;
            this.winResourceGroupNameLabel.Text = "Resource Group";
            // 
            // winAppServiceNameLabel
            // 
            this.winAppServiceNameLabel.AutoSize = true;
            this.winAppServiceNameLabel.Location = new System.Drawing.Point(4, 124);
            this.winAppServiceNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.winAppServiceNameLabel.Name = "winAppServiceNameLabel";
            this.winAppServiceNameLabel.Size = new System.Drawing.Size(158, 25);
            this.winAppServiceNameLabel.TabIndex = 2;
            this.winAppServiceNameLabel.Text = "App Service Name";
            // 
            // winSubscriptionComboBox
            // 
            this.winSubscriptionComboBox.DisplayMember = "Name";
            this.winSubscriptionComboBox.Location = new System.Drawing.Point(186, 5);
            this.winSubscriptionComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.winSubscriptionComboBox.Name = "winSubscriptionComboBox";
            this.winSubscriptionComboBox.Size = new System.Drawing.Size(400, 33);
            this.winSubscriptionComboBox.TabIndex = 3;
            this.winSubscriptionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.winSubscriptionComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Subscription");
            this.winSubscriptionComboBox.SelectionChangeCommitted += new System.EventHandler(this.winSubscriptionComboBox_SelectedIndexChanged);
            // 
            // winResourceGroupComboBox
            // 
            this.winResourceGroupComboBox.Location = new System.Drawing.Point(186, 67);
            this.winResourceGroupComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.winResourceGroupComboBox.Name = "winResourceGroupComboBox";
            this.winResourceGroupComboBox.Size = new System.Drawing.Size(400, 33);
            this.winResourceGroupComboBox.TabIndex = 4;
            this.winResourceGroupComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.winResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            this.winResourceGroupComboBox.SelectionChangeCommitted += new System.EventHandler(this.winResourceGroupComboBox_SelectedIndexChanged);
            // 
            // winAppServiceComboBox
            // 
            this.winAppServiceComboBox.Location = new System.Drawing.Point(186, 129);
            this.winAppServiceComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.winAppServiceComboBox.Name = "winAppServiceComboBox";
            this.winAppServiceComboBox.Size = new System.Drawing.Size(400, 33);
            this.winAppServiceComboBox.TabIndex = 5;
            this.winAppServiceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.winAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Windows app");
            // 
            // linuxDetailsGroupBox
            // 
            this.linuxDetailsGroupBox.Controls.Add(this.createNewLabel);
            this.linuxDetailsGroupBox.Controls.Add(this.createNewLinkLabel);
            this.linuxDetailsGroupBox.Controls.Add(this.linuxDetailsTableLayout);
            this.linuxDetailsGroupBox.Controls.Add(this.featureCheckBox);
            this.linuxDetailsGroupBox.Location = new System.Drawing.Point(4, 300);
            this.linuxDetailsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxDetailsGroupBox.Name = "linuxDetailsGroupBox";
            this.linuxDetailsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxDetailsGroupBox.Size = new System.Drawing.Size(630, 315);
            this.linuxDetailsGroupBox.TabIndex = 1;
            this.linuxDetailsGroupBox.TabStop = false;
            this.linuxDetailsGroupBox.Text = "Destination Site (Linux App Service)";
            // 
            // createNewLabel
            // 
            this.createNewLabel.AutoSize = true;
            this.createNewLabel.Location = new System.Drawing.Point(16, 36);
            this.createNewLabel.Name = "createNewLabel";
            this.createNewLabel.Size = new System.Drawing.Size(472, 25);
            this.createNewLabel.TabIndex = 2;
            this.createNewLabel.Text = "Do not have an existing WordPress site? Create a new site ";
            // 
            // createNewLinkLabel
            // 
            this.createNewLinkLabel.AutoSize = true;
            this.createNewLinkLabel.Location = new System.Drawing.Point(484, 36);
            this.createNewLinkLabel.Name = "createNewLinkLabel";
            this.createNewLinkLabel.Size = new System.Drawing.Size(46, 25);
            this.createNewLinkLabel.TabIndex = 1;
            this.createNewLinkLabel.TabStop = true;
            this.createNewLinkLabel.Text = "here";
            this.createNewLinkLabel.Links.Add(0, 4, Constants.AZURE_PORTAL_URL);
            this.createNewLinkLabel.LinkClicked += (s,e) => System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {Constants.AZURE_PORTAL_URL}"){ CreateNoWindow = true }); 
            // 
            // linuxDetailsTableLayout
            // 
            this.linuxDetailsTableLayout.ColumnCount = 2;
            this.linuxDetailsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.linuxDetailsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.linuxDetailsTableLayout.Controls.Add(this.linuxSubscriptionIdLabel, 0, 0);
            this.linuxDetailsTableLayout.Controls.Add(this.linuxResourceGroupLabel, 0, 1);
            this.linuxDetailsTableLayout.Controls.Add(this.linuxAppServiceNameLabel, 0, 2);
            this.linuxDetailsTableLayout.Controls.Add(this.linuxSubscriptionComboBox, 1, 0);
            this.linuxDetailsTableLayout.Controls.Add(this.linuxResourceGroupComboBox, 1, 1);
            this.linuxDetailsTableLayout.Controls.Add(this.linuxAppServiceComboBox, 1, 2);
            this.linuxDetailsTableLayout.Location = new System.Drawing.Point(13, 76);
            this.linuxDetailsTableLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxDetailsTableLayout.Name = "linuxDetailsTableLayout";
            this.linuxDetailsTableLayout.RowCount = 3;
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.linuxDetailsTableLayout.Size = new System.Drawing.Size(609, 187);
            this.linuxDetailsTableLayout.TabIndex = 0;
            // 
            // linuxSubscriptionIdLabel
            // 
            this.linuxSubscriptionIdLabel.AutoSize = true;
            this.linuxSubscriptionIdLabel.Location = new System.Drawing.Point(4, 0);
            this.linuxSubscriptionIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linuxSubscriptionIdLabel.Name = "linuxSubscriptionIdLabel";
            this.linuxSubscriptionIdLabel.Size = new System.Drawing.Size(132, 25);
            this.linuxSubscriptionIdLabel.TabIndex = 0;
            this.linuxSubscriptionIdLabel.Text = "Subscription";
            // 
            // linuxResourceGroupLabel
            // 
            this.linuxResourceGroupLabel.AutoSize = true;
            this.linuxResourceGroupLabel.Location = new System.Drawing.Point(4, 62);
            this.linuxResourceGroupLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linuxResourceGroupLabel.Name = "linuxResourceGroupLabel";
            this.linuxResourceGroupLabel.Size = new System.Drawing.Size(138, 25);
            this.linuxResourceGroupLabel.TabIndex = 1;
            this.linuxResourceGroupLabel.Text = "Resource Group";
            // 
            // linuxAppServiceNameLabel
            // 
            this.linuxAppServiceNameLabel.AutoSize = true;
            this.linuxAppServiceNameLabel.Location = new System.Drawing.Point(4, 124);
            this.linuxAppServiceNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linuxAppServiceNameLabel.Name = "linuxAppServiceNameLabel";
            this.linuxAppServiceNameLabel.Size = new System.Drawing.Size(158, 25);
            this.linuxAppServiceNameLabel.TabIndex = 2;
            this.linuxAppServiceNameLabel.Text = "App Service Name";
            // 
            // linuxSubscriptionComboBox
            // 
            this.linuxSubscriptionComboBox.DisplayMember = "Name";
            this.linuxSubscriptionComboBox.Location = new System.Drawing.Point(186, 5);
            this.linuxSubscriptionComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxSubscriptionComboBox.Name = "linuxSubscriptionComboBox";
            this.linuxSubscriptionComboBox.Size = new System.Drawing.Size(400, 33);
            this.linuxSubscriptionComboBox.TabIndex = 11;
            this.linuxSubscriptionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.linuxSubscriptionComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Subscription");
            this.linuxSubscriptionComboBox.SelectionChangeCommitted += new System.EventHandler(this.linuxSubscriptionComboBox_SelectedIndexChanged);
            // 
            // linuxResourceGroupComboBox
            // 
            this.linuxResourceGroupComboBox.Location = new System.Drawing.Point(186, 67);
            this.linuxResourceGroupComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxResourceGroupComboBox.Name = "linuxResourceGroupComboBox";
            this.linuxResourceGroupComboBox.Size = new System.Drawing.Size(400, 33);
            this.linuxResourceGroupComboBox.TabIndex = 12;
            this.linuxResourceGroupComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.linuxResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            this.linuxResourceGroupComboBox.SelectionChangeCommitted += new System.EventHandler(this.linuxResourceGroupComboBox_SelectedIndexChanged);
            // 
            // linuxAppServiceComboBox
            // 
            this.linuxAppServiceComboBox.Location = new System.Drawing.Point(186, 129);
            this.linuxAppServiceComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.linuxAppServiceComboBox.Name = "linuxAppServiceComboBox";
            this.linuxAppServiceComboBox.Size = new System.Drawing.Size(400, 33);
            this.linuxAppServiceComboBox.TabIndex = 14;
            this.linuxAppServiceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.linuxAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress on Linux app");
            // 
            // featureCheckBox
            //
            this.featureCheckBox.Location = new System.Drawing.Point(19, 262);
            this.featureCheckBox.Size = new System.Drawing.Size(600, 33);
            this.featureCheckBox.Appearance = Appearance.Normal;
            this.featureCheckBox.Checked = true;
            this.featureCheckBox.Font = new System.Drawing.Font("Segoe UI", 5.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.featureCheckBox.Text = "Retain AFD, CDN or Blob Storage features after migration.\r\n(Enabling this will install W3 Total Cache plugin and override its settings)";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.bottomTableLayoutPanel1);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(5, 645);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(649, 83);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // bottomTableLayoutPanel1
            // 
            this.bottomTableLayoutPanel1.ColumnCount = 2;
            this.bottomTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomTableLayoutPanel1.Controls.Add(this.bottomTableLayoutPanel2, 1, 0);
            this.bottomTableLayoutPanel1.Location = new System.Drawing.Point(4, 5);
            this.bottomTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bottomTableLayoutPanel1.Name = "bottomTableLayoutPanel1";
            this.bottomTableLayoutPanel1.RowCount = 1;
            this.bottomTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomTableLayoutPanel1.Size = new System.Drawing.Size(644, 77);
            this.bottomTableLayoutPanel1.TabIndex = 0;
            // 
            // bottomTableLayoutPanel2
            // 
            this.bottomTableLayoutPanel2.ColumnCount = 3;
            this.bottomTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.bottomTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.bottomTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.bottomTableLayoutPanel2.Controls.Add(this.migrateButton, 1, 0);
            this.bottomTableLayoutPanel2.Controls.Add(this.cancelButton, 2, 0);
            this.bottomTableLayoutPanel2.Location = new System.Drawing.Point(326, 5);
            this.bottomTableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bottomTableLayoutPanel2.Name = "bottomTableLayoutPanel2";
            this.bottomTableLayoutPanel2.RowCount = 1;
            this.bottomTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomTableLayoutPanel2.Size = new System.Drawing.Size(314, 67);
            this.bottomTableLayoutPanel2.TabIndex = 0;
            // 
            // migrateButton
            // 
            this.migrateButton.Location = new System.Drawing.Point(108, 5);
            this.migrateButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.migrateButton.Name = "migrateButton";
            this.migrateButton.Size = new System.Drawing.Size(96, 57);
            this.migrateButton.TabIndex = 0;
            this.migrateButton.Text = "Migrate";
            this.migrateButton.UseVisualStyleBackColor = true;
            this.migrateButton.Click += new System.EventHandler(this.migrateButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(212, 5);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(97, 57);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // MigrationUX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 733);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.mainFlowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "MigrationUX";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WordPress Migration Tool (Azure App Services)";
            this.mainFlowLayoutPanel1.ResumeLayout(false);
            this.mainPanelTableLayout1.ResumeLayout(false);
            this.windowsDetailsGroupBox.ResumeLayout(false);
            this.windowsDetailsTableLayout.ResumeLayout(false);
            this.windowsDetailsTableLayout.PerformLayout();
            this.linuxDetailsGroupBox.ResumeLayout(false);
            this.linuxDetailsGroupBox.PerformLayout();
            this.linuxDetailsTableLayout.ResumeLayout(false);
            this.linuxDetailsTableLayout.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.bottomTableLayoutPanel1.ResumeLayout(false);
            this.bottomTableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FlowLayoutPanel mainFlowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private TableLayoutPanel bottomTableLayoutPanel1;
        private TableLayoutPanel bottomTableLayoutPanel2;
        private Button migrateButton;
        private Button cancelButton;
        private TableLayoutPanel mainPanelTableLayout1;
        private GroupBox windowsDetailsGroupBox;
        private GroupBox linuxDetailsGroupBox;
        private TableLayoutPanel windowsDetailsTableLayout;
        private Label winSubscriptionIdLabel;
        private Label winResourceGroupNameLabel;
        private Label winAppServiceNameLabel;
        private ComboBox winSubscriptionComboBox;
        private ComboBox winResourceGroupComboBox;
        private ComboBox winAppServiceComboBox;
        private TableLayoutPanel linuxDetailsTableLayout;
        private Label linuxSubscriptionIdLabel;
        private Label linuxResourceGroupLabel;
        private Label linuxAppServiceNameLabel;
        private ComboBox linuxSubscriptionComboBox;
        private ComboBox linuxResourceGroupComboBox;
        private ComboBox linuxAppServiceComboBox;
        private Label createNewLabel;
        private LinkLabel createNewLinkLabel;
        private CheckBox featureCheckBox;
    }
}