using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Lists files ripped out of the game paks. The folder is separate from the Steam install.
    /// </summary>
    public class WorkingProjectForm : Form
    {
        readonly TextBox _path = new TextBox();
        readonly ComboBox _filter = new ComboBox();
        readonly ListView _list = new ListView();
        readonly TextBox _detail = new TextBox();
        readonly ProgressBar _bar = new ProgressBar();
        readonly Label _status = new Label();
        readonly TextBox _search = new TextBox();
        readonly Label _storage = new Label();
        readonly AssetPreview _preview = new AssetPreview();
        List<WorkingProjectService.Item> _all = new List<WorkingProjectService.Item>();
        List<WorkingProjectService.Item> _shown = new List<WorkingProjectService.Item>();
        bool _busy;

        public WorkingProjectForm()
        {
            Text = "Working Project Folder";
            Width = 1180;
            Height = 780;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 560);
            Font = new Font("Segoe UI", 9f);
            AppBranding.Apply(this);
            Build();
            UiTheme.Apply(this);
            _path.Text = WorkingProjectService.Resolve();
        }

        void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            Controls.Add(root);

            var intro = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "This folder is the copy of the game you mod from. Scan remembers every file that was already ripped. " +
                       "A later scan only reads files that are new or changed, unless you choose Hard rip. " +
                       "The Steam install is never opened. Copy files from here into the open mod, or open them in the matching editor."
            };
            var pathRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            _path.Width = 640;
            var browse = Btn("Choose folder", (s, e) => Browse());
            var make = Btn("Create folder", (s, e) => CreateFolder());
            var scan = Btn("Scan (skip known files)", (s, e) => StartScan(false));
            var hard = Btn("Hard rip", (s, e) => StartScan(true));
            pathRow.Controls.Add(_path);
            pathRow.Controls.Add(browse);
            pathRow.Controls.Add(make);
            pathRow.Controls.Add(scan);
            pathRow.Controls.Add(hard);
            var top = new Panel { Dock = DockStyle.Fill };
            top.Controls.Add(pathRow);
            top.Controls.Add(intro);
            root.Controls.Add(top, 0, 0);

            var mid = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            mid.Controls.Add(new Label { Text = "Show", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
            _filter.DropDownStyle = ComboBoxStyle.DropDownList;
            _filter.Width = 220;
            _filter.Items.AddRange(new object[]
            {
                "All ripped files", "Weapon", "Ship", "Armor", "Helmet", "NPC", "Dialogue", "EvE",
                "Mesh", "Texture", "Material", "Audio", "Text", "Config", "Map", "Galaxy", "Interface", "Animation",
                "Incomplete cooked files", "Made by an external tool"
            });
            _filter.SelectedIndex = 0;
            _filter.SelectedIndexChanged += (s, e) => ApplyFilter();
            mid.Controls.Add(_filter);
            mid.Controls.Add(new Label { Text = "Find", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            _search.Width = 180;
            _search.TextChanged += (s, e) => ApplyFilter();
            mid.Controls.Add(_search);
            var row2 = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 34, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            row2.Controls.Add(Btn("Import ripped folder…", (s, e) => Import(false)));
            row2.Controls.Add(Btn("Re-import changed files", (s, e) => Import(true)));
            row2.Controls.Add(Btn("Open this file", (s, e) => OpenForEdit()));
            row2.Controls.Add(Btn("Show in folder", (s, e) => ShowFile()));
            row2.Controls.Add(Btn("Copy selected into open mod", (s, e) => CopySelected()));
            row2.Controls.Add(Btn("Copy shown list into open mod", (s, e) => CopyShown()));
            row2.Controls.Add(Btn("EvE voice", (s, e) => OpenEve()));
            var midWrap = new Panel { Dock = DockStyle.Fill };
            mid.Dock = DockStyle.Top;
            mid.Height = 34;
            row2.Dock = DockStyle.Bottom;
            midWrap.Controls.Add(row2);
            midWrap.Controls.Add(mid);
            root.Controls.Add(midWrap, 0, 1);

            _list.Dock = DockStyle.Fill;
            _list.View = View.Details;
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.VirtualMode = true;
            _list.VirtualListSize = 0;
            _list.Columns.Add("File", 220);
            _list.Columns.Add("Part", 110);
            _list.Columns.Add("Piece", 200);
            _list.Columns.Add("Tool", 110);
            _list.Columns.Add("What was done", 320);
            _list.RetrieveVirtualItem += (s, e) =>
            {
                if (e.ItemIndex < 0 || e.ItemIndex >= _shown.Count) { e.Item = new ListViewItem(""); return; }
                var it = _shown[e.ItemIndex];
                var row = new ListViewItem(Path.GetFileName(it.Relative));
                row.SubItems.Add(it.Component);
                row.SubItems.Add(it.Part);
                row.SubItems.Add(it.Tool);
                row.SubItems.Add(it.What);
                e.Item = row;
            };
            _list.SelectedIndexChanged += (s, e) => ShowDetail();
            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            _list.Dock = DockStyle.Fill;
            _preview.Dock = DockStyle.Fill;
            split.Controls.Add(_list, 0, 0);
            split.Controls.Add(_preview, 1, 0);
            root.Controls.Add(split, 0, 2);

            var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _status.Dock = DockStyle.Fill;
            _status.Text = "Not scanned yet. A scan skips files already in the rip ledger.";
            _storage.Dock = DockStyle.Bottom;
            _storage.Height = 18;
            _storage.Text = "Storage is shown after a scan.";
            _bar.Dock = DockStyle.Top;
            _bar.Height = 8;
            _detail.Dock = DockStyle.Fill;
            _detail.Multiline = true;
            _detail.ReadOnly = true;
            _detail.ScrollBars = ScrollBars.Vertical;
            var statusWrap = new Panel { Dock = DockStyle.Fill };
            statusWrap.Controls.Add(_storage);
            statusWrap.Controls.Add(_status);
            statusWrap.Controls.Add(_bar);
            _bar.Dock = DockStyle.Bottom;
            bottom.Controls.Add(statusWrap, 0, 0);
            bottom.Controls.Add(_detail, 0, 1);
            root.Controls.Add(bottom, 0, 3);
        }

        Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Margin = new Padding(4, 2, 4, 2) };
            b.Click += click;
            return b;
        }

        void Browse()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Working folder for ripped game files. Not the Steam install.";
                dlg.SelectedPath = Directory.Exists(_path.Text) ? _path.Text : WorkingProjectService.DefaultPath;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _path.Text = dlg.SelectedPath;
                ConfigService.WorkingProjectPath = dlg.SelectedPath;
                ConfigService.Save();
            }
        }

        void CreateFolder()
        {
            ConfigService.WorkingProjectPath = _path.Text.Trim();
            string err;
            string made = WorkingProjectService.Ensure(out err);
            if (made == null) { MessageBox.Show(this, err, "Working folder", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            _path.Text = made;
            ConfigService.Save();
            _status.Text = "Folder is ready. Import a ripped export, then scan.";
        }

        void StartScan(bool hard)
        {
            if (_busy) return;
            if (hard && MessageBox.Show(this,
                "Hard rip reads every file again, even ones that have not changed. Use this only when a previous scan looks wrong.",
                "Hard rip", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            ConfigService.WorkingProjectPath = _path.Text.Trim();
            string err;
            string folder = WorkingProjectService.Ensure(out err);
            if (folder == null) { MessageBox.Show(this, err, "Working folder", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            _path.Text = folder;
            ConfigService.Save();
            _busy = true;
            _bar.Style = ProgressBarStyle.Marquee;
            _status.Text = hard ? "Hard rip…" : "Scanning. Known files are skipped…";
            Task.Run(() => WorkingProjectService.Scan(folder, new Progress<string>(t => BeginInvoke(new Action(() => _status.Text = t))), hard))
                .ContinueWith(t =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        _busy = false;
                        _bar.Style = ProgressBarStyle.Blocks;
                        if (t.Exception != null)
                        {
                            _status.Text = "Scan failed.";
                            MessageBox.Show(this, t.Exception.GetBaseException().Message);
                            return;
                        }
                        var scan = t.Result;
                        if (!string.IsNullOrEmpty(scan.Error))
                        {
                            _status.Text = scan.Error;
                            return;
                        }
                        _all = scan.Items;
                        ApplyFilter();
                        _storage.Text = WorkingProjectService.StorageText(folder, scan.UsedBytes);
                        _status.Text = _all.Count + " files. " + scan.Reused + " already ripped (not read again). " + scan.NewSinceLast + " new or changed. The game install was not opened.";
                    }));
                });
        }

        void ApplyFilter()
        {
            string f = _filter.SelectedItem as string ?? "All ripped files";
            IEnumerable<WorkingProjectService.Item> q = _all;
            if (f == "Incomplete cooked files")
                q = q.Where(i => (i.Trio ?? "").IndexOf("Missing", StringComparison.OrdinalIgnoreCase) >= 0);
            else if (f == "Made by an external tool")
                q = q.Where(i => i.Tool != "Pak extract" && i.Tool != "Unknown" && i.Tool != "Config edit");
            else if (f != "All ripped files")
                q = q.Where(i => i.Component == f);
            string find = (_search.Text ?? "").Trim();
            if (find.Length > 0)
                q = q.Where(i => (i.Relative ?? "").IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0
                    || (i.Component ?? "").IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0);
            _shown = q.ToList();
            _list.VirtualListSize = _shown.Count;
            _list.Invalidate();
        }

        void ShowDetail()
        {
            if (_list.SelectedIndices.Count == 0) return;
            int i = _list.SelectedIndices[0];
            if (i < 0 || i >= _shown.Count) return;
            var it = _shown[i];
            _detail.Text =
                it.Relative + "\r\n" +
                "Part of the game: " + it.Component + "\r\n" +
                "File role: " + it.Part + "\r\n" +
                "Cooked set: " + (string.IsNullOrEmpty(it.Trio) ? "n/a" : it.Trio) + "\r\n" +
                "External tool: " + it.Tool + "\r\n" +
                "What that tool did: " + it.What + "\r\n" +
                "In-app read: " + (string.IsNullOrEmpty(it.Peek) ? "No header read for this type." : it.Peek) + "\r\n" +
                "Size: " + it.Bytes + " bytes\r\n" +
                it.FullPath;
            _preview.ShowFile(it.FullPath);
        }

        WorkingProjectService.Item Selected()
        {
            if (_list.SelectedIndices.Count == 0) return null;
            int i = _list.SelectedIndices[0];
            if (i < 0 || i >= _shown.Count) return null;
            return _shown[i];
        }

        void ShowFile()
        {
            var it = Selected();
            if (it == null) return;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + it.FullPath + "\"");
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void Import(bool replaceChanged)
        {
            string err;
            string dest = WorkingProjectService.Ensure(out err);
            if (dest == null) { MessageBox.Show(this, err); return; }
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Folder of files already extracted from a pak (not the game install).";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                int copied, skipped;
                WorkingProjectService.ImportTree(dlg.SelectedPath, dest, replaceChanged, out copied, out skipped, out err);
                if (err != null) { MessageBox.Show(this, err, "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                MessageBox.Show(this, "Copied " + copied + " files. Left " + skipped + " alone because they were already ripped"
                    + (replaceChanged ? " and the same size." : ". Use Re-import changed files if a rip was updated.")
                    + " Scan to classify them.", "Import");
            }
        }

        void OpenForEdit()
        {
            var it = Selected();
            if (it == null) { MessageBox.Show(this, "Select a file first."); return; }
            string ext = Path.GetExtension(it.FullPath).ToLowerInvariant();
            try
            {
                if (ext == ".png" || ext == ".bmp" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")
                {
                    WorkspaceHost.Open(new ImageViewerForm(it.FullPath));
                    return;
                }
                if (ext == ".fbx" || ext == ".obj" || ext == ".gltf" || ext == ".glb" || ext == ".stl")
                {
                    WorkspaceHost.Open(new ModelViewerForm(it.FullPath));
                    return;
                }
                if (ext == ".wav" || ext == ".bank" || it.Component == "EvE")
                {
                    WorkspaceHost.Open(new EveVoiceForm(it.FullPath));
                    return;
                }
                if ((ext == ".uasset" || ext == ".umap") && !string.IsNullOrEmpty(ConfigService.UAssetGuiPath) && File.Exists(ConfigService.UAssetGuiPath))
                {
                    System.Diagnostics.Process.Start(ConfigService.UAssetGuiPath, "\"" + it.FullPath + "\"");
                    return;
                }
                if (!string.IsNullOrEmpty(ConfigService.CodeEditorPath) && File.Exists(ConfigService.CodeEditorPath) &&
                    (ext == ".txt" || ext == ".ini" || ext == ".lua" || ext == ".json"))
                {
                    System.Diagnostics.Process.Start(ConfigService.CodeEditorPath, "\"" + it.FullPath + "\"");
                    return;
                }
                System.Diagnostics.Process.Start(it.FullPath);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void OpenEve()
        {
            var it = Selected();
            WorkspaceHost.Open(new EveVoiceForm(it == null ? null : it.FullPath));
        }

        void CopySelected()
        {
            if (_list.SelectedIndices.Count == 0) { MessageBox.Show(this, "Select one or more files first."); return; }
            var items = new List<WorkingProjectService.Item>();
            foreach (int i in _list.SelectedIndices)
                if (i >= 0 && i < _shown.Count) items.Add(_shown[i]);
            CopyMany(items);
        }

        void CopyShown()
        {
            if (_shown.Count == 0) { MessageBox.Show(this, "Nothing is in the list."); return; }
            if (MessageBox.Show(this, "Copy " + _shown.Count + " shown files into the open mod? Files already in the mod are skipped.",
                "Copy into mod", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            CopyMany(_shown);
        }

        void CopyMany(List<WorkingProjectService.Item> items)
        {
            int copied = 0, skipped = 0, failed = 0;
            string last = "";
            foreach (var it in items)
            {
                string err;
                string dest = WorkingProjectService.CopyIntoOpenMod(it, out err);
                if (err != null && err.StartsWith("ALREADY:")) { skipped++; continue; }
                if (err != null) { failed++; last = err; continue; }
                copied++;
                last = dest;
            }
            _detail.AppendText("\r\nCopied " + copied + ". Skipped " + skipped + " already in the mod. Failed " + failed + ".\r\n" + last);
            if (failed > 0 && copied == 0) MessageBox.Show(this, last);
        }
    }
}
