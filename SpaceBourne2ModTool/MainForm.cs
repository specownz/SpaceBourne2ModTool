using System;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpecFramework;
using SpaceBourne2ModTool.Forms;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;
using SpaceBourne2ModTool.Utils;

namespace SpaceBourne2ModTool
{
    public partial class MainForm : Form
    {
        private ModManager _modManager;
        private ModProject _currentProject;
        private readonly Color _accent = Color.FromArgb(0, 120, 215);
        private readonly Color _darkBg = Color.FromArgb(32, 32, 32);
        private readonly Color _panelBg = Color.FromArgb(45, 45, 48);

        public MainForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle);
            ApplyDarkTheme();
            try { UiTheme.Apply(this); } catch { }
            _modManager = new ModManager();
            BuildMenuStrip();
            RebuildSidebarNav();
            try
            {
                CrossToolHub.Changed += () =>
                {
                    try
                    {
                        if (!IsHandleCreated) return;
                        BeginInvoke(new Action(RefreshDashboardSnapshot));
                    }
                    catch { }
                };
            }
            catch { }
            BuildDashboardExtras();
            BuildSettingsExtras();
            BuildNewModExtras();
            LoadSettingsIntoUI();
            RefreshRecentModsList();
            UpdateStatus("Ready. Set the SpaceBourne 2 install folder on the home screen, then run a Game File Scan.");
            LoggerService.Log("MainForm initialized.");
            this.Shown += MainForm_Shown;
            InstallWorkspace();
        }

        private void InstallWorkspace()
        {
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            if (panelSidebar != null)
            {
                panelSidebar.Width = 236;
                panelSidebar.AutoScroll = true;
                panelSidebar.Dock = DockStyle.Left;
                panelSidebar.Resize += (s, e) =>
                {
                    int w = Math.Max(80, panelSidebar.ClientSize.Width);
                    if (lblTitle != null) lblTitle.Width = w;
                    foreach (Control c in panelSidebar.Controls)
                        if (c is Button) c.Width = w;
                };
            }
            WorkspaceHost.Attach(panelContent, new[]
            {
                panelDashboard, panelNewMod, panelPakTools, panelLocalization, panelTrainer, panelSettings
            });
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.Shown -= MainForm_Shown;
            if (!ConfigService.FirstRunAutoDetectDone)
            {
                using (var splash = new AutoDetectSplashForm())
                {
                    splash.ShowDialog(this);
                }
                LoadSettingsIntoUI();
                RefreshRecentModsList();
                UpdateStatus("First-run auto-detect finished.");
            }
            else
            {
                RecentModsStore.DiscoverFromDisk();
                RefreshRecentModsList();
            }
        }

        private void ApplyDarkTheme()
        {
            // High-contrast UI: near-black panels, pure white / bright cyan labels
            Color sidebarBg = Color.FromArgb(18, 18, 20);
            Color contentBg = Color.FromArgb(28, 28, 30);
            Color btnBg = Color.FromArgb(40, 40, 48);
            Color btnHover = Color.FromArgb(0, 90, 160);
            Color btnText = Color.White; // maximum contrast
            Color accentText = Color.FromArgb(100, 200, 255);

            this.BackColor = contentBg;
            this.ForeColor = Color.White;
            panelSidebar.BackColor = sidebarBg;
            panelContent.BackColor = contentBg;
            statusStrip.BackColor = sidebarBg;
            statusStrip.ForeColor = Color.White;
            toolStripStatusLabel.ForeColor = Color.White;

            if (lblTitle != null)
            {
                lblTitle.ForeColor = accentText;
                lblTitle.BackColor = Color.FromArgb(28, 28, 34);
            }

            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 80);
                    btn.BackColor = btnBg;
                    btn.ForeColor = btnText;
                    btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(14, 0, 0, 0);
                    btn.Cursor = Cursors.Hand;
                    btn.MouseEnter += (s, e) =>
                    {
                        btn.BackColor = btnHover;
                        btn.ForeColor = Color.White;
                    };
                    btn.MouseLeave += (s, e) =>
                    {
                        btn.BackColor = btnBg;
                        btn.ForeColor = btnText;
                    };
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = accentText;
                }
            }

            // Content panels: force readable label/button colors
            ApplyContrastToContainer(panelDashboard, contentBg);
            ApplyContrastToContainer(panelNewMod, contentBg);
            ApplyContrastToContainer(panelPakTools, contentBg);
            ApplyContrastToContainer(panelLocalization, contentBg);
            ApplyContrastToContainer(panelTrainer, contentBg);
            ApplyContrastToContainer(panelSettings, contentBg);
        }

        private void ApplyContrastToContainer(Control parent, Color bg)
        {
            if (parent == null) return;
            parent.BackColor = bg;
            parent.ForeColor = Color.White;
            foreach (Control c in parent.Controls)
            {
                if (c is Label)
                {
                    c.ForeColor = Color.FromArgb(230, 230, 235);
                    c.BackColor = Color.Transparent;
                }
                else if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 100);
                    btn.BackColor = Color.FromArgb(50, 50, 58);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font(btn.Font.FontFamily, btn.Font.Size, FontStyle.Bold);
                }
                else if (c is TextBox || c is ListBox || c is ComboBox || c is NumericUpDown)
                {
                    c.BackColor = Color.FromArgb(45, 45, 50);
                    c.ForeColor = Color.White;
                }
            }
        }

        private void LoadSettingsIntoUI()
        {
            lblGamePathValue.Text = string.IsNullOrEmpty(ConfigService.Game.InstallPath)
                ? "(not set)"
                : ConfigService.Game.InstallPath;
            lblGameValid.Text = ConfigService.Game.IsValid ? "✓ Valid" : "✗ Invalid / not found";
            lblGameValid.ForeColor = ConfigService.Game.IsValid ? Color.LightGreen : Color.OrangeRed;
            txtModsOutput.Text = ConfigService.ModsOutputPath;
            txtModName.Text = ConfigService.LastModName;
            txtAuthor.Text = Environment.UserName;
            numChunkId.Value = ConfigService.DefaultChunkId;
            if (txtUnrealPak != null) txtUnrealPak.Text = ConfigService.UnrealPakPath ?? "";
            if (txtFModel != null) txtFModel.Text = ConfigService.FModelPath ?? "";
            if (txtUModel != null) txtUModel.Text = ConfigService.UModelPath ?? "";
            if (txtUAssetGui != null) txtUAssetGui.Text = ConfigService.UAssetGuiPath ?? "";
            if (txtCodeEditor != null) txtCodeEditor.Text = ConfigService.CodeEditorPath ?? "";
            if (txtImageEditor != null) txtImageEditor.Text = ConfigService.ImageEditorPath ?? "";
            if (txtModelingTool != null) txtModelingTool.Text = ConfigService.ModelingToolPath ?? "";
            if (chkPreferPaid != null) chkPreferPaid.Checked = ConfigService.PreferPaidTools;
            if (chkAllowVanilla != null) chkAllowVanilla.Checked = ConfigService.AllowVanillaCoreEdits;
            if (chkAllowVanilla != null) chkAllowVanilla.Checked = ConfigService.AllowVanillaCoreEdits;
        }

        private void UpdateStatus(string msg)
        {
            toolStripStatusLabel.Text = msg;
            LoggerService.Log(msg);
        }

        #region Sidebar Navigation

        private void btnDashboard_Click(object sender, EventArgs e) => ShowPanel(panelDashboard);
        private void btnNewMod_Click(object sender, EventArgs e) => ShowPanel(panelNewMod);
        private void btnPakTools_Click(object sender, EventArgs e) => ShowPanel(panelPakTools);
        private void btnLocalization_Click(object sender, EventArgs e) => ShowPanel(panelLocalization);
        private void btnTrainer_Click(object sender, EventArgs e) => ShowPanel(panelTrainer);
        private void btnSettings_Click(object sender, EventArgs e) => ShowPanel(panelSettings);

        private void ShowPanel(Panel target)
        {
            if (target == null) return;
            WorkspaceHost.ShowHome();
            panelDashboard.Visible = false;
            panelNewMod.Visible = false;
            panelPakTools.Visible = false;
            panelLocalization.Visible = false;
            panelTrainer.Visible = false;
            panelSettings.Visible = false;
            target.Visible = true;
            target.Dock = DockStyle.Fill;
            target.BringToFront();
        }

        #endregion

        #region Dashboard

        private void btnSetGamePath_Click(object sender, EventArgs e)
        {
            using (var dlg = new GamePathDialog())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ConfigService.SetGamePath(dlg.SelectedPath);
                    LoadSettingsIntoUI();
                    UpdateStatus("Game path updated: " + dlg.SelectedPath);
                }
            }
        }

        private void btnOpenModsFolder_Click(object sender, EventArgs e)
        {
            FileService.EnsureDirectory(ConfigService.ModsOutputPath);
            ProcessHelper.StartProcess("explorer.exe", "\"" + ConfigService.ModsOutputPath + "\"");
        }

        private void btnOpenGamePaks_Click(object sender, EventArgs e)
        {
            if (!ConfigService.Game.IsValid)
            {
                MessageBox.Show("Please set a valid game path first.", "Game Path Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ProcessHelper.StartProcess("explorer.exe", "\"" + ConfigService.Game.PaksPath + "\"");
        }

        #endregion

        #region New Mod Workflow

        private void btnCreateMod_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModName.Text))
            {
                MessageBox.Show("Please enter a mod name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var info = new ModInfo
            {
                Name = txtModName.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Version = txtVersion.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                ChunkId = (int)numChunkId.Value,
                Type = (ModType)cmbModType.SelectedIndex
            };

            try
            {
                _currentProject = _modManager.CreateNewMod(info);
                bool specVert = chkSpecVerted != null && chkSpecVerted.Checked;
                if (specVert)
                {
                    SpecVertService.MarkModSpecVerted(_currentProject.ProjectRoot, true);
                    if (info.ExtraMetadata != null)
                        info.ExtraMetadata[SpecVertService.FlagKey] = "true";
                    try { CurrentModContext.Open(_currentProject.ProjectRoot); } catch { }
                }
                RecentModsStore.Add(_currentProject.ProjectRoot, info.Name);
                RefreshRecentModsList();
                lblCurrentMod.Text = "Current: " + info.Name + (specVert ? " [SpecVerted]" : "");
                UpdateStatus("Created mod project: " + info.Name);
                MessageBox.Show(
                    "Mod project created successfully!\n\n" +
                    "Location: " + _currentProject.ProjectRoot + "\n\n" +
                    "Next steps:\n" +
                    "• Add files via PAK Tools or Localization tab\n" +
                    "• Or drop assets into the staging Content folder\n" +
                    "• Then package with the Packager.",
                    "Mod Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to create mod", ex);
                MessageBox.Show("Error creating mod:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenCurrentProject_Click(object sender, EventArgs e)
        {
            if (_currentProject == null)
            {
                MessageBox.Show("No active project. Create one first.", "No Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ProcessHelper.StartProcess("explorer.exe", "\"" + _currentProject.ProjectRoot + "\"");
        }

        #endregion

        #region PAK Tools

        private void btnBrowseAddFile_Click(object sender, EventArgs e)
        {
            if (_currentProject == null)
            {
                MessageBox.Show("Create or select a mod project first.", "No Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select asset file(s) to add to mod";
                ofd.Filter = "Unreal Assets|*.uasset;*.uexp;*.ubulk;*.umap|All Files|*.*";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string rel = txtRelativePath.Text.Trim();
                    if (string.IsNullOrEmpty(rel))
                    {
                        MessageBox.Show("Enter the relative path under Content/ (e.g. Textures/MyShip/MyTexture.uasset)", "Path Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    foreach (string file in ofd.FileNames)
                    {
                        string fileRel = Path.Combine(Path.GetDirectoryName(rel) ?? "", Path.GetFileName(file)).Replace('\\', '/');
                        _currentProject.AddFile(fileRel, file);
                    }
                    UpdateStatus($"Added {ofd.FileNames.Length} file(s) to mod.");
                    RefreshFileList();
                }
            }
        }

        private void btnCreatePak_Click(object sender, EventArgs e)
        {
            if (_currentProject == null)
            {
                MessageBox.Show("Create a mod project first.", "No Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok = PakHelper.CreatePak(_currentProject, out string pakPath, out string msg);
            UpdateStatus(msg);
            MessageBox.Show(msg + (ok ? "\n\nPAK: " + pakPath : ""), ok ? "Success" : "Info", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnOpenPackager_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new ModPackagerForm(_currentProject));
        }

        private void RefreshFileList()
        {
            lstModFiles.Items.Clear();
            if (_currentProject == null) return;
            foreach (var f in _currentProject.Info.Files)
                lstModFiles.Items.Add(f);
        }

        #endregion

        #region Localization

        private void btnCreateLocTemplate_Click(object sender, EventArgs e)
        {
            string lang = cmbLanguage.SelectedItem?.ToString() ?? "en";
            string outDir = Path.Combine(ConfigService.ModsOutputPath, "LocalizationTemplates");
            string path = LocalizationHelper.CreateEditableTemplate(lang, outDir);
            UpdateStatus("Localization template created: " + path);
            MessageBox.Show(LocalizationHelper.GetInstallInstructions(lang) + "\n\nTemplate saved to:\n" + path,
                "Localization Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ProcessHelper.StartProcess("explorer.exe", "/select,\"" + path + "\"");
        }

        private void btnOpenLocEditor_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new LocalizationEditorForm());
        }

        #endregion

        #region Trainer

        private void btnGenerateTrainerDocs_Click(object sender, EventArgs e)
        {
            var options = TrainerTemplateGenerator.GetDefaultOptions();
            string outDir = Path.Combine(ConfigService.ModsOutputPath, "TrainerTemplates");
            FileService.EnsureDirectory(outDir);

            string md = Path.Combine(outDir, "SpaceBourne2_Trainer_Guide.md");
            string ct = Path.Combine(outDir, "SpaceBourne2_CE_Stub.CT.txt");
            string checklist = Path.Combine(outDir, "PointerScan_Checklist.txt");
            string practices = Path.Combine(outDir, "UE427_Modding_Practices.txt");

            TrainerTemplateGenerator.GenerateMarkdownGuide(options, md);
            TrainerTemplateGenerator.GenerateCheatEngineTableStub(options, ct);
            TrainerTemplateGenerator.GeneratePointerScanChecklist(checklist);
            File.WriteAllText(practices, ModdingPractices.GetFullGuide(), System.Text.Encoding.UTF8);

            UpdateStatus("Trainer templates + pointer-scan guide generated.");
            MessageBox.Show(
                "Generated:\n" +
                "• Trainer guide (Markdown) with full CE pointer-scan workflow\n" +
                "• CE table stub (not live offsets)\n" +
                "• Pointer-scan checklist\n" +
                "• UE 4.27 modding practices document\n\n" +
                "You still must perform live value + pointer scans for the current game build.\n" +
                "See the checklist for the exact steps used on UE4 titles.",
                "Trainer Templates", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ProcessHelper.StartProcess("explorer.exe", "\"" + outDir + "\"");
        }

        private void btnOpenTrainerConfig_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new TrainerConfigForm());
        }

        #endregion

        #region Settings

        private void btnBrowseModsOutput_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select folder where mod projects will be created";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.ModsOutputPath = fbd.SelectedPath;
                    txtModsOutput.Text = fbd.SelectedPath;
                    _modManager = new ModManager(fbd.SelectedPath);
                }
            }
        }

        private void btnBrowseUnrealPak_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "UnrealPak.exe|UnrealPak.exe|All|*.*";
                ofd.Title = "Locate UnrealPak.exe (from UE 4.27 Engine\\Binaries\\Win64)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.UnrealPakPath = ofd.FileName;
                    txtUnrealPak.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseUModel_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "umodel.exe|umodel.exe;umodel_64.exe|All|*.*";
                ofd.Title = "Locate umodel.exe (UE Viewer)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.UModelPath = ofd.FileName;
                    if (txtUModel != null) txtUModel.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseUAssetGui_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "UAssetGUI.exe|UAssetGUI.exe|All|*.*";
                ofd.Title = "Locate UAssetGUI.exe";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.UAssetGuiPath = ofd.FileName;
                    if (txtUAssetGui != null) txtUAssetGui.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseFModel_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "FModel.exe|FModel.exe|All|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.FModelPath = ofd.FileName;
                    txtFModel.Text = ofd.FileName;
                }
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtModsOutput.Text))
                ConfigService.ModsOutputPath = txtModsOutput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(txtUnrealPak.Text))
                ConfigService.UnrealPakPath = txtUnrealPak.Text.Trim();
            if (!string.IsNullOrWhiteSpace(txtFModel.Text))
                ConfigService.FModelPath = txtFModel.Text.Trim();
            if (txtUModel != null && !string.IsNullOrWhiteSpace(txtUModel.Text))
                ConfigService.UModelPath = txtUModel.Text.Trim();
            if (txtUAssetGui != null && !string.IsNullOrWhiteSpace(txtUAssetGui.Text))
                ConfigService.UAssetGuiPath = txtUAssetGui.Text.Trim();
            if (txtCodeEditor != null)
                ConfigService.CodeEditorPath = (txtCodeEditor.Text ?? "").Trim();
            if (txtImageEditor != null)
                ConfigService.ImageEditorPath = (txtImageEditor.Text ?? "").Trim();
            if (txtModelingTool != null)
                ConfigService.ModelingToolPath = (txtModelingTool.Text ?? "").Trim();
            if (chkPreferPaid != null)
                ConfigService.PreferPaidTools = chkPreferPaid.Checked;
            if (chkAllowVanilla != null)
            {
                if (chkAllowVanilla.Checked && !ConfigService.AllowVanillaCoreEdits)
                {
                    var r = MessageBox.Show(
                        "Enable editing of VANILLA CORE game files?\n\nThis can permanently break SpaceBourne 2.\nOnly use with full backups. Default remains OFF for safety.",
                        "Dangerous Setting",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (r != DialogResult.Yes)
                        chkAllowVanilla.Checked = false;
                }
                ConfigService.AllowVanillaCoreEdits = chkAllowVanilla.Checked;
            }
            ConfigService.Save();
            UpdateStatus("Settings saved.");
            MessageBox.Show("Settings saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAutoDetect_Click(object sender, EventArgs e)
        {
            using (var splash = new AutoDetectSplashForm())
            {
                splash.ShowDialog(this);
            }
            LoadSettingsIntoUI();
            if (txtUnrealPak != null) txtUnrealPak.Text = ConfigService.UnrealPakPath ?? "";
            if (txtFModel != null) txtFModel.Text = ConfigService.FModelPath ?? "";
            if (txtUModel != null) txtUModel.Text = ConfigService.UModelPath ?? "";
            if (txtUAssetGui != null) txtUAssetGui.Text = ConfigService.UAssetGuiPath ?? "";
            if (txtCodeEditor != null) txtCodeEditor.Text = ConfigService.CodeEditorPath ?? "";
            if (txtImageEditor != null) txtImageEditor.Text = ConfigService.ImageEditorPath ?? "";
            if (txtModelingTool != null) txtModelingTool.Text = ConfigService.ModelingToolPath ?? "";
            if (chkPreferPaid != null) chkPreferPaid.Checked = ConfigService.PreferPaidTools;
            if (chkAllowVanilla != null) chkAllowVanilla.Checked = ConfigService.AllowVanillaCoreEdits;
            UpdateStatus("Auto-detect finished.");
        }

        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            string log = LoggerService.GetLogPath();
            if (!string.IsNullOrEmpty(log) && File.Exists(log))
                ProcessHelper.StartProcess("notepad.exe", "\"" + log + "\"");
            else
                MessageBox.Show("Log file not found.", "Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private void btnAnalyzer_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new GameAnalyzerForm());
        }

        private void btnConversion_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new ConversionToolsForm());
        }

        private void btnLooseMods_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new LooseModForm());
        }

        private void btnContentBrowser_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new ContentBrowserForm());
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            WorkspaceHost.Open(new AboutForm());
        }


        private MenuStrip _menuStrip;
        private ListBox lstRecentMods;
        private Label lblRecentMods;
        private Label lblDownloads;
        private LinkLabel linkDownloads;
        private TextBox txtCodeEditor;
        private TextBox txtImageEditor;
        private TextBox txtModelingTool;
        private CheckBox chkPreferPaid;
        private CheckBox chkAllowVanilla;
        private Label lblCodeEditor;
        private Label lblImageEditor;
        private Label lblModelingTool;
        private Button btnBrowseCodeEditor;
        private Button btnBrowseImageEditor;
        private Button btnBrowseModeling;
        private Label lblAuthoringHdr;
        private Label lblUeToolsHdr;
        private CheckBox chkSpecVerted;


        /// <summary>Uniform Quickbar — title does not overlap buttons.</summary>
        private void RebuildSidebarNav()
        {
            if (panelSidebar == null) return;

            // Hide legacy buttons (still used from menu for some panels)
            foreach (Control c in new Control[] { btnAbout, btnTrainer, btnLocalization, btnPakTools, btnConversion, btnLooseMods })
            {
                if (c != null) c.Visible = false;
            }

            // Remove previously generated dynamic quickbar buttons
            for (int i = panelSidebar.Controls.Count - 1; i >= 0; i--)
            {
                Control c = panelSidebar.Controls[i];
                if (c != null && c.Tag != null && string.Equals(c.Tag.ToString(), "quickbar", StringComparison.Ordinal))
                    panelSidebar.Controls.RemoveAt(i);
            }

            // Header: reserved strip so buttons never sit under the label
            if (lblTitle != null)
            {
                lblTitle.Text = "Quickbar";
                lblTitle.AutoSize = false;
                lblTitle.Location = new Point(0, 0);
                lblTitle.Size = new Size(panelSidebar.Width, 40);
                lblTitle.TextAlign = ContentAlignment.MiddleLeft;
                lblTitle.Padding = new Padding(12, 0, 0, 0);
                lblTitle.Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
                lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
                lblTitle.BackColor = Color.FromArgb(28, 28, 34);
                lblTitle.Visible = true;
                lblTitle.BringToFront();
            }

            int y = 44;
            int h = 36;
            Font btnFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            Color btnBg = Color.FromArgb(32, 32, 38);
            Color btnFg = Color.White;

            y = PlaceExistingQuickButton(btnDashboard, "Dashboard", (s, e) => ShowPanel(panelDashboard), y, h, btnFont, btnBg, btnFg);
            y = PlaceExistingQuickButton(btnNewMod, "New Mod", (s, e) => ShowPanel(panelNewMod), y, h, btnFont, btnBg, btnFg);
            y = PlaceExistingQuickButton(btnAnalyzer, "Game Analyzer", btnAnalyzer_Click, y, h, btnFont, btnBg, btnFg);
            y = PlaceExistingQuickButton(btnContentBrowser, "Content Browser", btnContentBrowser_Click, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Working Folder", (s, e) => { WorkspaceHost.Open(new WorkingProjectForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Mod Templates", (s, e) => { WorkspaceHost.Open(new ModTemplateForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Steam Requests", (s, e) => { WorkspaceHost.Open(new SteamModStudioForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Object Editors", (s, e) => { WorkspaceHost.Open(new ObjectEditorHubForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Script Editor", (s, e) => { WorkspaceHost.Open(new ScriptEditorForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Mod Loader", (s, e) => { WorkspaceHost.Open(new ModLoaderForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("GitHub", (s, e) => { WorkspaceHost.Open(new GitHubHubForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Speckage", (s, e) => { WorkspaceHost.Open(new SpeckagePackagerForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("3D Model Viewer", (s, e) => { WorkspaceHost.Open(new ModelViewerForm()); }, y, h, btnFont, btnBg, btnFg);
            y = PlaceNewQuickButton("Image Viewer", (s, e) => { WorkspaceHost.Open(new ImageViewerForm()); }, y, h, btnFont, btnBg, btnFg);
            PlaceExistingQuickButton(btnSettings, "Settings", (s, e) => ShowPanel(panelSettings), y, h, btnFont, btnBg, btnFg);

            if (lblTitle != null) lblTitle.BringToFront();
        }

        private int PlaceExistingQuickButton(Button btn, string text, EventHandler handler, int y, int h, Font btnFont, Color btnBg, Color btnFg)
        {
            if (btn == null) return y;
            btn.Visible = true;
            btn.Tag = "quickbar";
            btn.Text = "  " + text;
            btn.Location = new Point(0, y); // FIXED-20261013-1402
            btn.Size = new Size(panelSidebar.Width, h);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 90, 140);
            btn.BackColor = btnBg;
            btn.ForeColor = btnFg;
            btn.Font = btnFont;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.UseVisualStyleBackColor = false;
            btn.Click -= handler;
            btn.Click += handler;
            return y + h;
        }

        private int PlaceNewQuickButton(string text, EventHandler handler, int y, int h, Font btnFont, Color btnBg, Color btnFg)
        {
            var b = new Button();
            b.Tag = "quickbar";
            b.Text = "  " + text;
            b.Location = new Point(0, y);
            b.Size = new Size(panelSidebar.Width, h);
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = btnBg;
            b.ForeColor = btnFg;
            b.Font = btnFont;
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.UseVisualStyleBackColor = false;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 90, 140);
            b.Click += handler;
            panelSidebar.Controls.Add(b);
            return y + h;
        }

        private void BuildMenuStrip()
        {
            _menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(24, 24, 28),
                ForeColor = Color.White,
                Renderer = new DarkMenuRenderer()
            };

            // File
            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.Add("New Mod Project…", null, (s, e) => ShowPanel(panelNewMod));
            file.DropDownItems.Add("Open Mod Folder…", null, (s, e) =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Open mod project";
                    fbd.SelectedPath = ConfigService.ModsOutputPath;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        try { CurrentModContext.Open(fbd.SelectedPath); UpdateStatus("Opened mod: " + CurrentModContext.DisplayName); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                }
            });
            file.DropDownItems.Add("Save Mod As (register)…", null, (s, e) => SaveModAs());
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add("Exit", null, (s, e) => Close());

            // Analyze
            var analyze = new ToolStripMenuItem("&Analyze");
            analyze.DropDownItems.Add("Game File Analyzer…", null, (s, e) => btnAnalyzer_Click(s, e));
            analyze.DropDownItems.Add("Working Project Folder…", null, (s, e) => { WorkspaceHost.Open(new WorkingProjectForm()); });
            analyze.DropDownItems.Add("EvE voice lines…", null, (s, e) => { WorkspaceHost.Open(new EveVoiceForm(null)); });
            analyze.DropDownItems.Add("Analysis Report Viewer…", null, (s, e) => OpenReportViewer());
            analyze.DropDownItems.Add("specVert Workspace…", null, (s, e) => { WorkspaceHost.Open(new SpecVertForm()); });
            analyze.DropDownItems.Add("Mod Workflow Wizard…", null, (s, e) => OpenWorkflowWizard());

            // Create
            var create = new ToolStripMenuItem("&Create");
            create.DropDownItems.Add("Mod Templates…", null, (s, e) => { WorkspaceHost.Open(new ModTemplateForm()); });
            create.DropDownItems.Add("Mod Opportunity Studio (Steam)…", null, (s, e) => { WorkspaceHost.Open(new SteamModStudioForm()); });
            create.DropDownItems.Add("Object Editors…", null, (s, e) => { WorkspaceHost.Open(new ObjectEditorHubForm()); });
            create.DropDownItems.Add("Script Editor…", null, (s, e) => { WorkspaceHost.Open(new ScriptEditorForm()); });
            create.DropDownItems.Add("Content Browser…", null, (s, e) => btnContentBrowser_Click(s, e));
            create.DropDownItems.Add("Code Editor…", null, (s, e) => OpenCodeEditor());
            create.DropDownItems.Add(new ToolStripSeparator());
            create.DropDownItems.Add("Localization…", null, (s, e) => ShowPanel(panelLocalization));
            create.DropDownItems.Add("Loose File Mods…", null, (s, e) => btnLooseMods_Click(s, e));
            create.DropDownItems.Add("Conversion Tools…", null, (s, e) => btnConversion_Click(s, e));
            create.DropDownItems.Add("Trainer Templates…", null, (s, e) => ShowPanel(panelTrainer));

            // Package
            var package = new ToolStripMenuItem("&Package");
            package.DropDownItems.Add("PAK Tools…", null, (s, e) => ShowPanel(panelPakTools));
            package.DropDownItems.Add("Mod Packager…", null, (s, e) =>
            {
                if (_currentProject == null)
                {
                    MessageBox.Show(this, "Create or open a mod project first.", "Mod Packager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowPanel(panelNewMod);
                    return;
                }
                WorkspaceHost.Open(new ModPackagerForm(_currentProject));
            });
            package.DropDownItems.Add("GitHub — publish / import mods…", null, (s, e) => { WorkspaceHost.Open(new GitHubHubForm()); });
            package.DropDownItems.Add("Speckage Packager…", null, (s, e) => { WorkspaceHost.Open(new SpeckagePackagerForm()); });
            package.DropDownItems.Add("Mod Loader…", null, (s, e) => { WorkspaceHost.Open(new ModLoaderForm()); });
            package.DropDownItems.Add("Convert mod to current game files…", null, (s, e) =>
            {
                if (!CurrentModContext.IsOpen) { MessageBox.Show(this, "Open a mod project first."); return; }
                string why;
                bool need = GameFingerprint.NeedsConversion(CurrentModContext.ModRoot, out why);
                if (!need && why != null && why.IndexOf("match", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MessageBox.Show(this, why, "No conversion needed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (MessageBox.Show(this, why + "\r\n\r\nContinue?", "Convert mod to current game files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                var res = ModConvertService.ConvertOpenModToCurrentGame(CurrentModContext.ModRoot);
                MessageBox.Show(this, res.Message, res.Converted ? "Converted" : "Convert", MessageBoxButtons.OK,
                    res.Converted ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            });
            package.DropDownItems.Add("SpecPipeline checklist / UE pipeline…", null, (s, e) => MessageBox.Show(this, SpecPipeline.StreamlineChecklist(), "SpecPipeline", MessageBoxButtons.OK, MessageBoxIcon.Information));
            package.DropDownItems.Add("Injection plan (from analysis)…", null, (s, e) => MessageBox.Show(this, ModInjectionPlanner.PlanSummary(), "Injection plan", MessageBoxButtons.OK, MessageBoxIcon.Information));

            // View
            var view = new ToolStripMenuItem("&View");
            view.DropDownItems.Add("Dashboard", null, (s, e) => ShowPanel(panelDashboard));
            view.DropDownItems.Add("3D Model Viewer…", null, (s, e) => { WorkspaceHost.Open(new ModelViewerForm()); });
            view.DropDownItems.Add("Image Viewer…", null, (s, e) => { WorkspaceHost.Open(new ImageViewerForm()); });

            // Tools
            var tools = new ToolStripMenuItem("&Tools");
            tools.DropDownItems.Add("Live session observer…", null, (s, e) => { WorkspaceHost.Open(new LiveSessionForm()); });
            tools.DropDownItems.Add("Settings…", null, (s, e) => ShowPanel(panelSettings));
            tools.DropDownItems.Add("Auto-Detect Game + Tools…", null, (s, e) => btnAutoDetect_Click(s, e));
            tools.DropDownItems.Add(new ToolStripSeparator());
            tools.DropDownItems.Add("Launch FModel", null, (s, e) => ExternalToolLauncher.Launch("fmodel", this));
            tools.DropDownItems.Add("Launch UAssetGUI", null, (s, e) => ExternalToolLauncher.Launch("uassetgui", this));
            tools.DropDownItems.Add("Launch UModel", null, (s, e) => ExternalToolLauncher.Launch("umodel", this));
            tools.DropDownItems.Add("Open Game Paks Folder", null, (s, e) => ExternalToolLauncher.Launch("paks", this));
            tools.DropDownItems.Add(new ToolStripSeparator());
            tools.DropDownItems.Add("UAssetAPI / Reflection status…", null, (s, e) => MessageBox.Show(UAssetApiBridge.StatusReport(), "UAssetAPI", MessageBoxButtons.OK, MessageBoxIcon.Information));

            // Help
            var help = new ToolStripMenuItem("&Help");
            help.DropDownItems.Add("Help section (full manual topics)…", null, (s, e) => { WorkspaceHost.Open(new HelpSectionForm()); });
            help.DropDownItems.Add("Beginner guide (read me first)…", null, (s, e) => { var guide = new HelpSectionForm(); guide.SelectTopic("2. Quick start"); WorkspaceHost.Open(guide); });
            help.DropDownItems.Add("Live scripts and programming notes…", null, (s, e) => { WorkspaceHost.Open(new ScriptingStudioForm()); });
            help.DropDownItems.Add("About specSB2modTool…", null, (s, e) => btnAbout_Click(s, e));
            help.DropDownItems.Add(new ToolStripSeparator());
            help.DropDownItems.Add("FModel", null, (s, e) => OpenUrl("https://fmodel.app/"));
            help.DropDownItems.Add("UAssetGUI", null, (s, e) => OpenUrl("https://github.com/atenfyr/UAssetGUI/releases"));
            help.DropDownItems.Add("UModel", null, (s, e) => OpenUrl("https://www.gildor.org/en/projects/umodel"));
            help.DropDownItems.Add("SpaceBourne 2 on Steam", null, (s, e) => OpenUrl("https://store.steampowered.com/app/1646850/SpaceBourne_2/"));

            _menuStrip.Items.Add(file);
            _menuStrip.Items.Add(analyze);
            _menuStrip.Items.Add(create);
            _menuStrip.Items.Add(package);
            _menuStrip.Items.Add(view);
            _menuStrip.Items.Add(tools);
            _menuStrip.Items.Add(help);
            var window = new ToolStripMenuItem("&Window");
            window.DropDownItems.Add("Home", null, (s, e) => ShowPanel(panelDashboard));
            window.DropDownItems.Add("Close this tool tab", null, (s, e) => WorkspaceHost.CloseActive());
            _menuStrip.Items.Add(window);
            MainMenuStrip = _menuStrip;
            Controls.Add(_menuStrip);
            _menuStrip.Dock = DockStyle.Top;
            _menuStrip.BringToFront();
        }

        private void BuildDashboardExtras()
        {
            if (panelDashboard == null) return;

            panelDashboard.AutoScroll = true;
            panelDashboard.HorizontalScroll.Enabled = false;
            panelDashboard.HorizontalScroll.Visible = false;
            // Give content room to scroll past the viewport
            panelDashboard.AutoScrollMinSize = new Size(0, 920);

            lblRecentMods = new Label
            {
                Text = "Recently Saved Mods",
                Location = new Point(24, 270),
                Size = new Size(300, 20),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            lstRecentMods = new ListBox
            {
                Location = new Point(24, 294),
                Size = new Size(400, 110),
                BackColor = Color.FromArgb(36, 36, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstRecentMods.DoubleClick += (s, e) => OpenSelectedRecentMod();
            var btnOpenRecent = new Button
            {
                Text = "Open Selected",
                Location = new Point(430, 294),
                Size = new Size(120, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 58),
                ForeColor = Color.White
            };
            btnOpenRecent.Click += (s, e) => OpenSelectedRecentMod();
            var btnRefreshRecent = new Button
            {
                Text = "Refresh List",
                Location = new Point(430, 328),
                Size = new Size(120, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 58),
                ForeColor = Color.White
            };
            btnRefreshRecent.Click += (s, e) => { RecentModsStore.DiscoverFromDisk(); RefreshRecentModsList(); };

            // Quick status / analysis snapshot
            var lblQuick = new Label
            {
                Text = "Modding snapshot (from last analysis / known SB2 layout)",
                Location = new Point(24, 420),
                Size = new Size(500, 20),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            var txtQuick = new TextBox
            {
                Location = new Point(24, 444),
                Size = new Size(620, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(36, 36, 42),
                ForeColor = Color.FromArgb(210, 210, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Text = BuildDashboardSnapshotText()
            };

            // Download links — full height, no clipping
            lblDownloads = new Label
            {
                Text = "Download external tools (click names)",
                Location = new Point(24, 560),
                Size = new Size(400, 20),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            linkDownloads = new LinkLabel
            {
                Location = new Point(24, 584),
                Size = new Size(620, 160),
                AutoSize = false,
                LinkColor = Color.FromArgb(120, 180, 255),
                ActiveLinkColor = Color.FromArgb(180, 220, 255),
                VisitedLinkColor = Color.FromArgb(160, 140, 220),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(36, 36, 42)
            };
            var sb = new System.Text.StringBuilder();
            foreach (var tool in ToolDownloadLinks.All)
                sb.AppendLine(tool.Name + " — " + tool.Notes);
            linkDownloads.Text = sb.ToString().TrimEnd();
            linkDownloads.Links.Clear();
            foreach (var tool in ToolDownloadLinks.All)
            {
                int idx = linkDownloads.Text.IndexOf(tool.Name, StringComparison.Ordinal);
                if (idx >= 0)
                    linkDownloads.Links.Add(idx, tool.Name.Length, tool.Url);
            }
            linkDownloads.LinkClicked += (s, e) =>
            {
                if (e.Link.LinkData != null)
                    OpenUrl(e.Link.LinkData.ToString());
            };

            // Tips row
            var lblTips = new Label
            {
                Text = "Quick tips",
                Location = new Point(24, 760),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            var txtTips = new TextBox
            {
                Location = new Point(24, 784),
                Size = new Size(620, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(36, 36, 42),
                ForeColor = Color.FromArgb(200, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Text =
                    "• Easiest mods: Loose File Mods → Movies (.mp4) or FMOD banks (no PAK).\r\n" +
                    "• Character visuals: Content Browser → Armors/Majorcharacters/<name> → Scaffold.\r\n" +
                    "• Pack with UE 4.27 UnrealPak -compress as z_YourMod_P.pak into Content/Paks.\r\n" +
                    "• Always ship matching .uasset + .uexp (+ .ubulk). Mount: ../../../\r\n" +
                    "• Run Game Analyzer once, then Content Browser → Refresh Analysis for depth-3 presets.\r\n" +
                    "• Code Editor can generate UnrealPak response files from a staging folder."
            };

            panelDashboard.Controls.Add(lblRecentMods);
            panelDashboard.Controls.Add(lstRecentMods);
            panelDashboard.Controls.Add(btnOpenRecent);
            panelDashboard.Controls.Add(btnRefreshRecent);
            panelDashboard.Controls.Add(lblQuick);
            panelDashboard.Controls.Add(txtQuick);
            panelDashboard.Controls.Add(lblDownloads);
            panelDashboard.Controls.Add(linkDownloads);
            panelDashboard.Controls.Add(lblTips);
            panelDashboard.Controls.Add(txtTips);
        }

        private void RefreshDashboardSnapshot()
        {
            try
            {
                if (panelDashboard == null) return;
                string text = BuildDashboardSnapshotText();
                foreach (Control c in panelDashboard.Controls)
                {
                    var tb = c as TextBox;
                    if (tb != null && (tb.Name == "txtSnapshot" || tb.Multiline))
                    {
                        // Prefer named snapshot box; otherwise last multiline near bottom may be snapshot
                        if (tb.Name == "txtSnapshot" || tb.ReadOnly)
                        {
                            tb.Text = text;
                            if (tb.Name == "txtSnapshot") break;
                        }
                    }
                }
            }
            catch { }
        }

        private string BuildDashboardSnapshotText()
        {
            try { return CrossToolHub.DashboardSnapshot(); }
            catch { return "Configure game path and run Auto-Detect / Game Analyzer for a live snapshot."; }
        }

        private void RefreshRecentModsList()
        {
            if (lstRecentMods == null) return;
            lstRecentMods.Items.Clear();
            foreach (var m in RecentModsStore.GetAll())
            {
                string when = m.SavedAtUtc;
                try { when = DateTime.Parse(m.SavedAtUtc).ToLocalTime().ToString("g"); } catch { }
                lstRecentMods.Items.Add(m.Name + "  |  " + when + "  |  " + m.Path);
            }
            if (lstRecentMods.Items.Count == 0)
                lstRecentMods.Items.Add("(no recent mods — save a project to see it here)");
        }

        private void OpenSelectedRecentMod()
        {
            if (lstRecentMods == null || lstRecentMods.SelectedItem == null) return;
            string s = lstRecentMods.SelectedItem.ToString();
            if (s.StartsWith("(")) return;
            string path = s.Contains("  |  ") ? s.Substring(s.LastIndexOf("  |  ") + 5).Trim() : s;
            OpenModAtPath(path);
        }

        private void OpenModProject()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select mod project folder (contains modinfo.json or SB2\\Content)";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() != DialogResult.OK) return;
                OpenModAtPath(fbd.SelectedPath);
            }
        }

        private void OpenModAtPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("Folder not found:\n" + path, "Open Mod", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                ConfigService.LastModName = Path.GetFileName(path);
                ConfigService.Save();
                RecentModsStore.Add(path, Path.GetFileName(path));
                RefreshRecentModsList();
                ProcessHelper.StartProcess("explorer.exe", "\"" + path + "\"");
                UpdateStatus("Opened mod: " + path);
                MessageBox.Show("Mod folder opened in Explorer:\n" + path + "\n\nUse Code Editor or Content Browser to continue editing.",
                    "Open Mod", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCurrentMod()
        {
            string name = txtModName != null && !string.IsNullOrWhiteSpace(txtModName.Text)
                ? txtModName.Text.Trim() : ConfigService.LastModName;
            string path = Path.Combine(ConfigService.ModsOutputPath, name);
            if (!Directory.Exists(path))
            {
                string p2 = path.EndsWith("_P") ? path : path + "_P";
                if (Directory.Exists(p2)) path = p2;
            }
            if (!Directory.Exists(path))
            {
                MessageBox.Show("No project folder found for \"" + name + "\". Create a New Mod Project or Scaffold first.",
                    "Save Mod", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowPanel(panelNewMod);
                return;
            }
            try
            {
                string info = Path.Combine(path, "modinfo.json");
                if (!File.Exists(info))
                    File.WriteAllText(info, "{\r\n  \"Name\": \"" + name.Replace("\"", "") + "\",\r\n  \"Version\": \"1.0.0\",\r\n  \"MountPoint\": \"../../../\"\r\n}\r\n");
                else
                    File.SetLastWriteTimeUtc(info, DateTime.UtcNow);
                RecentModsStore.Add(path, Path.GetFileName(path));
                RefreshRecentModsList();
                ConfigService.LastModName = Path.GetFileName(path);
                ConfigService.Save();
                UpdateStatus("Saved mod: " + path);
                MessageBox.Show("Mod saved / updated in recent list:\n" + path, "Save Mod", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveModAs()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select folder to register as a saved mod project";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() != DialogResult.OK) return;
                RecentModsStore.Add(fbd.SelectedPath, Path.GetFileName(fbd.SelectedPath));
                RefreshRecentModsList();
                UpdateStatus("Registered mod: " + fbd.SelectedPath);
            }
        }

        private void OpenCodeEditor()
        {
            var editor = new CodeEditorForm();
            editor.FormClosed += (s, ev) => RefreshRecentModsList();
            WorkspaceHost.Open(editor);
        }

        private void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show("Could not open link:\n" + ex.Message, "Link", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }



        /// <summary>
        /// Settings panel: AutoScroll, action buttons at bottom, authoring tool rows.
        /// </summary>
        private void BuildSettingsExtras()
        {
            if (panelSettings == null) return;
            panelSettings.AutoScroll = true;
            panelSettings.AutoScrollMinSize = new Size(0, 720);
            panelSettings.HorizontalScroll.Enabled = false;

            // Move action buttons to bottom so they never cover path fields
            int btnY = 640;
            if (btnAutoDetect != null)
            {
                btnAutoDetect.Location = new Point(24, btnY);
                btnAutoDetect.Size = new Size(220, 32);
            }
            if (btnSaveSettings != null)
            {
                btnSaveSettings.Location = new Point(260, btnY);
                btnSaveSettings.Size = new Size(140, 32);
            }
            if (btnOpenLog != null)
            {
                btnOpenLog.Location = new Point(410, btnY);
                btnOpenLog.Size = new Size(140, 32);
            }

            // Prefer paid checkbox under existing UE tool fields (~330 was old button row)
            chkPreferPaid = new CheckBox
            {
                Text = "Prefer paid software when auto-detecting (Sublime/Photoshop/Maya over free)",
                Location = new Point(24, 330),
                Size = new Size(600, 24),
                ForeColor = Color.White,
                Checked = ConfigService.PreferPaidTools
            };
            panelSettings.Controls.Add(chkPreferPaid);
            chkAllowVanilla = new CheckBox
            {
                Text = "Allow editing vanilla core files (DANGEROUS — base PAKs / non-_P chunks)",
                Location = new Point(24, 352),
                Size = new Size(620, 24),
                ForeColor = Color.FromArgb(255, 160, 120),
                Checked = ConfigService.AllowVanillaCoreEdits
            };
            panelSettings.Controls.Add(chkAllowVanilla);

            lblUeToolsHdr = new Label { Text = "UE / game tools", Location = new Point(24, 8), AutoSize = true, ForeColor = Color.FromArgb(100, 200, 255) };
            if (panelSettings != null && !panelSettings.Controls.Contains(lblUeToolsHdr)) panelSettings.Controls.Add(lblUeToolsHdr);
            lblAuthoringHdr = new Label
            {
                Text = "Authoring tools (textures / meshes / scripts)",
                Location = new Point(24, 385),
                Size = new Size(500, 20),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panelSettings.Controls.Add(lblAuthoringHdr);

            int y = 410;
            AddSettingsPathRow(ref y, "Code editor (Sublime, VS, VS Code… — fallback Notepad)",
                out lblCodeEditor, out txtCodeEditor, out btnBrowseCodeEditor,
                () => BrowseExeTo(txtCodeEditor), ConfigService.CodeEditorPath);
            AddSettingsPathRow(ref y, "Image editor (Photoshop, Affinity… — fallback GIMP)",
                out lblImageEditor, out txtImageEditor, out btnBrowseImageEditor,
                () => BrowseExeTo(txtImageEditor), ConfigService.ImageEditorPath);
            AddSettingsPathRow(ref y, "3D modeling (Maya, 3ds Max… — fallback Blender)",
                out lblModelingTool, out txtModelingTool, out btnBrowseModeling,
                () => BrowseExeTo(txtModelingTool), ConfigService.ModelingToolPath);

            var lblFmod = new Label
            {
                Text = "FMOD note: SB2 ships Master.bank + Master.strings.bank as loose files under Content/FMOD/Desktop (~118 MB). Replace via Loose File Mods; in-pak audio still needs cooked path overrides.",
                Location = new Point(24, y + 8),
                Size = new Size(620, 48),
                ForeColor = Color.FromArgb(180, 190, 160)
            };
            panelSettings.Controls.Add(lblFmod);
        }

        private void AddSettingsPathRow(ref int y, string labelText,
            out Label lbl, out TextBox txt, out Button browse, Action onBrowse, string initial)
        {
            lbl = new Label
            {
                Text = labelText,
                Location = new Point(24, y),
                Size = new Size(620, 18),
                ForeColor = Color.FromArgb(200, 200, 210)
            };
            y += 20;
            txt = new TextBox
            {
                Location = new Point(24, y),
                Size = new Size(530, 22),
                BackColor = Color.FromArgb(40, 40, 48),
                ForeColor = Color.White,
                Text = initial ?? ""
            };
            browse = new Button
            {
                Text = "Browse...",
                Location = new Point(560, y - 2),
                Size = new Size(80, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 58),
                ForeColor = Color.White
            };
            browse.Click += (s, e) => onBrowse();
            panelSettings.Controls.Add(lbl);
            panelSettings.Controls.Add(txt);
            panelSettings.Controls.Add(browse);
            y += 36;
        }

        private void BrowseExeTo(TextBox target)
        {
            if (target == null) return;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executables|*.exe|All|*.*";
                ofd.Title = "Select tool executable";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                    target.Text = ofd.FileName;
            }
        }



        private void BuildNewModExtras()
        {
            if (panelNewMod == null) return;
            chkSpecVerted = new CheckBox
            {
                Text = "SpecVerted workspace (modular Modules/ tree — does not rewrite game EXE)",
                Location = new Point(24, 320),
                Size = new Size(520, 24),
                ForeColor = Color.White,
                Checked = false
            };
            panelNewMod.Controls.Add(chkSpecVerted);
            if (btnCreateMod != null)
                btnCreateMod.Location = new Point(24, 360);
            if (btnOpenCurrentProject != null)
                btnOpenCurrentProject.Location = new Point(200, 360);
        }

        private void OpenReportViewer()
        {
            string root = null;
            try { root = Path.Combine(ConfigService.ModsOutputPath ?? "", "Analysis"); } catch { }
            WorkspaceHost.Open(new AnalysisReportViewerForm(root));
        }

        private void OpenWorkflowWizard()
        {
            WorkspaceHost.Open(new ModWorkflowWizardForm());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cmbModType.SelectedIndex = 0;
            cmbLanguage.SelectedIndex = 0;
            ShowPanel(panelDashboard);
            txtUnrealPak.Text = ConfigService.UnrealPakPath;
            txtFModel.Text = ConfigService.FModelPath;
        }
    }
    /// <summary>White text / black highlight for dark MenuStrip dropdowns.</summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColors()) { }

        private sealed class DarkMenuColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.Black;
            public override Color MenuItemSelectedGradientBegin => Color.Black;
            public override Color MenuItemSelectedGradientEnd => Color.Black;
            public override Color MenuItemBorder => Color.FromArgb(60, 60, 70);
            public override Color MenuBorder => Color.FromArgb(40, 40, 48);
            public override Color ToolStripDropDownBackground => Color.FromArgb(36, 36, 42);
            public override Color ImageMarginGradientBegin => Color.FromArgb(36, 36, 42);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(36, 36, 42);
            public override Color ImageMarginGradientEnd => Color.FromArgb(36, 36, 42);
            public override Color SeparatorDark => Color.FromArgb(60, 60, 70);
            public override Color SeparatorLight => Color.FromArgb(60, 60, 70);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }
    }

}
