using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;
using SpecFramework;

namespace SpaceBourne2ModTool.Forms
{
    public partial class SpeckagePackagerForm : Form
    {
        public SpeckagePackagerForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Speckage Packager");
            ApplyTheme();
            txtAuthor.Text = Environment.UserName ?? "spec";
            txtOut.Text = Path.Combine(ConfigService.ModsOutputPath ?? "", "Packages");
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is TextBox tb) { tb.BackColor = Color.FromArgb(40, 40, 48); tb.ForeColor = Color.White; }
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(190, 195, 205);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void btnBrowseSrc_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Mod staging folder (contains .pak and/or SB2/Content files)";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() == DialogResult.OK) txtSource.Text = fbd.SelectedPath;
            }
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Output folder for .speckage package";
                if (fbd.ShowDialog() == DialogResult.OK) txtOut.Text = fbd.SelectedPath;
            }
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSource.Text) || !Directory.Exists(txtSource.Text))
            {
                MessageBox.Show(this, "Select a valid source folder.", "Speckage", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var man = new SpeckageManifest
                {
                    Name = string.IsNullOrWhiteSpace(txtName.Text) ? Path.GetFileName(txtSource.Text.TrimEnd('\\', '/')) : txtName.Text.Trim(),
                    Author = txtAuthor.Text.Trim(),
                    Version = string.IsNullOrWhiteSpace(txtVersion.Text) ? "1.0.0" : txtVersion.Text.Trim(),
                    Description = txtDesc.Text.Trim(),
                    DefaultLoadOrder = (int)numOrder.Value,
                    GameId = "1646850",
                    GameName = "SpaceBourne 2"
                };
                Directory.CreateDirectory(txtOut.Text);
                string outFile = Path.Combine(txtOut.Text, PathGuard.SanitizeFileName(man.Name) + ".speckage");
                string path = SpeckagePack.CreateFromFolder(txtSource.Text, outFile, man);
                lblResult.Text = "Created: " + path;
                MessageBox.Show(this,
                    "Speckage package created:\n" + path + "\n\nShare this file for others to Import in Mod Loader.",
                    "Speckage", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Speckage", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
