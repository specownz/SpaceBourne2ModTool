using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Modular hub: browse object types, open/create documents, import into current mod.
    /// </summary>
    public partial class ObjectEditorHubForm : Form
    {
        public ObjectEditorHubForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Object Editors");
            ApplyTheme();
            foreach (var t in ObjectTypeRegistry.Types)
                lstTypes.Items.Add(t.DisplayName + "  [" + t.Id + "]");
            if (lstTypes.Items.Count > 0) lstTypes.SelectedIndex = 0;
            RefreshModLabel();
            RefreshObjectList();
            CurrentModContext.Changed += RefreshModLabel;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            CurrentModContext.Changed -= RefreshModLabel;
            base.OnFormClosed(e);
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lstTypes.BackColor = Color.FromArgb(36, 36, 42);
            lstTypes.ForeColor = Color.White;
            lstObjects.BackColor = Color.FromArgb(36, 36, 42);
            lstObjects.ForeColor = Color.White;
            txtDesc.BackColor = Color.FromArgb(20, 22, 26);
            txtDesc.ForeColor = Color.FromArgb(200, 210, 200);
            foreach (Control c in Controls)
            {
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void RefreshModLabel()
        {
            if (lblMod == null) return;
            if (lblMod.InvokeRequired) { lblMod.BeginInvoke(new Action(RefreshModLabel)); return; }
            lblMod.Text = "Current mod: " + CurrentModContext.DisplayName;
        }

        private ObjectTypeRegistry.TypeDef SelectedType()
        {
            if (lstTypes.SelectedIndex < 0) return ObjectTypeRegistry.Types[0];
            return ObjectTypeRegistry.Types[lstTypes.SelectedIndex];
        }

        private void lstTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = SelectedType();
            txtDesc.Text = t.Description + "\r\n\r\nDefault staging: " + t.DefaultStaging +
                "\r\nUE4.27 note: cooked overrides need matching .uasset+.uexp(+.ubulk). Prefer existing package paths so AssetRegistry resolves them.";
            RefreshObjectList();
        }

        private void RefreshObjectList()
        {
            lstObjects.Items.Clear();
            if (!CurrentModContext.IsOpen) return;
            string dir = CurrentModContext.ObjectsDir;
            var type = SelectedType();
            foreach (string f in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    var doc = GameObjectDocument.Load(f);
                    if (!string.Equals(doc.ObjectType, type.Id, StringComparison.OrdinalIgnoreCase) &&
                        type.Id != "Generic")
                        continue;
                    lstObjects.Items.Add(Path.GetFileName(f));
                }
                catch
                {
                    lstObjects.Items.Add(Path.GetFileName(f));
                }
            }
        }

        private void btnOpenMod_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Open mod project folder";
                fbd.SelectedPath = ConfigService.ModsOutputPath;
                if (fbd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    CurrentModContext.Open(fbd.SelectedPath);
                    RefreshModLabel();
                    RefreshObjectList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Open Mod", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod project first.", "Object Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var type = SelectedType();
            var doc = new GameObjectDocument
            {
                ObjectType = type.Id,
                DisplayName = "New " + type.DisplayName,
                StagingRelative = type.DefaultStaging
            };
            foreach (string k in type.DefaultPropertyKeys)
                doc.Properties[k] = "";
            using (var ed = new ObjectEditorForm(doc))
            {
                if (ed.ShowDialog(this) == DialogResult.OK)
                    RefreshObjectList();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedItem == null || !CurrentModContext.IsOpen) return;
            string path = Path.Combine(CurrentModContext.ObjectsDir, lstObjects.SelectedItem.ToString());
            if (!File.Exists(path)) return;
            var doc = GameObjectDocument.Load(path);
            using (var ed = new ObjectEditorForm(doc, path))
            {
                if (ed.ShowDialog(this) == DialogResult.OK)
                    RefreshObjectList();
            }
        }

        private void btnImportStaging_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod first.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (lstObjects.SelectedItem == null) return;
            string path = Path.Combine(CurrentModContext.ObjectsDir, lstObjects.SelectedItem.ToString());
            var doc = GameObjectDocument.Load(path);
            string staging = CurrentModContext.StagingContentDir;
            string rel = string.IsNullOrEmpty(doc.StagingRelative)
                ? (ObjectTypeRegistry.Find(doc.ObjectType).DefaultStaging ?? "")
                : doc.StagingRelative;
            string targetDir = Path.Combine(staging, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(targetDir);
            // Write marker + copy document reference
            string marker = Path.Combine(targetDir, "_object_" + doc.Id + ".json");
            doc.Save(marker);
            File.WriteAllText(Path.Combine(targetDir, "README_IMPORT.txt"),
                "Object '" + doc.DisplayName + "' (" + doc.ObjectType + ") imported.\r\n" +
                "GamePath: " + doc.GamePath + "\r\n" +
                "Export matching cooked assets from FModel into this folder (keep .uasset+.uexp), then pack with UnrealPak.\r\n");
            MessageBox.Show(this,
                "Imported into staging:\n" + targetDir +
                "\n\nNext: FModel export → place cooked trio here → Speckage / Mod Packager.",
                "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnScript_Click(object sender, EventArgs e)
        {
            using (var f = new ScriptEditorForm())
                f.ShowDialog(this);
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void lstObjects_DoubleClick(object sender, EventArgs e) => btnEdit_Click(sender, e);
    }
}
