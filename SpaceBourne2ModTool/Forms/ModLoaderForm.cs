using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;
using SpecFramework;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ModLoaderForm : Form
    {
        private readonly ModLoaderService _loader = new ModLoaderService();
        private ModLoaderState _state;

        public ModLoaderForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Mod Loader");
            ApplyTheme();
            RefreshList();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            listMods.BackColor = Color.FromArgb(36, 36, 42);
            listMods.ForeColor = Color.White;
            txtLog.BackColor = Color.FromArgb(20, 22, 26);
            txtLog.ForeColor = Color.FromArgb(200, 220, 200);
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

        private void RefreshList()
        {
            _state = _loader.LoadState();
            listMods.Items.Clear();
            foreach (var m in _state.Mods.OrderBy(x => x.LoadOrder).ThenBy(x => x.Name))
            {
                string line = string.Format("[{0}] ord={1}  {2} v{3}  ({4})",
                    m.Enabled ? "ON" : "off",
                    m.LoadOrder,
                    m.Name,
                    m.Version,
                    Path.GetFileName(m.PackagePath ?? ""));
                listMods.Items.Add(line);
            }
            lblLib.Text = "Library: " + _loader.LibraryDirectory;
        }

        private ModLoadEntry SelectedMod()
        {
            if (listMods.SelectedIndex < 0) return null;
            var ordered = _state.Mods.OrderBy(x => x.LoadOrder).ThenBy(x => x.Name).ToList();
            if (listMods.SelectedIndex >= ordered.Count) return null;
            return ordered[listMods.SelectedIndex];
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Speckage / SB2 mod|*.speckage;*.sb2mod;*.zip|All|*.*";
                ofd.Title = "Import Speckage package";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var entry = _loader.ImportPackage(ofd.FileName);
                    AppendLog("Imported: " + entry.Name + " v" + entry.Version);
                    RefreshList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            var m = SelectedMod();
            if (m == null) return;
            m.Enabled = !m.Enabled;
            _loader.SaveState(_state);
            RefreshList();
        }

        private void btnOrderUp_Click(object sender, EventArgs e)
        {
            var m = SelectedMod();
            if (m == null) return;
            m.LoadOrder = Math.Max(0, m.LoadOrder - 10);
            _loader.SaveState(_state);
            RefreshList();
        }

        private void btnOrderDown_Click(object sender, EventArgs e)
        {
            var m = SelectedMod();
            if (m == null) return;
            m.LoadOrder += 10;
            _loader.SaveState(_state);
            RefreshList();
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            if (!ConfigService.Game.IsValid)
            {
                MessageBox.Show(this, "Set a valid game path in Settings first.", "Mod Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string bak;
            var log = _loader.InjectEnabledMods(out bak);
            foreach (var line in log) AppendLog(line);
            MessageBox.Show(this,
                "Injection finished.\nBackup: " + bak + "\n\nVanilla base PAKs were not modified.",
                "Mod Loader", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshList();
        }

        private void btnUnload_Click(object sender, EventArgs e)
        {
            var log = _loader.UnloadAllManaged();
            foreach (var line in log) AppendLog(line);
            RefreshList();
        }

        private void btnInjectLaunch_Click(object sender, EventArgs e)
        {
            btnInject_Click(sender, e);
            string msg;
            if (_loader.StartGame(out msg))
                AppendLog("Game: " + msg);
            else
                MessageBox.Show(this, msg, "Start Game", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var m = SelectedMod();
            if (m == null) return;
            if (MessageBox.Show(this, "Remove " + m.Name + " from library?", "Remove", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            _state.Mods.RemoveAll(x => x.Id == m.Id);
            _loader.SaveState(_state);
            try
            {
                string dir = Path.Combine(_loader.LibraryDirectory, m.Id);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
            catch { }
            RefreshList();
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void AppendLog(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);
        }
    }
}
