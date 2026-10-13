using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class GamePathDialog : Form
    {
        public string SelectedPath { get; private set; }

        public GamePathDialog()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            txtPath.Text = ConfigService.Game.InstallPath ?? "";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select SpaceBourne 2 install folder (contains SB2 subfolder)";
                fbd.SelectedPath = txtPath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtPath.Text = fbd.SelectedPath;
            }
        }

        private void btnAutoDetect_Click(object sender, EventArgs e)
        {
            btnAutoDetect.Enabled = false;
            try
            {
                var r = Core.ToolAutoDetector.DetectAll();
                if (!string.IsNullOrEmpty(r.GameInstallPath))
                {
                    txtPath.Text = r.GameInstallPath;
                    MessageBox.Show("Found:\n" + r.GameInstallPath, "Auto-detect", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Could not find SpaceBourne 2.\nSearched Steam libraries (registry + libraryfolders.vdf) and common drives.\n\n" + r.SummaryText(),
                        "Auto-detect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Auto-detect error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAutoDetect.Enabled = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!FileService.IsValidGamePath(txtPath.Text))
            {
                var result = MessageBox.Show(
                    "The selected path does not look like a valid SpaceBourne 2 install (missing SB2\\Content\\Paks).\n\nUse it anyway?",
                    "Validation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;
            }
            SelectedPath = txtPath.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
