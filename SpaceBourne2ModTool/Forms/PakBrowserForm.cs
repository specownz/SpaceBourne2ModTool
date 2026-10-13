using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class PakBrowserForm : Form
    {
        public PakBrowserForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
