using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ObjectEditorForm : Form
    {
        private GameObjectDocument _doc;
        private string _path;

        public ObjectEditorForm(GameObjectDocument doc, string existingPath = null)
        {
            _doc = doc ?? new GameObjectDocument();
            _path = existingPath;
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — " + _doc.ObjectType);
            ApplyTheme();
            LoadDoc();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            grid.BackgroundColor = Color.FromArgb(36, 36, 42);
            grid.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 48);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 58);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.EnableHeadersVisualStyles = false;
            foreach (Control c in Controls)
            {
                if (c is TextBox tb) { tb.BackColor = Color.FromArgb(40, 40, 48); tb.ForeColor = Color.White; }
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(190, 195, 205);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        private void LoadDoc()
        {
            txtName.Text = _doc.DisplayName;
            txtGamePath.Text = _doc.GamePath;
            txtStaging.Text = _doc.StagingRelative;
            txtNotes.Text = _doc.Notes;
            grid.Rows.Clear();
            foreach (var kv in _doc.Properties)
                grid.Rows.Add(kv.Key, kv.Value);
            lblType.Text = "Type: " + _doc.ObjectType;
        }

        private void ApplyToDoc()
        {
            _doc.DisplayName = txtName.Text.Trim();
            _doc.GamePath = txtGamePath.Text.Trim();
            _doc.StagingRelative = txtStaging.Text.Trim();
            _doc.Notes = txtNotes.Text;
            _doc.Properties.Clear();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                string k = Convert.ToString(row.Cells[0].Value);
                string v = Convert.ToString(row.Cells[1].Value);
                if (!string.IsNullOrWhiteSpace(k))
                    _doc.Properties[k.Trim()] = v ?? "";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "No mod open — cannot save object into project.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ApplyToDoc();
            if (string.IsNullOrEmpty(_path))
                _path = Path.Combine(CurrentModContext.ObjectsDir, _doc.SuggestedFileName());
            _doc.Save(_path);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            ApplyToDoc();
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Object JSON|*.json";
                sfd.FileName = _doc.SuggestedFileName();
                if (CurrentModContext.IsOpen)
                    sfd.InitialDirectory = CurrentModContext.ObjectsDir;
                if (sfd.ShowDialog() != DialogResult.OK) return;
                _path = sfd.FileName;
                _doc.Save(_path);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnAddProp_Click(object sender, EventArgs e)
        {
            grid.Rows.Add("NewKey", "");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
