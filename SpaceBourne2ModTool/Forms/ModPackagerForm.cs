using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ModPackagerForm : Form
    {
        private readonly ModProject _project;

        public ModPackagerForm(ModProject project)
        {
            _project = project;
            InitializeComponent();
            AppBranding.Apply(this);
            if (_project != null)
            {
                lblProject.Text = "Project: " + _project.Info.Name;
                txtStaging.Text = _project.StagingRoot;
            }
            else
            {
                lblProject.Text = "Project: (none – create one first)";
                btnCreate.Enabled = false;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (_project == null) return;
            bool ok = PakHelper.CreatePak(_project, out string path, out string msg);
            txtLog.AppendText(msg + Environment.NewLine);
            if (ok)
                txtLog.AppendText("Output: " + path + Environment.NewLine);
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
