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
            this.linuxDetailsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.linuxSubscriptionIdLabel = new System.Windows.Forms.Label();
            this.linuxResourceGroupLabel = new System.Windows.Forms.Label();
            this.linuxAppServiceNameLabel = new System.Windows.Forms.Label();
            this.linuxSubscriptionComboBox = new System.Windows.Forms.ComboBox();
            this.linuxResourceGroupComboBox = new System.Windows.Forms.ComboBox();
            this.linuxAppServiceComboBox = new System.Windows.Forms.ComboBox();
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
            this.mainFlowLayoutPanel1.Location = new System.Drawing.Point(1, 2);
            this.mainFlowLayoutPanel1.Name = "mainFlowLayoutPanel1";
            this.mainFlowLayoutPanel1.Size = new System.Drawing.Size(454, 345);
            this.mainFlowLayoutPanel1.TabIndex = 0;
            // 
            // mainPanelTableLayout1
            // 
            this.mainPanelTableLayout1.ColumnCount = 1;
            this.mainPanelTableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.Controls.Add(this.windowsDetailsGroupBox, 0, 0);
            this.mainPanelTableLayout1.Controls.Add(this.linuxDetailsGroupBox, 0, 1);
            this.mainPanelTableLayout1.Location = new System.Drawing.Point(3, 3);
            this.mainPanelTableLayout1.Name = "mainPanelTableLayout1";
            this.mainPanelTableLayout1.RowCount = 2;
            this.mainPanelTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanelTableLayout1.Size = new System.Drawing.Size(451, 342);
            this.mainPanelTableLayout1.TabIndex = 0;
            // 
            // windowsDetailsGroupBox
            // 
            this.windowsDetailsGroupBox.Controls.Add(this.windowsDetailsTableLayout);
            this.windowsDetailsGroupBox.Location = new System.Drawing.Point(3, 3);
            this.windowsDetailsGroupBox.Name = "windowsDetailsGroupBox";
            this.windowsDetailsGroupBox.Size = new System.Drawing.Size(441, 165);
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
            this.windowsDetailsTableLayout.Location = new System.Drawing.Point(6, 36);
            this.windowsDetailsTableLayout.Name = "windowsDetailsTableLayout";
            this.windowsDetailsTableLayout.RowCount = 3;
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.windowsDetailsTableLayout.Size = new System.Drawing.Size(426, 113);
            this.windowsDetailsTableLayout.TabIndex = 0;
            // 
            // winSubscriptionIdLabel
            // 
            this.winSubscriptionIdLabel.AutoSize = true;
            this.winSubscriptionIdLabel.Location = new System.Drawing.Point(3, 0);
            this.winSubscriptionIdLabel.Name = "winSubscriptionIdLabel";
            this.winSubscriptionIdLabel.Size = new System.Drawing.Size(86, 15);
            this.winSubscriptionIdLabel.TabIndex = 0;
            this.winSubscriptionIdLabel.Text = "Subscription Id";
            // 
            // winResourceGroupNameLabel
            // 
            this.winResourceGroupNameLabel.AutoSize = true;
            this.winResourceGroupNameLabel.Location = new System.Drawing.Point(3, 37);
            this.winResourceGroupNameLabel.Name = "winResourceGroupNameLabel";
            this.winResourceGroupNameLabel.Size = new System.Drawing.Size(91, 15);
            this.winResourceGroupNameLabel.TabIndex = 1;
            this.winResourceGroupNameLabel.Text = "Resource Group";
            // 
            // winAppServiceNameLabel
            // 
            this.winAppServiceNameLabel.AutoSize = true;
            this.winAppServiceNameLabel.Location = new System.Drawing.Point(3, 74);
            this.winAppServiceNameLabel.Name = "winAppServiceNameLabel";
            this.winAppServiceNameLabel.Size = new System.Drawing.Size(104, 15);
            this.winAppServiceNameLabel.TabIndex = 2;
            this.winAppServiceNameLabel.Text = "App Service Name";
            // 
            // winSubscriptionComboBox
            //
            this.winSubscriptionComboBox.Location = new System.Drawing.Point(130, 3);
            this.winSubscriptionComboBox.Name = "winSubscriptionComboBox";
            this.winSubscriptionComboBox.Size = new System.Drawing.Size(281, 23);
            this.winSubscriptionComboBox.TabIndex = 3;
            this.winSubscriptionComboBox.DataSource = this.WinSubscriptions;
            this.winSubscriptionComboBox.DisplayMember = "Name";
            this.winSubscriptionComboBox.ValueMember = "Id";
            this.winSubscriptionComboBox.SelectionChangeCommitted += new System.EventHandler(async (s, e) => await this.winSubscriptionComboBox_SelectedIndexChanged(s, e));
            // 
            // winResourceGroupComboBox
            // 
            this.winResourceGroupComboBox.Location = new System.Drawing.Point(130, 40);
            this.winResourceGroupComboBox.Name = "winResourceGroupComboBox";
            this.winResourceGroupComboBox.Size = new System.Drawing.Size(281, 23);
            this.winResourceGroupComboBox.TabIndex = 4;
            this.winResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            this.winResourceGroupComboBox.SelectionChangeCommitted += new System.EventHandler(async (s, e) => await this.winResourceGroupComboBox_SelectedIndexChanged(s, e));
            // 
            // winAppServiceComboBox
            // 
            this.winAppServiceComboBox.Location = new System.Drawing.Point(130, 77);
            this.winAppServiceComboBox.Name = "winAppServiceComboBox";
            this.winAppServiceComboBox.Size = new System.Drawing.Size(281, 23);
            this.winAppServiceComboBox.TabIndex = 5;
            this.winAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress app");
            // 
            // linuxDetailsGroupBox
            // 
            this.linuxDetailsGroupBox.Controls.Add(this.linuxDetailsTableLayout);
            this.linuxDetailsGroupBox.Location = new System.Drawing.Point(3, 174);
            this.linuxDetailsGroupBox.Name = "linuxDetailsGroupBox";
            this.linuxDetailsGroupBox.Size = new System.Drawing.Size(441, 165);
            this.linuxDetailsGroupBox.TabIndex = 1;
            this.linuxDetailsGroupBox.TabStop = false;
            this.linuxDetailsGroupBox.Text = "Destination Site (Linux App Service)";
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
            this.linuxDetailsTableLayout.Location = new System.Drawing.Point(6, 36);
            this.linuxDetailsTableLayout.Name = "linuxDetailsTableLayout";
            this.linuxDetailsTableLayout.RowCount = 3;
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.linuxDetailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.linuxDetailsTableLayout.Size = new System.Drawing.Size(426, 125);
            this.linuxDetailsTableLayout.TabIndex = 0;
            // 
            // linuxSubscriptionIdLabel
            // 
            this.linuxSubscriptionIdLabel.AutoSize = true;
            this.linuxSubscriptionIdLabel.Location = new System.Drawing.Point(3, 0);
            this.linuxSubscriptionIdLabel.Name = "linuxSubscriptionIdLabel";
            this.linuxSubscriptionIdLabel.Size = new System.Drawing.Size(86, 15);
            this.linuxSubscriptionIdLabel.TabIndex = 0;
            this.linuxSubscriptionIdLabel.Text = "Subscription Id";
            // 
            // linuxResourceGroupLabel
            // 
            this.linuxResourceGroupLabel.AutoSize = true;
            this.linuxResourceGroupLabel.Location = new System.Drawing.Point(3, 41);
            this.linuxResourceGroupLabel.Name = "linuxResourceGroupLabel";
            this.linuxResourceGroupLabel.Size = new System.Drawing.Size(91, 15);
            this.linuxResourceGroupLabel.TabIndex = 1;
            this.linuxResourceGroupLabel.Text = "Resource Group";
            // 
            // linuxAppServiceNameLabel
            // 
            this.linuxAppServiceNameLabel.AutoSize = true;
            this.linuxAppServiceNameLabel.Location = new System.Drawing.Point(3, 82);
            this.linuxAppServiceNameLabel.Name = "linuxAppServiceNameLabel";
            this.linuxAppServiceNameLabel.Size = new System.Drawing.Size(104, 15);
            this.linuxAppServiceNameLabel.TabIndex = 2;
            this.linuxAppServiceNameLabel.Text = "App Service Name";
            // 
            // linuxSubscriptionComboBox
            // 
            this.linuxSubscriptionComboBox.Location = new System.Drawing.Point(130, 3);
            this.linuxSubscriptionComboBox.Name = "linuxSubscriptionComboBox";
            this.linuxSubscriptionComboBox.Size = new System.Drawing.Size(281, 23);
            this.linuxSubscriptionComboBox.TabIndex = 11;
            this.linuxSubscriptionComboBox.DataSource = this.LinSubscriptions;
            this.linuxSubscriptionComboBox.DisplayMember = "Name";
            this.linuxSubscriptionComboBox.ValueMember = "Id";
            this.linuxSubscriptionComboBox.SelectionChangeCommitted += new System.EventHandler(async (s, e) => await this.linuxSubscriptionComboBox_SelectedIndexChanged(s, e));
            // 
            // linuxResourceGroupComboBox
            // win
            this.linuxResourceGroupComboBox.Location = new System.Drawing.Point(130, 44);
            this.linuxResourceGroupComboBox.Name = "linuxResourceGroupComboBox";
            this.linuxResourceGroupComboBox.Size = new System.Drawing.Size(281, 23);
            this.linuxResourceGroupComboBox.TabIndex = 12;
            this.linuxResourceGroupComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a Resource Group");
            this.linuxResourceGroupComboBox.SelectionChangeCommitted += new System.EventHandler(async(s,e) => await this.linuxResourceGroupComboBox_SelectedIndexChanged(s, e));
            //
            // linuxAppServiceComboBox
            // 
            this.linuxAppServiceComboBox.Location = new System.Drawing.Point(130, 85);
            this.linuxAppServiceComboBox.Name = "linuxAppServiceComboBox";
            this.linuxAppServiceComboBox.Size = new System.Drawing.Size(281, 23);
            this.linuxAppServiceComboBox.TabIndex = 14;
            this.linuxAppServiceComboBox.DataSource = HelperUtils.GetDefaultDropdownList("Select a WordPress App");
            //
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.bottomTableLayoutPanel1);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(1, 346);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(454, 50);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // bottomTableLayoutPanel1
            // 
            this.bottomTableLayoutPanel1.ColumnCount = 2;
            this.bottomTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomTableLayoutPanel1.Controls.Add(this.bottomTableLayoutPanel2, 1, 0);
            this.bottomTableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.bottomTableLayoutPanel1.Name = "bottomTableLayoutPanel1";
            this.bottomTableLayoutPanel1.RowCount = 1;
            this.bottomTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomTableLayoutPanel1.Size = new System.Drawing.Size(451, 46);
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
            this.bottomTableLayoutPanel2.Location = new System.Drawing.Point(228, 3);
            this.bottomTableLayoutPanel2.Name = "bottomTableLayoutPanel2";
            this.bottomTableLayoutPanel2.RowCount = 1;
            this.bottomTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomTableLayoutPanel2.Size = new System.Drawing.Size(220, 40);
            this.bottomTableLayoutPanel2.TabIndex = 0;
            // 
            // migrateButton
            // 
            this.migrateButton.Location = new System.Drawing.Point(76, 3);
            this.migrateButton.Name = "migrateButton";
            this.migrateButton.Size = new System.Drawing.Size(67, 34);
            this.migrateButton.TabIndex = 0;
            this.migrateButton.Text = "Migrate";
            this.migrateButton.UseVisualStyleBackColor = true;
            this.migrateButton.Click += new System.EventHandler(this.migrateButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(149, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(68, 34);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // MigrationUX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 396);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.mainFlowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
    }
}