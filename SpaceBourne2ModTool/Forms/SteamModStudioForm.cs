using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Hub for Steam-requested mod types: grammar, world exposure, rebalance — maximized in-app automation.
    /// </summary>
    public partial class SteamModStudioForm : Form
    {
        public SteamModStudioForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Mod Opportunity Studio");
            FormScaleHelper.Enable(this);
            ApplyTheme();
            foreach (var r in SteamModRequestHub.Requests)
                lstRequests.Items.Add(r);
            lstRequests.DisplayMember = "Title";
            if (lstRequests.Items.Count > 0) lstRequests.SelectedIndex = 0;
            else txtSummary.Text = BeginnerGuide.SteamStudio;
            RefreshExposure();
        }

        void ApplyTheme()
        {
            UiTheme.Apply(this);
            if (lblTitle != null) lblTitle.ForeColor = UiTheme.Accent;
        }

        void lstRequests_SelectedIndexChanged(object sender, EventArgs e)
        {
            var r = lstRequests.SelectedItem as SteamModRequestHub.Request;
            if (r == null) return;
            txtSummary.Text = r.Summary + "\r\n\r\nIn-app: " + r.InAppPath + "\r\nTemplate: " + r.TemplateId;
        }

        void btnScaffold_Click(object sender, EventArgs e)
        {
            var r = lstRequests.SelectedItem as SteamModRequestHub.Request;
            if (r == null) return;
            string name = "Steam_" + r.Id + "_Mod";
            var t = new ModTemplateService.Template
            {
                Id = r.TemplateId,
                Name = r.Title,
                Description = r.Summary,
                Category = "SteamRequests",
                DefaultLoadOrder = r.Id == "rebalance" ? 30 : r.Id == "grammar" ? 90 : 50,
                StagingFolders = r.StagingFolders,
                TargetPaths = r.StagingFolders,
                ObjectTypes = r.Id == "rebalance" ? new[] { "Item", "Ship", "Generic" } : r.Id == "grammar" ? new[] { "Dialog", "Quest" } : new[] { "Generic", "Cinematic", "Character" },
                ScriptKinds = r.Id == "grammar" ? new[] { "Dialog", "Quest" } : new string[0],
                SpecVerted = r.Id != "grammar",
                WorkflowNotes = r.Summary
            };
            try
            {
                string root = ModTemplateService.CreateModFromTemplate(t, name);
                MessageBox.Show(this, "Scaffolded:\n" + root, "Steam Mod Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void btnCleanGrammar_Click(object sender, EventArgs e)
        {
            string before = txtGrammarIn.Text;
            string after = DialogueGrammarTool.Clean(before);
            txtGrammarOut.Text = after;
            var diff = DialogueGrammarTool.DiffLines(before, after);
            lblGrammarStatus.Text = diff.Count == 0 ? "No changes" : (diff.Count + " line(s) changed");
        }

        void btnSaveGrammar_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod project first.");
                return;
            }
            string dir = Path.Combine(CurrentModContext.ModRoot, "Scripts");
            Directory.CreateDirectory(dir);
            var doc = new ScriptDocument
            {
                Kind = "Dialog",
                Category = "GrammarFix",
                Title = "Cleaned_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                Body = txtGrammarOut.Text
            };
            doc.Save(Path.Combine(dir, doc.FileName));
            MessageBox.Show(this, "Saved to " + dir);
        }

        void RefreshExposure()
        {
            var sb = new StringBuilder();
            sb.AppendLine("World exposure from analysis / catalog (for graphics · planets · missions):");
            sb.AppendLine();
            try
            {
                foreach (var t in CrossToolHub.LoadModTargets())
                {
                    if (t.Kind == "MAJORCHAR" || t.Kind == "QUEST" || t.Kind == "ARMORSET" || t.Kind == "CINEMATIC" || t.Kind == "LOOSE")
                        sb.AppendLine(t.Kind + "  " + t.Name + "  →  " + t.StagingHint);
                }
            }
            catch { }
            sb.AppendLine();
            sb.AppendLine("High-value folders (from SB2 analysis):");
            foreach (string f in new[] { "1Galaxy", "_Textures", "1Meshs", "Armors/Majorcharacters", "1Data/Quests", "1Data/NPCs/Dialogs", "111Cinematic", "1Widgets", "BallisticsVFX", "Locomotion", "Packes" })
                sb.AppendLine("  " + f);
            sb.AppendLine();
            sb.AppendLine("Pipeline: Analyzer → SpecVert/Content Browser → stage cooked trios → Speckage → Mod Loader.");
            sb.AppendLine("Binary package edit: UAssetGUI 4.27 (or optional UAssetAPI.dll). Texture preview: Image Viewer / FModel export.");
            txtExposure.Text = sb.ToString();
        }

        void btnAddBalance_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod first.");
                return;
            }
            string dir = Path.Combine(CurrentModContext.ModRoot, "Objects");
            Directory.CreateDirectory(dir);
            var doc = new GameObjectDocument
            {
                ObjectType = "Item",
                DisplayName = (txtBalName.Text ?? "BalanceEntry").Trim(),
                StagingRelative = "1Data",
                Notes = "Rebalance override — apply numeric fields in UAssetGUI on matching DT_/data assets when available."
            };
            doc.Properties["Health"] = txtBalHealth.Text.Trim();
            doc.Properties["Damage"] = txtBalDamage.Text.Trim();
            doc.Properties["Armor"] = txtBalArmor.Text.Trim();
            doc.Properties["Speed"] = txtBalSpeed.Text.Trim();
            doc.Properties["Cost"] = txtBalCost.Text.Trim();
            doc.Save(Path.Combine(dir, doc.SuggestedFileName()));
            MessageBox.Show(this, "Balance document saved under Objects/.");
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
