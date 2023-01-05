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
    public partial class PhpPopupForm : Form
    {
        private bool _continueMigration;

        public PhpPopupForm(ref bool continueMigration)
        {
            this._continueMigration= continueMigration;
            InitializeComponent();
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
    }
}
