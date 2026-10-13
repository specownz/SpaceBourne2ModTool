using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class LocalizationEditorForm : Form
    {
        public LocalizationEditorForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            txtInstructions.Text = LocalizationHelper.GetInstallInstructions("xx-XX");
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
