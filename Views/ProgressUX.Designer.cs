namespace WordPressMigrationTool.Views
{
    partial class ProgressUX
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressViewFlowLayout1 = new System.Windows.Forms.FlowLayoutPanel();
            this.progressViewRTextBox = new System.Windows.Forms.RichTextBox();
            this.progressViewFlowLayout1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressViewFlowLayout1
            // 
            this.progressViewFlowLayout1.Controls.Add(this.progressViewRTextBox);
            this.progressViewFlowLayout1.Location = new System.Drawing.Point(0, 0);
            this.progressViewFlowLayout1.Name = "progressViewFlowLayout1";
            this.progressViewFlowLayout1.Size = new System.Drawing.Size(649, 575);
            this.progressViewFlowLayout1.TabIndex = 0;
            // 
            // progressViewRTextBox
            // 
            this.progressViewRTextBox.Location = new System.Drawing.Point(3, 3);
            this.progressViewRTextBox.Name = "progressViewRTextBox";
            this.progressViewRTextBox.Size = new System.Drawing.Size(643, 569);
            this.progressViewRTextBox.TabIndex = 0;
            this.progressViewRTextBox.Text = "Initializing Migration...\n";
            // 
            // ProgressUX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressViewFlowLayout1);
            this.Name = "ProgressUX";
            this.Size = new System.Drawing.Size(649, 575);
            this.progressViewFlowLayout1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public FlowLayoutPanel progressViewFlowLayout1;
        public RichTextBox progressViewRTextBox;
    }
}
