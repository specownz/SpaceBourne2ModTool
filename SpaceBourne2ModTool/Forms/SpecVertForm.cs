using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class SpecVertForm : Form
    {
        public SpecVertForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — specVert");
            ApplyTheme();
            txtReports.Text = Path.Combine(ConfigService.ModsOutputPath ?? "", "Analysis");
            if (!Directory.Exists(txtReports.Text))
                txtReports.Text = ConfigService.ModsOutputPath ?? "";
            txtWorkspace.Text = SpecVertService.DefaultWorkspaceRoot();
        }

        void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            txtLog.BackColor = Color.FromArgb(20, 22, 26);
            txtLog.ForeColor = Color.FromArgb(200, 220, 200);
            foreach (Control c in Controls)
            {
                if (c is TextBox tb && tb != txtLog) { tb.BackColor = Color.FromArgb(40, 40, 48); tb.ForeColor = Color.White; }
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        async void btnRun_Click(object sender, EventArgs e)
        {
            btnRun.Enabled = false;
            txtLog.Clear();
            string reports = txtReports.Text.Trim();
            string ws = txtWorkspace.Text.Trim();
            try
            {
                var result = await Task.Run(() => SpecVertService.BuildWorkspace(reports, ws, msg =>
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            txtLog.AppendText(msg + Environment.NewLine);
                        }));
                    }
                    catch { }
                }));
                txtLog.AppendText("Modules: " + result.ModulesCreated + "  Path notes: " + result.PathNotes + Environment.NewLine);
                MessageBox.Show(this,
                    "specVert finished.\n\nWorkspace:\n" + result.WorkspaceRoot +
                    "\n\nThis does NOT modify game files or recompile the EXE.\nRe-run after Analyzer updates.",
                    "specVert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "specVert", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { btnRun.Enabled = true; }
        }

        void btnBrowseReports_Click(object sender, EventArgs e)
        {
            using (var f = new FolderBrowserDialog())
            {
                f.Description = "Folder with SB2_*_LATEST analysis reports";
                if (f.ShowDialog() == DialogResult.OK) txtReports.Text = f.SelectedPath;
            }
        }

        void btnBrowseWs_Click(object sender, EventArgs e)
        {
            using (var f = new FolderBrowserDialog())
            {
                f.Description = "specVert workspace root";
                if (f.ShowDialog() == DialogResult.OK) txtWorkspace.Text = f.SelectedPath;
            }
        }

        void btnOpenWs_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtWorkspace.Text))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "\"" + txtWorkspace.Text + "\"",
                    UseShellExecute = true
                });
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
