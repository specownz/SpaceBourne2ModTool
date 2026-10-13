using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Models;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Compares this scan with the previous one so a game update can be sent back as a short report.
    /// </summary>
    public static class GameUpdateDiff
    {
        public const string SnapshotName = "SB2_ScanSnapshot_LATEST.txt";
        public const string DiffName = "SB2_UpdateDiff_LATEST.txt";

        public class Previous
        {
            public bool HadSnapshot;
            public Dictionary<string, long> PakSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, long> PakTicks = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> Folders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> Exts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public string Binary = "";
        }

        public static Previous Capture(string outputDirectory)
        {
            var prev = new Previous();
            if (string.IsNullOrEmpty(outputDirectory)) return prev;
            string path = Path.Combine(outputDirectory, SnapshotName);
            if (!File.Exists(path))
            {
                TryReadOlderJson(outputDirectory, prev);
                return prev;
            }
            prev.HadSnapshot = true;
            string section = "";
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("[") && line.EndsWith("]")) { section = line; continue; }
                string[] p = line.Split('\t');
                if (section == "[paks]" && p.Length >= 3)
                {
                    long sz, tk;
                    if (long.TryParse(p[1], out sz)) prev.PakSizes[p[0]] = sz;
                    if (long.TryParse(p[2], out tk)) prev.PakTicks[p[0]] = tk;
                }
                else if (section == "[folders]" && p.Length >= 2)
                {
                    int n; if (int.TryParse(p[1], out n)) prev.Folders[p[0]] = n;
                }
                else if (section == "[ext]" && p.Length >= 2)
                {
                    int n; if (int.TryParse(p[1], out n)) prev.Exts[p[0]] = n;
                }
                else if (section == "[binary]") prev.Binary = line;
            }
            return prev;
        }

        static void TryReadOlderJson(string outputDirectory, Previous prev)
        {
            string jsonPath = Path.Combine(outputDirectory, "SB2_GameAnalysis_LATEST.json");
            if (!File.Exists(jsonPath)) return;
            string json;
            try { json = File.ReadAllText(jsonPath); }
            catch { return; }
            prev.HadSnapshot = true;
            prev.Binary = "from previous SB2_GameAnalysis_LATEST.json";
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                json, "\"fileName\": \"([^\"]+)\", \"sizeBytes\": (\\d+)"))
            {
                long sz;
                if (long.TryParse(m.Groups[2].Value, out sz))
                    prev.PakSizes[m.Groups[1].Value] = sz;
            }
            var ext = System.Text.RegularExpressions.Regex.Match(json, "\"extensionCounts\": \\{([^}]*)\\}");
            if (ext.Success)
            {
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                    ext.Groups[1].Value, "\"([^\"]+)\": (\\d+)"))
                {
                    int n;
                    if (int.TryParse(m.Groups[2].Value, out n)) prev.Exts[m.Groups[1].Value] = n;
                }
            }
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                json, "\"folder:([^\"]+)\": \"(\\d+)\""))
            {
                int n;
                if (int.TryParse(m.Groups[2].Value, out n)) prev.Folders[m.Groups[1].Value] = n;
            }
        }

        public static void Emit(string outputDirectory, Previous prev, GameAnalysisReport report)
        {
            if (string.IsNullOrEmpty(outputDirectory) || report == null) return;
            Directory.CreateDirectory(outputDirectory);
            if (prev == null) prev = new Previous();

            var folders = FolderCounts(report);
            var exts = report.ExtensionCounts ?? new Dictionary<string, int>();
            var sb = new StringBuilder();
            sb.AppendLine("SpaceBourne 2 — what changed since the last scan");
            sb.AppendLine("Send this file back with SB2_GameAnalysis_LATEST.md after a game update.");
            sb.AppendLine("ScannedUtc=" + report.AnalyzedAtUtc.ToString("o"));
            sb.AppendLine("ToolVersion=" + report.ToolVersion);
            sb.AppendLine("Install=" + (report.InstallPath ?? ""));
            sb.AppendLine("Binary=" + (report.BinaryProductVersion ?? "") + " file=" + (report.BinaryFileVersion ?? ""));
            sb.AppendLine("PreviousSnapshot=" + (prev.HadSnapshot ? "yes" : "no — this is the baseline"));
            sb.AppendLine();

            if (!prev.HadSnapshot)
            {
                sb.AppendLine("No earlier snapshot was in this reports folder.");
                sb.AppendLine("Run the analyzer again after the next game update. The next report will list added, removed, and resized pak files and content folders.");
            }
            else
            {
                sb.AppendLine("== Pak files added ==");
                foreach (var p in report.PakFiles ?? new List<PakFileInfo>())
                    if (!prev.PakSizes.ContainsKey(p.FileName))
                        sb.AppendLine("ADDED\t" + p.FileName + "\t" + p.SizeBytes);
                sb.AppendLine("== Pak files removed ==");
                var nowNames = new HashSet<string>((report.PakFiles ?? new List<PakFileInfo>()).Select(p => p.FileName), StringComparer.OrdinalIgnoreCase);
                foreach (var kv in prev.PakSizes)
                    if (!nowNames.Contains(kv.Key))
                        sb.AppendLine("REMOVED\t" + kv.Key + "\t" + kv.Value);
                sb.AppendLine("== Pak files resized or rewritten ==");
                foreach (var p in report.PakFiles ?? new List<PakFileInfo>())
                {
                    long oldSz, oldTk;
                    if (!prev.PakSizes.TryGetValue(p.FileName, out oldSz)) continue;
                    prev.PakTicks.TryGetValue(p.FileName, out oldTk);
                    if (oldSz != p.SizeBytes)
                        sb.AppendLine("RESIZED\t" + p.FileName + "\t" + oldSz + "\t" + p.SizeBytes);
                    else if (oldTk != 0 && oldTk != p.LastWriteUtc.Ticks)
                        sb.AppendLine("REWRITTEN\t" + p.FileName + "\tsame size, new date");
                }
                sb.AppendLine("== Content folders that changed count ==");
                int shown = 0;
                foreach (var kv in folders.OrderByDescending(x => Math.Abs(x.Value - (prev.Folders.ContainsKey(x.Key) ? prev.Folders[x.Key] : 0))))
                {
                    int old;
                    prev.Folders.TryGetValue(kv.Key, out old);
                    if (old == kv.Value) continue;
                    sb.AppendLine((kv.Value - old).ToString("+0;-0") + "\t" + old + "\t" + kv.Value + "\t" + kv.Key);
                    if (++shown >= 80) { sb.AppendLine("… more folder changes omitted"); break; }
                }
                if (shown == 0) sb.AppendLine("(no folder-count change)");
                sb.AppendLine("== File types that changed count ==");
                foreach (var kv in exts)
                {
                    int old; prev.Exts.TryGetValue(kv.Key, out old);
                    if (old != kv.Value)
                        sb.AppendLine((kv.Value - old).ToString("+0;-0") + "\t" + kv.Key + "\t" + old + " -> " + kv.Value);
                }
                if (!string.IsNullOrEmpty(prev.Binary))
                    sb.AppendLine("PreviousBinary=" + prev.Binary);
            }

            string diffPath = Path.Combine(outputDirectory, DiffName);
            File.WriteAllText(diffPath, sb.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "SB2_UpdateDiff_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"), sb.ToString(), Encoding.UTF8);

            var snap = new StringBuilder();
            snap.AppendLine("[binary]");
            snap.AppendLine("product=" + (report.BinaryProductVersion ?? "") + " file=" + (report.BinaryFileVersion ?? ""));
            snap.AppendLine("[paks]");
            foreach (var p in (report.PakFiles ?? new List<PakFileInfo>()).OrderBy(x => x.FileName))
                snap.AppendLine(p.FileName + "\t" + p.SizeBytes + "\t" + p.LastWriteUtc.Ticks);
            snap.AppendLine("[folders]");
            foreach (var kv in folders.OrderBy(x => x.Key))
                snap.AppendLine(kv.Key + "\t" + kv.Value);
            snap.AppendLine("[ext]");
            foreach (var kv in exts.OrderBy(x => x.Key))
                snap.AppendLine(kv.Key + "\t" + kv.Value);
            File.WriteAllText(Path.Combine(outputDirectory, SnapshotName), snap.ToString(), Encoding.UTF8);
        }

        static Dictionary<string, int> FolderCounts(GameAnalysisReport report)
        {
            var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (report.ExtraMetadata == null) return d;
            foreach (var kv in report.ExtraMetadata)
            {
                if (!kv.Key.StartsWith("folder:")) continue;
                int n; if (!int.TryParse(kv.Value, out n)) continue;
                d[kv.Key.Substring("folder:".Length)] = n;
            }
            return d;
        }
    }
}
