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
    /// Browse all SB2_*_LATEST analysis reports with search and open-in-explorer.
    /// </summary>
    public partial class AnalysisReportViewerForm : Form
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _root;

        public AnalysisReportViewerForm(string preferredRoot = null)
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Analysis Reports");
            ApplyTheme();
            _root = preferredRoot;
            if (string.IsNullOrEmpty(_root) || !Directory.Exists(_root))
            {
                _root = Path.Combine(ConfigService.ModsOutputPath ?? "", "Analysis");
                if (!Directory.Exists(_root))
                    _root = ConfigService.ModsOutputPath;
            }
            txtRoot.Text = _root ?? "";
            ReloadList();
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lstReports.BackColor = Color.FromArgb(36, 36, 42);
            lstReports.ForeColor = Color.White;
            txtContent.BackColor = Color.FromArgb(20, 22, 26);
            txtContent.ForeColor = Color.FromArgb(210, 220, 210);
            txtContent.Font = new Font("Consolas", 9.5F);
            txtSearch.BackColor = Color.FromArgb(40, 40, 48);
            txtSearch.ForeColor = Color.White;
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

        private void ReloadList()
        {
            _files.Clear();
            lstReports.Items.Clear();
            txtContent.Clear();
            var roots = new List<string>();
            if (!string.IsNullOrEmpty(txtRoot.Text) && Directory.Exists(txtRoot.Text))
                roots.Add(txtRoot.Text);
            if (!string.IsNullOrEmpty(ConfigService.ModsOutputPath))
            {
                roots.Add(ConfigService.ModsOutputPath);
                roots.Add(Path.Combine(ConfigService.ModsOutputPath, "Analysis"));
            }
            string appReports = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "GameAnalysisReports");
            try { roots.Add(Path.GetFullPath(appReports)); } catch { }

            string[] patterns =
            {
                "SB2_GameAnalysis_LATEST.md",
                "SB2_ModTargets_LATEST.txt",
                "SB2_ToolCrossRef_LATEST.txt",
                "SB2_AssetHealth_LATEST.txt",
                "SB2_UE427_ModdingGuide_LATEST.txt",
                "SB2_UE4_PakStructure_LATEST.txt",
                "SB2_Formats_LATEST.txt",
                "SB2_LooseFiles_LATEST.txt",
                "SB2_ContentMap_LATEST.txt",
                "SB2_AuthorFeedback_LATEST.txt",
                "SB2_GameAnalysis_LATEST.json"
            };

            foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(root)) continue;
                foreach (string pat in patterns)
                {
                    string path = Path.Combine(root, pat);
                    if (File.Exists(path) && !_files.ContainsKey(pat))
                        _files[pat] = path;
                }
                // Also any other SB2_*_LATEST.*
                try
                {
                    foreach (string f in Directory.GetFiles(root, "SB2_*_LATEST.*"))
                    {
                        string name = Path.GetFileName(f);
                        if (!_files.ContainsKey(name))
                            _files[name] = f;
                    }
                }
                catch { }
            }

            foreach (var kv in _files.OrderBy(x => x.Key))
            {
                long len = 0;
                try { len = new FileInfo(kv.Value).Length; } catch { }
                lstReports.Items.Add(kv.Key + "  (" + FormatSize(len) + ")");
            }
            lblStatus.Text = _files.Count + " report file(s) found";
            if (lstReports.Items.Count > 0)
            {
                lstReports.SelectedIndex = 0;
                ShowSelected();
            }
        }

        private static string FormatSize(long b)
        {
            if (b < 1024) return b + " B";
            if (b < 1024 * 1024) return (b / 1024.0).ToString("0.0") + " KB";
            return (b / (1024.0 * 1024)).ToString("0.00") + " MB";
        }

        private void ShowSelected()
        {
            if (lstReports.SelectedItem == null) return;
            string item = lstReports.SelectedItem.ToString();
            string key = item;
            int p = item.IndexOf("  (");
            if (p > 0) key = item.Substring(0, p);
            if (!_files.TryGetValue(key, out string path) || !File.Exists(path))
            {
                txtContent.Text = "(file missing)";
                return;
            }
            lblPath.Text = path;
            try
            {
                // Cap huge files in viewer
                var fi = new FileInfo(path);
                if (fi.Length > 2_000_000)
                {
                    using (var sr = new StreamReader(path))
                    {
                        char[] buf = new char[500_000];
                        int n = sr.Read(buf, 0, buf.Length);
                        txtContent.Text = new string(buf, 0, n) +
                            "\r\n\r\n--- truncated for viewer (" + FormatSize(fi.Length) + " total). Open in external editor for full file. ---";
                    }
                }
                else
                    txtContent.Text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                txtContent.Text = "Error reading file:\r\n" + ex.Message;
            }
            ApplySearchHighlight();
        }

        private void ApplySearchHighlight()
        {
            // Plain TextBox — just jump to first match
            string q = txtSearch.Text;
            if (string.IsNullOrWhiteSpace(q) || string.IsNullOrEmpty(txtContent.Text)) return;
            int idx = txtContent.Text.IndexOf(q, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                txtContent.SelectionStart = idx;
                txtContent.SelectionLength = q.Length;
                txtContent.ScrollToCaret();
            }
        }

        private void lstReports_SelectedIndexChanged(object sender, EventArgs e) => ShowSelected();
        private void btnReload_Click(object sender, EventArgs e) => ReloadList();
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Folder containing SB2_*_LATEST reports";
                if (Directory.Exists(txtRoot.Text)) fbd.SelectedPath = txtRoot.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtRoot.Text = fbd.SelectedPath;
                    ReloadList();
                }
            }
        }
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            string dir = Path.GetDirectoryName(lblPath.Text);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                dir = txtRoot.Text;
            if (Directory.Exists(dir))
                ReportViewerShell.StartExplorer(dir);
        }
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(lblPath.Text))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = lblPath.Text,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { ApplySearchHighlight(); e.Handled = true; e.SuppressKeyPress = true; }
        }
        private void btnFind_Click(object sender, EventArgs e) => ApplySearchHighlight();
        private void btnClose_Click(object sender, EventArgs e) => Close();
        private void btnWorkflow_Click(object sender, EventArgs e)
        {
            using (var w = new ModWorkflowWizardForm())
                w.ShowDialog(this);
        }
    }

    internal static class ReportViewerShell
    {
        public static void StartExplorer(string path)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "\"" + path + "\"",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
