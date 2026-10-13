using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Script / conversation / quest editor.
    /// Organizes by Kind → Category (NPC or quest folder). Full binary dialog trees
    /// remain in cooked uassets; this editor holds text stubs and editable script files.
    /// </summary>
    public partial class ScriptEditorForm : Form
    {
        private readonly List<ScriptDocument> _docs = new List<ScriptDocument>();
        private ScriptDocument _current;
        private string _currentPath;

        public ScriptEditorForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Script Editor");
            ApplyTheme();
            ReloadTree();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            tree.BackColor = Color.FromArgb(36, 36, 42);
            tree.ForeColor = Color.White;
            txtBody.BackColor = Color.FromArgb(20, 22, 26);
            txtBody.ForeColor = Color.FromArgb(210, 220, 210);
            txtBody.Font = new Font("Consolas", 10F);
            foreach (Control c in Controls)
            {
                if (c is TextBox tb && tb != txtBody) { tb.BackColor = Color.FromArgb(40, 40, 48); tb.ForeColor = Color.White; }
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void ReloadTree()
        {
            tree.Nodes.Clear();
            _docs.Clear();

            // From open mod Scripts/
            if (CurrentModContext.IsOpen)
            {
                foreach (string f in Directory.GetFiles(CurrentModContext.ScriptsDir, "*.script.txt"))
                {
                    try { _docs.Add(ScriptDocument.Load(f)); }
                    catch { }
                }
                // also store path via tag later
            }

            // Seed from analysis if available
            string reports = Path.Combine(ConfigService.ModsOutputPath ?? "", "Analysis");
            if (!Directory.Exists(reports))
                reports = ConfigService.ModsOutputPath;
            if (Directory.Exists(reports))
            {
                foreach (var d in ScriptDocument.DiscoverFromAnalysisReports(reports))
                {
                    if (!_docs.Any(x => string.Equals(x.GamePath, d.GamePath, StringComparison.OrdinalIgnoreCase)))
                        _docs.Add(d);
                }
            }

            var byKind = _docs.GroupBy(d => d.Kind ?? "Other").OrderBy(g => g.Key);
            foreach (var kindGroup in byKind)
            {
                var kindNode = tree.Nodes.Add(kindGroup.Key);
                kindNode.Tag = "kind:" + kindGroup.Key;
                foreach (var catGroup in kindGroup.GroupBy(d => d.Category ?? "General").OrderBy(g => g.Key))
                {
                    var catNode = kindNode.Nodes.Add(catGroup.Key + " (" + catGroup.Count() + ")");
                    catNode.Tag = "cat:" + kindGroup.Key + "/" + catGroup.Key;
                    foreach (var doc in catGroup.OrderBy(d => d.Title))
                    {
                        var leaf = catNode.Nodes.Add(doc.Title);
                        leaf.Tag = doc;
                    }
                }
            }
            tree.ExpandAll();
            lblStatus.Text = _docs.Count + " script entries";
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || !(e.Node.Tag is ScriptDocument))
                return;
            _current = (ScriptDocument)e.Node.Tag;
            _currentPath = null;
            if (CurrentModContext.IsOpen)
            {
                string candidate = Path.Combine(CurrentModContext.ScriptsDir, _current.FileName);
                if (File.Exists(candidate)) _currentPath = candidate;
            }
            txtKind.Text = _current.Kind;
            txtCategory.Text = _current.Category;
            txtTitle.Text = _current.Title;
            txtGamePath.Text = _current.GamePath;
            txtBody.Text = _current.Body;
        }

        private void ApplyFields()
        {
            if (_current == null) return;
            _current.Kind = txtKind.Text.Trim();
            _current.Category = txtCategory.Text.Trim();
            _current.Title = txtTitle.Text.Trim();
            _current.GamePath = txtGamePath.Text.Trim();
            _current.Body = txtBody.Text;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod (Object Editors → Open Mod) to save scripts into the project.", "Script Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ApplyFields();
            _currentPath = Path.Combine(CurrentModContext.ScriptsDir, _current.FileName);
            _current.Save(_currentPath);
            lblStatus.Text = "Saved: " + _currentPath;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            ApplyFields();
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Script text|*.script.txt;*.txt|All|*.*";
                sfd.FileName = _current.FileName;
                if (CurrentModContext.IsOpen) sfd.InitialDirectory = CurrentModContext.ScriptsDir;
                if (sfd.ShowDialog() != DialogResult.OK) return;
                _currentPath = sfd.FileName;
                _current.Save(_currentPath);
                lblStatus.Text = "Saved: " + _currentPath;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _current = new ScriptDocument
            {
                Kind = "Dialog",
                Category = "NewNPC",
                Title = "NewConversation",
                Body = "[lines]\r\nSpeaker: Text goes here\r\n"
            };
            _currentPath = null;
            txtKind.Text = _current.Kind;
            txtCategory.Text = _current.Category;
            txtTitle.Text = _current.Title;
            txtGamePath.Text = "";
            txtBody.Text = _current.Body;
            _docs.Add(_current);
            ReloadTree();
        }

        private void btnImportMod_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod first.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ApplyFields();
            btnSave_Click(sender, e);
            // Also place a staging note under dialogs path
            string rel = "1Data/NPCs/Dialogs/" + (_current.Category ?? "General");
            if (string.Equals(_current.Kind, "Quest", StringComparison.OrdinalIgnoreCase))
                rel = "1Data/Quests/" + (_current.Category ?? "General");
            string dir = Path.Combine(CurrentModContext.StagingContentDir, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(dir);
            File.Copy(_currentPath, Path.Combine(dir, Path.GetFileName(_currentPath)), true);
            File.WriteAllText(Path.Combine(dir, "SCRIPT_README.txt"),
                "Script imported from Script Editor.\r\nGamePath: " + _current.GamePath + "\r\n" +
                "Cooked dialog assets must still be exported via FModel and property-edited in UAssetGUI (UE 4.27 profile).\r\n");
            MessageBox.Show(this, "Script saved into mod Scripts/ and staged under:\n" + dir, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e) => ReloadTree();

        private void btnUAsset_Click(object sender, EventArgs e)
        {
            ExternalToolLauncher.Launch("uassetgui", this);
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
