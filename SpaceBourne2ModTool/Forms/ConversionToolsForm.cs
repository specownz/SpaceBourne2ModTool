using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;
using SpaceBourne2ModTool.Utils;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ConversionToolsForm : Form
    {
        // Image exports can be reviewed via Tools → Image Viewer (CompactImagePreview available for embeds).
    public ConversionToolsForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            ApplyContrast();
            LoadRecipes();
        }

        private void ApplyContrast()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lstRecipes.BackColor = Color.FromArgb(40, 40, 48);
            lstRecipes.ForeColor = Color.White;
            txtDetails.BackColor = Color.FromArgb(20, 20, 22);
            txtDetails.ForeColor = Color.FromArgb(220, 220, 220);
            foreach (Control c in Controls)
            {
                if (c is Label) c.ForeColor = Color.FromArgb(230, 230, 235);
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.FromArgb(50, 50, 58);
                    btn.ForeColor = Color.White;
                }
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void LoadRecipes()
        {
            lstRecipes.Items.Clear();
            foreach (var r in AssetConversionHelper.GetRecipes())
                lstRecipes.Items.Add(r.Category + "  [" + r.SourceTypes + "] → [" + r.TargetTypes + "]"
                    + (r.CanLooseReplace ? "  (loose OK)" : "  (cook+pak)"));
            if (lstRecipes.Items.Count > 0)
                lstRecipes.SelectedIndex = 0;
        }

        private void lstRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = lstRecipes.SelectedIndex;
            if (i < 0) return;
            var r = AssetConversionHelper.GetRecipes()[i];
            var sb = new StringBuilder();
            sb.AppendLine(r.Category.ToUpperInvariant());
            sb.AppendLine(new string('─', 48));
            sb.AppendLine("Source formats: " + r.SourceTypes);
            sb.AppendLine("Game formats:   " + r.TargetTypes);
            sb.AppendLine("Loose replace:  " + (r.CanLooseReplace ? "Possible on disk" : "No — must cook and PAK"));
            sb.AppendLine();
            sb.AppendLine(r.Steps);
            txtDetails.Text = sb.ToString();
        }

        private void btnWriteGuide_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(ConfigService.ModsOutputPath, "ConversionGuides");
            string path = AssetConversionHelper.WriteGuide(dir);
            AssetConversionHelper.WritePipelineBatch(dir, "Mod");
            MessageBox.Show("Wrote:\n" + path + "\n\nand CookAndPack_README.bat", "Conversion guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ProcessHelper.StartProcess("explorer.exe", "\"" + dir + "\"");
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
