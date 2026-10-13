using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class LooseModForm : Form
    {
        private string _folder;

        public LooseModForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            ApplyContrast();
            cmbTarget.Items.AddRange(new object[] { "Movies (Content/Movies)", "FMOD (Content/FMOD)" });
            cmbTarget.SelectedIndex = 0;
            txtInstall.Text = ConfigService.Game.InstallPath ?? "";
            RefreshList();
        }

        private void ApplyContrast()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lstFiles.BackColor = Color.FromArgb(40, 40, 48);
            lstFiles.ForeColor = Color.White;
            txtInstall.BackColor = Color.FromArgb(45, 45, 50);
            txtInstall.ForeColor = Color.White;
            txtReplacement.BackColor = Color.FromArgb(45, 45, 50);
            txtReplacement.ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is Label) c.ForeColor = Color.FromArgb(230, 230, 235);
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.FromArgb(50, 50, 58);
                    btn.ForeColor = Color.White;
                }
                if (c is ComboBox)
                {
                    c.BackColor = Color.FromArgb(45, 45, 50);
                    c.ForeColor = Color.White;
                }
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void btnBrowseInstall_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "SpaceBourne 2 install folder";
                fbd.SelectedPath = txtInstall.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtInstall.Text = fbd.SelectedPath;
                    RefreshList();
                }
            }
        }

        private void cmbTarget_SelectedIndexChanged(object sender, EventArgs e) => RefreshList();

        private void RefreshList()
        {
            lstFiles.Items.Clear();
            string install = txtInstall.Text.Trim();
            _folder = cmbTarget.SelectedIndex == 0
                ? LooseModHelper.GetMoviesPath(install)
                : LooseModHelper.GetFmodPath(install);

            lblFolder.Text = _folder ?? "(folder not found — set install path)";
            if (_folder == null) return;

            string pattern = cmbTarget.SelectedIndex == 0 ? "*.*" : "*.*";
            var files = LooseModHelper.ListFiles(_folder, pattern);
            foreach (var f in files)
            {
                string rel = f.FullName.Substring(_folder.Length).TrimStart(Path.DirectorySeparatorChar);
                lstFiles.Items.Add(rel + "  [" + FileService.FormatSize(f.Length) + "]");
            }
            lblCount.Text = files.Count + " files";
        }

        private void btnBrowseReplacement_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select replacement file";
                ofd.Filter = cmbTarget.SelectedIndex == 0
                    ? "Video|*.mp4;*.bk2;*.avi;*.webm|All|*.*"
                    : "Audio/FMOD|*.bank;*.wav;*.ogg;*.fsb|All|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                    txtReplacement.Text = ofd.FileName;
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (_folder == null || lstFiles.SelectedIndex < 0)
            {
                MessageBox.Show("Select a target file in the list.", "Replace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(txtReplacement.Text))
            {
                MessageBox.Show("Select a valid replacement file.", "Replace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string entry = lstFiles.SelectedItem.ToString();
            string rel = entry.Split(new[] { "  [" }, StringSplitOptions.None)[0];
            string target = Path.Combine(_folder, rel);

            var confirm = MessageBox.Show(
                "Replace:\n" + target + "\n\nWith:\n" + txtReplacement.Text +
                "\n\nA backup will be saved under _SB2ModBackup.",
                "Confirm replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                string bak = LooseModHelper.ReplaceWithBackup(target, txtReplacement.Text);
                MessageBox.Show("Replaced.\nBackup: " + bak, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => RefreshList();

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
