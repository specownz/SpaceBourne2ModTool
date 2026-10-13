using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Robust content browser: search, category filter, analysis merge, stage to open mod,
    /// virtual path / response-file lines, packaging notes (Oodle/Kraken).
    /// </summary>
    public partial class ContentBrowserForm : Form
    {
        private List<ContentCatalog.NamedPreset> _allPresets = new List<ContentCatalog.NamedPreset>();
        private List<ContentCatalog.NamedPreset> _visible = new List<ContentCatalog.NamedPreset>();

        public ContentBrowserForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Content Browser");
            FormScaleHelper.Enable(this);
            try { UiTheme.Apply(this); } catch { }
            RebuildPresetList(force: true);
            FillCategoryCombo();
            ApplyFilter();
            lblHelp.Text =
                "Browse real content folders for SpaceBourne 2. Search and filter, then scaffold folders into your open mod " +
                "or copy the virtual path for FModel / package archives. Always keep .uasset with matching .uexp (+ .ubulk).";
        }

        void RebuildPresetList(bool force)
        {
            if (!force && _allPresets.Count > 0) return;
            _allPresets = new List<ContentCatalog.NamedPreset>(ContentCatalog.Presets);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in _allPresets) seen.Add(NormalizeKey(p));
            try
            {
                var snap = AnalysisImporter.LoadFromDefaultLocations();
                foreach (var extra in AnalysisImporter.ToPresets(snap))
                {
                    string k = NormalizeKey(extra);
                    if (seen.Contains(k)) continue;
                    seen.Add(k);
                    _allPresets.Add(extra);
                }
                var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (snap.Depth3Counts != null)
                    foreach (var kv in snap.Depth3Counts)
                        meta["folder:d3:" + kv.Key] = kv.Value.ToString();
                foreach (var extra in ContentCatalog.BuildDepth3Presets(meta))
                {
                    string k = NormalizeKey(extra);
                    if (seen.Contains(k)) continue;
                    seen.Add(k);
                    _allPresets.Add(extra);
                }
            }
            catch { }
            // Merge easy targets text if present
            try
            {
                string mt = CrossToolHub.FindLatest("SB2_ModTargets_LATEST.txt");
                if (mt != null)
                {
                    foreach (string line in File.ReadLines(mt))
                    {
                        if (line.IndexOf("stage ", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        int i = line.IndexOf("stage ", StringComparison.OrdinalIgnoreCase);
                        string path = line.Substring(i + 6).Trim();
                        if (string.IsNullOrEmpty(path) || path.Length < 3) continue;
                        path = path.Replace('/', '\\').Trim();
                        var np = new ContentCatalog.NamedPreset
                        {
                            Id = "scan_" + path.GetHashCode().ToString("x8"),
                            DisplayName = "Scan: " + GuessScanName(path),
                            StagingPath = path,
                            Role = GuessCategory(path),
                            SamplePaths = new[] { path }
                        };
                        string k = NormalizeKey(np);
                        if (seen.Contains(k)) continue;
                        seen.Add(k);
                        _allPresets.Add(np);
                    }
                }
            }
            catch { }
            lblCount.Text = _allPresets.Count + " locations loaded";
        }

        static string NormalizeKey(ContentCatalog.NamedPreset p)
        {
            return ((p.StagingPath ?? "") + "|" + (p.DisplayName ?? "")).ToLowerInvariant();
        }

        
        static string GuessScanName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "path";
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            return System.IO.Path.GetFileName(path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        }

        static string GuessCategory(string path)
        {
            string p = (path ?? "").Replace('\\', '/').ToLowerInvariant();
            if (p.Contains("majorcharacter") || p.Contains("armor")) return "Characters / Armor";
            if (p.Contains("quest")) return "Quests";
            if (p.Contains("npc") || p.Contains("dialog")) return "NPCs / Dialog";
            if (p.Contains("movie")) return "Movies";
            if (p.Contains("fmod") || p.Contains("audio")) return "Audio";
            if (p.Contains("widget") || p.Contains("ui")) return "UI";
            if (p.Contains("mesh")) return "Meshes";
            if (p.Contains("texture")) return "Textures";
            if (p.Contains("vfx") || p.Contains("fx")) return "VFX";
            if (p.Contains("galaxy") || p.Contains("planet")) return "World / Galaxy";
            return "Other";
        }

        void FillCategoryCombo()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("(All categories)");
            foreach (string c in _allPresets.Select(p => string.IsNullOrEmpty(p.Role) ? "Other" : p.Role)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
                cmbCategory.Items.Add(c);
            cmbCategory.SelectedIndex = 0;
        }

        void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim();
            string cat = cmbCategory.SelectedItem as string;
            bool allCat = string.IsNullOrEmpty(cat) || cat.StartsWith("(All");
            _visible = _allPresets.Where(p =>
            {
                if (!allCat)
                {
                    string pc = string.IsNullOrEmpty(p.Role) ? "Other" : p.Role;
                    if (!string.Equals(pc, cat, StringComparison.OrdinalIgnoreCase)) return false;
                }
                if (q.Length == 0) return true;
                return (p.DisplayName != null && p.DisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (p.StagingPath != null && p.StagingPath.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (p.Role != null && p.Role.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (p.Role != null && p.Role.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }).OrderBy(p => p.Role).ThenBy(p => p.DisplayName).ToList();

            lstFolders.BeginUpdate();
            lstFolders.Items.Clear();
            foreach (var p in _visible)
                lstFolders.Items.Add(FormatListItem(p));
            lstFolders.EndUpdate();
            lblCount.Text = _visible.Count + " shown / " + _allPresets.Count + " total";
            if (lstFolders.Items.Count > 0) lstFolders.SelectedIndex = 0;
            else
            {
                lstSamples.Items.Clear();
                txtDetails.Text = "No locations match this search. Clear the search box or pick All categories.";
            }
        }

        static string FormatListItem(ContentCatalog.NamedPreset p)
        {
            string cat = string.IsNullOrEmpty(p.Role) ? "Other" : p.Role;
            return "[" + cat + "] " + (p.DisplayName ?? p.StagingPath);
        }

        ContentCatalog.NamedPreset SelectedPreset()
        {
            int i = lstFolders.SelectedIndex;
            if (i < 0 || i >= _visible.Count) return null;
            return _visible[i];
        }

        void ShowPreset(ContentCatalog.NamedPreset p)
        {
            lstSamples.Items.Clear();
            if (p == null) return;
            if (p.SamplePaths != null)
                foreach (string s in p.SamplePaths.Take(40))
                    lstSamples.Items.Add(s);

            var sb = new StringBuilder();
            sb.AppendLine(p.DisplayName ?? "(unnamed)");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine("Category: " + (p.Role ?? "Other"));
            sb.AppendLine("Stage under: SB2/Content/" + (p.StagingPath ?? "").Replace('/', '\\'));
            sb.AppendLine("Virtual path: /Game/" + (p.StagingPath ?? "").Replace('\\', '/'));
            sb.AppendLine();
            if (!string.IsNullOrEmpty(p.Role))
            {
                sb.AppendLine(p.Role);
                sb.AppendLine();
            }
            sb.AppendLine("How to use this location");
            sb.AppendLine("1. Scaffold into the open mod (creates folders + TARGET note).");
            sb.AppendLine("2. Open the same path in FModel (engine 4.27) to export.");
            sb.AppendLine("3. Edit textures/meshes externally; keep cooked pairs together.");
            sb.AppendLine("4. Pack with Speckage or UnrealPak 4.27 -compress (Oodle/Kraken-capable).");
            sb.AppendLine();
            sb.AppendLine("Packing note (Oodle / Kraken)");
            sb.AppendLine("SpaceBourne 2 package archives use Oodle compression (Kraken is the usual");
            sb.AppendLine("engine default balance of size vs decode speed). Build patch archives with");
            sb.AppendLine("Unreal Engine 4.27 UnrealPak and -compress so the game can read them.");
            sb.AppendLine("Response-file line example:");
            string virt = "\"../../../SB2/Content/" + (p.StagingPath ?? "").Replace('\\', '/') + "/\" \"../../../SB2/Content/" + (p.StagingPath ?? "").Replace('\\', '/') + "/\"";
            sb.AppendLine("  " + virt);
            if (CurrentModContext.IsOpen)
                sb.AppendLine("\nOpen mod: " + CurrentModContext.ModRoot);
            else
                sb.AppendLine("\nNo mod open — open or create a mod first to scaffold here.");
            txtDetails.Text = sb.ToString();
        }

        void ScaffoldPreset(ContentCatalog.NamedPreset preset)
        {
            if (preset == null) return;
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open or create a mod project first (File menu or Templates), then scaffold again.",
                    "Content Browser", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string root = CurrentModContext.ModRoot;
            var report = new ScaffoldMergeService.Report();
            ScaffoldMergeService.EnsureContentFolder(root, preset.StagingPath, report);
            string stage = Path.Combine(root, "SB2", "Content", (preset.StagingPath ?? "").Replace('/', Path.DirectorySeparatorChar));
            string noteBody =
                "Staged by Content Browser\r\n" +
                "Name: " + (preset.DisplayName ?? "") + "\r\n" +
                "Virtual: /Game/" + (preset.StagingPath ?? "").Replace('\\', '/') + "\r\n" +
                "Place cooked .uasset+.uexp (+.ubulk) pairs here.\r\n" +
                "Pack with UE 4.27 UnrealPak -compress (Oodle).\r\n";
            ScaffoldMergeService.WriteTextMerged(this, Path.Combine(stage, "README_STAGE.txt"), noteBody, report, "stage readme");
            if (!string.IsNullOrEmpty(preset.StagingPath))
                ScaffoldMergeService.MergeLines(Path.Combine(root, "TARGET_PATHS.txt"), new[] { preset.StagingPath }, report);
            MessageBox.Show(this, "Scaffold finished (merge-safe).\r\n\r\n" + report, "Scaffolded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void ScaffoldSelectedBatch()
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod project first.", "Content Browser", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var report = new ScaffoldMergeService.Report();
            int n = 0;
            foreach (int i in lstFolders.SelectedIndices)
            {
                if (i < 0 || i >= _visible.Count) continue;
                var preset = _visible[i];
                ScaffoldMergeService.EnsureContentFolder(CurrentModContext.ModRoot, preset.StagingPath, report);
                if (!string.IsNullOrEmpty(preset.StagingPath))
                    ScaffoldMergeService.MergeLines(Path.Combine(CurrentModContext.ModRoot, "TARGET_PATHS.txt"), new[] { preset.StagingPath }, report);
                n++;
            }
            MessageBox.Show(this, "Staged " + n + " location(s) (merge-safe).\r\n\r\n" + report, "Scaffolded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void ScaffoldPresetQuiet(ContentCatalog.NamedPreset preset)
        {
            if (preset == null || !CurrentModContext.IsOpen) return;
            var report = new ScaffoldMergeService.Report();
            ScaffoldMergeService.EnsureContentFolder(CurrentModContext.ModRoot, preset.StagingPath, report);
            if (!string.IsNullOrEmpty(preset.StagingPath))
                ScaffoldMergeService.MergeLines(Path.Combine(CurrentModContext.ModRoot, "TARGET_PATHS.txt"), new[] { preset.StagingPath }, report);
        }

        // Events
        void lstFolders_SelectedIndexChanged(object sender, EventArgs e) => ShowPreset(SelectedPreset());
        void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();
        void cmbCategory_SelectedIndexChanged(object sender, EventArgs e) => ApplyFilter();
        void btnRefresh_Click(object sender, EventArgs e)
        {
            _allPresets.Clear();
            RebuildPresetList(force: true);
            FillCategoryCombo();
            ApplyFilter();
        }
        void btnScaffold_Click(object sender, EventArgs e)
        {
            if (lstFolders.SelectedIndices.Count > 1) ScaffoldSelectedBatch();
            else ScaffoldPreset(SelectedPreset());
        }
        void btnCopyPath_Click(object sender, EventArgs e)
        {
            var p = SelectedPreset();
            if (p == null) return;
            string v = "/Game/" + (p.StagingPath ?? "").Replace('\\', '/');
            try { Clipboard.SetText(v); } catch { }
            MessageBox.Show(this, "Copied:\n" + v, "Virtual path", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void btnCopyStage_Click(object sender, EventArgs e)
        {
            var p = SelectedPreset();
            if (p == null) return;
            string s = "SB2/Content/" + (p.StagingPath ?? "").Replace('/', '\\');
            try { Clipboard.SetText(s); } catch { }
            MessageBox.Show(this, "Copied:\n" + s, "Stage path", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void btnExportList_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text|*.txt";
                sfd.FileName = "ContentBrowser_Export.txt";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("specSB2modTool Content Browser export");
                sb.AppendLine("Count=" + _visible.Count);
                foreach (var p in _visible)
                    sb.AppendLine((p.Role ?? "") + "\t" + (p.DisplayName ?? "") + "\t" + (p.StagingPath ?? ""));
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show(this, "Saved:\n" + sfd.FileName);
            }
        }
        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
