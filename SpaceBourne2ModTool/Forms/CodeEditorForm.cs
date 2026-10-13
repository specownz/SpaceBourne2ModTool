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
    /// In-app editor for SB2 mod scripts: UnrealPak response files, ModInfo JSON,
    /// batch pack scripts, Cheat Engine AA snippets, path lists. Integrates catalog
    /// paths, game/tool paths, and packing templates from the rest of the app.
    /// </summary>
    public partial class CodeEditorForm : Form
    {
        private string _currentFile;
        private bool _dirty;

        public CodeEditorForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Code Editor");
            ApplyTheme();
            LoadDocumentTypes();
            txtEditor.Text = GetTemplate("response");
            _dirty = false;
            UpdateTitle();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            txtEditor.BackColor = Color.FromArgb(20, 22, 26);
            txtEditor.ForeColor = Color.FromArgb(220, 230, 220);
            txtEditor.Font = new Font("Consolas", 11F);
            cmbDocType.BackColor = Color.FromArgb(40, 40, 48);
            cmbDocType.ForeColor = Color.White;
            lstSnippets.BackColor = Color.FromArgb(36, 36, 42);
            lstSnippets.ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.BackColor = Color.FromArgb(50, 50, 58);
                    b.ForeColor = Color.White;
                }
                if (c is Label) c.ForeColor = Color.FromArgb(200, 200, 210);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void LoadDocumentTypes()
        {
            cmbDocType.Items.Clear();
            cmbDocType.Items.AddRange(new object[]
            {
                "UnrealPak response file",
                "ModInfo JSON",
                "Pack batch (.bat)",
                "Cheat Engine AA snippet",
                "Content path list",
                "Custom / plain text"
            });
            cmbDocType.SelectedIndex = 0;
            lstSnippets.Items.Clear();
            lstSnippets.Items.AddRange(new object[]
            {
                "Insert mount ../../../",
                "Insert game install path",
                "Insert UnrealPak path",
                "Insert Paks folder",
                "Insert mods output",
                "Sample: Armors/Majorcharacters/…",
                "Sample: 1Data/NPCs/Dialogs/…",
                "Sample: Movies loose replace note",
                "Full pack command template",
                "z_ModName_P.pak filename",
                "FModel export reminder",
                "Cooked trio reminder"
            });
        }

        private string GetTemplate(string key)
        {
            string game = ConfigService.Game.InstallPath ?? "C:\\Path\\To\\SpaceBourne 2";
            string pak = ConfigService.UnrealPakPath ?? "UnrealPak.exe";
            string mods = ConfigService.ModsOutputPath ?? "Mods";
            switch (key)
            {
                case "response":
                    return "; UnrealPak response file for specSB2modTool\r\n"
                        + "; Format: \"AbsoluteOrRelativeFile\" \"../../../SB2/Content/relative/path.ext\"\r\n"
                        + "; Mount point for SB2: ../../../\r\n\r\n"
                        + "\"C:\\Mods\\MyMod_P\\SB2\\Content\\Armors\\Majorcharacters\\N_GuzelWoman\\Guzel1.uasset\" \"../../../SB2/Content/Armors/Majorcharacters/N_GuzelWoman/Guzel1.uasset\"\r\n"
                        + "\"C:\\Mods\\MyMod_P\\SB2\\Content\\Armors\\Majorcharacters\\N_GuzelWoman\\Guzel1.uexp\" \"../../../SB2/Content/Armors/Majorcharacters/N_GuzelWoman/Guzel1.uexp\"\r\n";
                case "modinfo":
                    return "{\r\n  \"Name\": \"MyMod\",\r\n  \"Author\": \"" + (Environment.UserName ?? "spec") + "\",\r\n"
                        + "  \"Version\": \"1.0.0\",\r\n  \"Description\": \"SB2 content override\",\r\n"
                        + "  \"ChunkId\": 1000,\r\n  \"MountPoint\": \"../../../\",\r\n  \"Type\": \"AssetReplacement\"\r\n}\r\n";
                case "batch":
                    return "@echo off\r\nREM Pack SB2 mod with UE 4.27 UnrealPak\r\n"
                        + "set UNREALPAK=\"" + pak + "\"\r\n"
                        + "set RESPONSE=response.txt\r\n"
                        + "set OUTPAK=z_MyMod_P.pak\r\n"
                        + "%UNREALPAK% %OUTPAK% -Create=%RESPONSE% -compress\r\n"
                        + "echo Copy %OUTPAK% to:\r\n"
                        + "echo " + Path.Combine(game, "SB2", "Content", "Paks") + "\r\n"
                        + "pause\r\n";
                case "ce":
                    return "[ENABLE]\r\n// Cheat Engine AA template — offsets break on game updates\r\n"
                        + "// aobscanmodule(INJECT,SB2-Win64-Shipping.exe,48 8B ?? ?? ?? ?? ?? 48 85 C0)\r\n"
                        + "// alloc(newmem,128)\r\n// ...\r\n[DISABLE]\r\n";
                case "paths":
                    return "SB2/Content/1Data/NPCs/Dialogs/\r\nSB2/Content/Armors/Majorcharacters/\r\n"
                        + "SB2/Content/Armors/1_New_sets/\r\nSB2/Content/1Data/Quests/\r\n";
                default:
                    return "";
            }
        }

        private void cmbDocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dirty)
            {
                var r = MessageBox.Show("Replace editor content with template for this document type?",
                    "Template", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }
            string[] keys = { "response", "modinfo", "batch", "ce", "paths", "custom" };
            int i = cmbDocType.SelectedIndex;
            if (i >= 0 && i < keys.Length)
            {
                txtEditor.Text = GetTemplate(keys[i]);
                _dirty = false;
                _currentFile = null;
                UpdateTitle();
            }
        }

        private void lstSnippets_DoubleClick(object sender, EventArgs e)
        {
            if (lstSnippets.SelectedItem == null) return;
            InsertSnippet(lstSnippets.SelectedItem.ToString());
        }

        private void InsertSnippet(string name)
        {
            string ins = "";
            string game = ConfigService.Game.InstallPath ?? "";
            string pak = ConfigService.UnrealPakPath ?? "UnrealPak.exe";
            string mods = ConfigService.ModsOutputPath ?? "";
            switch (name)
            {
                case "Insert mount ../../../": ins = "../../../"; break;
                case "Insert game install path": ins = game; break;
                case "Insert UnrealPak path": ins = pak; break;
                case "Insert Paks folder":
                    ins = string.IsNullOrEmpty(game) ? "" : Path.Combine(game, "SB2", "Content", "Paks");
                    break;
                case "Insert mods output": ins = mods; break;
                case "Sample: Armors/Majorcharacters/…":
                    ins = "SB2/Content/Armors/Majorcharacters/N_GuzelWoman/";
                    break;
                case "Sample: 1Data/NPCs/Dialogs/…":
                    ins = "SB2/Content/1Data/NPCs/Dialogs/Angelas/";
                    break;
                case "Sample: Movies loose replace note":
                    ins = "// Loose: replace Content/Movies/<name>.mp4 (backup first via Loose File Mods)\r\n";
                    break;
                case "Full pack command template":
                    ins = "\"" + pak + "\" z_MyMod_P.pak -Create=response.txt -compress\r\n";
                    break;
                case "z_ModName_P.pak filename":
                    ins = "z_MyMod_P.pak";
                    break;
                case "FModel export reminder":
                    ins = "// FModel: set UE 4.27, open game PAK, export matching path, keep uasset+uexp(+ubulk)\r\n";
                    break;
                case "Cooked trio reminder":
                    ins = "// Always ship matching .uasset + .uexp (+ .ubulk when present)\r\n";
                    break;
            }
            if (string.IsNullOrEmpty(ins)) return;
            int pos = txtEditor.SelectionStart;
            txtEditor.Text = txtEditor.Text.Insert(pos, ins);
            txtEditor.SelectionStart = pos + ins.Length;
            txtEditor.Focus();
            _dirty = true;
            UpdateTitle();
        }

        private void btnInsertPath_Click(object sender, EventArgs e)
        {
            using (var f = new ContentBrowserForm())
            {
                // User can copy path from browser; also offer preset staging path
                f.ShowDialog(this);
            }
            MessageBox.Show(
                "Use Content Browser → Copy Path, then paste here (Ctrl+V).\r\nOr double-click a snippet on the left.",
                "Insert path", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard()) return;
            txtEditor.Clear();
            _currentFile = null;
            _dirty = false;
            UpdateTitle();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard()) return;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Mod scripts|*.txt;*.json;*.bat;*.ct;*.response|All|*.*";
                ofd.InitialDirectory = ConfigService.ModsOutputPath;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtEditor.Text = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                    _currentFile = ofd.FileName;
                    _dirty = false;
                    UpdateTitle();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e) => Save(false);
        private void btnSaveAs_Click(object sender, EventArgs e) => Save(true);

        private void Save(bool saveAs)
        {
            if (saveAs || string.IsNullOrEmpty(_currentFile))
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Text|*.txt|JSON|*.json|Batch|*.bat|All|*.*";
                    sfd.InitialDirectory = ConfigService.ModsOutputPath;
                    sfd.FileName = cmbDocType.SelectedIndex == 1 ? "modinfo.json" : "response.txt";
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    _currentFile = sfd.FileName;
                }
            }
            File.WriteAllText(_currentFile, txtEditor.Text, Encoding.UTF8);
            _dirty = false;
            UpdateTitle();
            // If saving under a mod project folder, track recent
            try
            {
                string dir = Path.GetDirectoryName(_currentFile);
                if (File.Exists(Path.Combine(dir, "modinfo.json")) || dir.EndsWith("_P", StringComparison.OrdinalIgnoreCase))
                    RecentModsStore.Add(dir, Path.GetFileName(dir));
            }
            catch { }
            MessageBox.Show("Saved:\n" + _currentFile, "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            var r = MessageBox.Show("Discard unsaved changes?", "Code Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return r == DialogResult.Yes;
        }

        private void txtEditor_TextChanged(object sender, EventArgs e)
        {
            _dirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(_currentFile) ? "(untitled)" : Path.GetFileName(_currentFile);
            Text = AppBranding.ProductTitle + " — Code Editor — " + name + (_dirty ? " *" : "");
            lblFile.Text = string.IsNullOrEmpty(_currentFile) ? "Not saved to disk" : _currentFile;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard()) return;
            Close();
        }

        private void btnGenerateResponse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select mod staging root (folder that contains SB2\\Content\\...)";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("; Generated by specSB2modTool Code Editor");
                sb.AppendLine("; Mount: ../../../");
                string root = fbd.SelectedPath;
                foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    string rel = file.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/');
                    // Prefer paths under SB2/Content
                    string mountRel = rel;
                    int idx = rel.IndexOf("SB2/Content/", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0) mountRel = rel.Substring(idx);
                    else if (!rel.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase))
                        mountRel = "SB2/Content/" + rel;
                    sb.AppendLine("\"" + file + "\" \"../../../" + mountRel + "\"");
                }
                txtEditor.Text = sb.ToString();
                cmbDocType.SelectedIndex = 0;
                _dirty = true;
                UpdateTitle();
            }
        }
    }
}
