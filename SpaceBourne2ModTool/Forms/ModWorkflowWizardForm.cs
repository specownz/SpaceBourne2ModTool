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
    /// Guided modding workflow based on analysis targets and available external tools.
    /// </summary>
    public partial class ModWorkflowWizardForm : Form
    {
        public ModWorkflowWizardForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Mod Workflow");
            ApplyTheme();
            cmbGoal.Items.AddRange(new object[]
            {
                "Replace a tutorial / cutscene video (Loose Movies)",
                "Replace FMOD audio banks (Loose)",
                "Reskin a major character (cooked assets)",
                "Edit NPC dialogs / 1Data",
                "Override quest data",
                "Pack staged cooked overrides into z_*_P.pak",
                "Localization (.locres) pass",
                "Config / INI tweak"
            });
            cmbGoal.SelectedIndex = 0;
            RefreshToolStatus();
            ShowPlan();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            txtPlan.BackColor = Color.FromArgb(20, 22, 26);
            txtPlan.ForeColor = Color.FromArgb(210, 220, 210);
            txtPlan.Font = new Font("Segoe UI", 10F);
            cmbGoal.BackColor = Color.FromArgb(40, 40, 48);
            cmbGoal.ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.BackColor = Color.FromArgb(50, 50, 58);
                    b.ForeColor = Color.White;
                }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void RefreshToolStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tools:");
            sb.AppendLine("  UnrealPak: " + Status(ConfigService.UnrealPakPath));
            sb.AppendLine("  FModel:    " + Status(ConfigService.FModelPath));
            sb.AppendLine("  UModel:    " + Status(ConfigService.UModelPath));
            sb.AppendLine("  UAssetGUI: " + Status(ConfigService.UAssetGuiPath));
            sb.AppendLine("  Game:      " + (ConfigService.Game.IsValid ? "OK" : "not set"));
            lblTools.Text = sb.ToString();
        }

        private static string Status(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "missing";
            return File.Exists(path) ? "OK — " + Path.GetFileName(path) : "path invalid";
        }

        private void cmbGoal_SelectedIndexChanged(object sender, EventArgs e) => ShowPlan();

        private void ShowPlan()
        {
            int i = cmbGoal.SelectedIndex;
            var sb = new StringBuilder();
            sb.AppendLine("Goal: " + cmbGoal.SelectedItem);
            sb.AppendLine();
            switch (i)
            {
                case 0:
                    sb.AppendLine("1. Tools → Loose File Mods");
                    sb.AppendLine("2. Pick a Movies/*.mp4 (82 files on disk)");
                    sb.AppendLine("3. Choose your replacement .mp4 → Apply (auto-backup)");
                    sb.AppendLine("4. Launch game and open the related tutorial");
                    sb.AppendLine();
                    sb.AppendLine("No PAK packing required.");
                    break;
                case 1:
                    sb.AppendLine("1. Tools → Loose File Mods → FMOD");
                    sb.AppendLine("2. Replace Master.bank / Master.strings.bank carefully");
                    sb.AppendLine("3. Keep the automatic backup — FMOD banks affect most audio");
                    break;
                case 2:
                    sb.AppendLine("1. Content Browser → Majorcharacter preset (e.g. N_GuzelWoman)");
                    sb.AppendLine("2. Scaffold Mod Here");
                    sb.AppendLine("3. Launch FModel (UE 4.27) → export matching paths");
                    sb.AppendLine("4. Edit in UAssetGUI (4.27) or re-cook in UE Editor");
                    sb.AppendLine("5. Place .uasset+.uexp(+.ubulk) under staged SB2/Content/...");
                    sb.AppendLine("6. Code Editor → Generate Response → UnrealPak -compress → z_*_P.pak");
                    sb.AppendLine("7. Copy PAK into game Content/Paks");
                    break;
                case 3:
                    sb.AppendLine("1. Content Browser → 1Data/NPCs/Dialogs (~2832 assets)");
                    sb.AppendLine("2. Scaffold → FModel export specific dialog package");
                    sb.AppendLine("3. Small property edits in UAssetGUI only — test often");
                    sb.AppendLine("4. Pack as z_*_P.pak");
                    break;
                case 4:
                    sb.AppendLine("1. Content Browser → 1Data/Quests or Quests/Obsesif");
                    sb.AppendLine("2. Same cooked override pipeline as dialogs");
                    sb.AppendLine("3. Prefer existing package paths (AssetRegistry)");
                    break;
                case 5:
                    sb.AppendLine("1. Ensure staging folder has SB2/Content/...");
                    sb.AppendLine("2. Code Editor → Generate Response from staging root");
                    sb.AppendLine("3. UnrealPak 4.27: <pak> -Create=response.txt -compress");
                    sb.AppendLine("4. Name z_YourMod_P.pak → copy to Content/Paks");
                    sb.AppendLine("Mount point must stay: ../../../");
                    break;
                case 6:
                    sb.AppendLine("1. FModel or UnrealPak extract .locres (~51 in pak)");
                    sb.AppendLine("2. Community locres text tools → edit → rebuild");
                    sb.AppendLine("3. Stage under same Localization path → _P.pak");
                    break;
                case 7:
                    sb.AppendLine("1. Content Browser → SB2 Config preset");
                    sb.AppendLine("2. Stage SB2/Config/DefaultGame.ini (or matching path)");
                    sb.AppendLine("3. Pack _P.pak — test carefully (gameplay side effects)");
                    break;
            }
            sb.AppendLine();
            sb.AppendLine("Tip: Run Game Analyzer → Report Viewer → ToolCrossRef for live tool status.");
            txtPlan.Text = sb.ToString();
        }

        private void btnLaunchFModel_Click(object sender, EventArgs e) => ExternalToolLauncher.Launch("fmodel", this);
        private void btnLaunchUAsset_Click(object sender, EventArgs e) => ExternalToolLauncher.Launch("uassetgui", this);
        private void btnLaunchUModel_Click(object sender, EventArgs e) => ExternalToolLauncher.Launch("umodel", this);
        private void btnLoose_Click(object sender, EventArgs e)
        {
            using (var f = new LooseModForm()) f.ShowDialog(this);
        }
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (var f = new ContentBrowserForm()) f.ShowDialog(this);
        }
        private void btnEditor_Click(object sender, EventArgs e)
        {
            using (var f = new CodeEditorForm()) f.ShowDialog(this);
        }
        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
