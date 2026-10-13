using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Shared runtime state so Analyzer → Content Browser / SpecVert / Script Editor / Mod Loader
    /// stay in sync without rewriting each form. Reads LATEST reports from ModsOutput/Analysis.
    /// </summary>
    public static class CrossToolHub
    {
        public static string LastAnalysisDir { get; private set; }
        public static DateTime? LastAnalysisUtc { get; private set; }
        public static event Action Changed;

        public static void NotifyAnalysisComplete(string outputDirectory)
        {
            LastAnalysisDir = outputDirectory;
            LastAnalysisUtc = DateTime.UtcNow;
            try { if (Changed != null) Changed.Invoke(); } catch { }
        }

        public static string ReportsRoot
        {
            get
            {
                if (!string.IsNullOrEmpty(LastAnalysisDir) && Directory.Exists(LastAnalysisDir))
                    return LastAnalysisDir;
                string a = Path.Combine(ConfigService.ModsOutputPath ?? "", "Analysis");
                return Directory.Exists(a) ? a : ConfigService.ModsOutputPath;
            }
        }

        public static string FindLatest(string fileName)
        {
            string root = ReportsRoot;
            if (string.IsNullOrEmpty(root)) return null;
            string p = Path.Combine(root, fileName);
            return File.Exists(p) ? p : null;
        }

        /// <summary>Parse MAJORCHAR / QUEST / ARMORSET / LOOSE lines from ModTargets.</summary>
        public static List<TargetHit> LoadModTargets()
        {
            var list = new List<TargetHit>();
            string f = FindLatest("SB2_ModTargets_LATEST.txt");
            if (f == null) return list;
            try
            {
                foreach (string raw in File.ReadLines(f))
                {
                    string line = raw.Trim();
                    if (line.Length < 5) continue;
                    var hit = TargetHit.Parse(line);
                    if (hit != null) list.Add(hit);
                }
            }
            catch { }
            return list;
        }

        public static string DashboardSnapshot()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Game path: " + (ConfigService.Game != null && !string.IsNullOrEmpty(ConfigService.Game.InstallPath) ? ConfigService.Game.InstallPath : "(not set)"));
            sb.AppendLine("Valid install: " + (ConfigService.Game != null && ConfigService.Game.IsValid ? "Yes" : "No"));
            sb.AppendLine("UnrealPak: " + (File.Exists(ConfigService.UnrealPakPath) ? ConfigService.UnrealPakPath : "(not found)"));
            sb.AppendLine("FModel: " + (File.Exists(ConfigService.FModelPath) ? "OK" : "(not found)"));
            sb.AppendLine("UAssetGUI: " + (File.Exists(ConfigService.UAssetGuiPath) ? "OK" : "(not found)"));
            sb.AppendLine("UModel: " + (File.Exists(ConfigService.UModelPath) ? "OK" : "(not found)"));
            sb.AppendLine("Mods output: " + (ConfigService.ModsOutputPath ?? ""));
            sb.AppendLine("Reports: " + (ReportsRoot ?? "(none)"));
            sb.AppendLine("Last analysis: " + (LastAnalysisUtc.HasValue ? LastAnalysisUtc.Value.ToLocalTime().ToString("g") : "see LATEST files"));
            sb.AppendLine("Vanilla core edits: " + (ConfigService.AllowVanillaCoreEdits ? "ALLOWED" : "blocked"));
            var targets = LoadModTargets();
            int mc = targets.Count(t => t.Kind == "MAJORCHAR");
            int q = targets.Count(t => t.Kind == "QUEST");
            int ar = targets.Count(t => t.Kind == "ARMORSET");
            sb.AppendLine("Targets: " + mc + " major chars, " + q + " quests, " + ar + " armor sets, " + targets.Count + " total lines");
            string risk = FindLatest("SB2_InjectionRisk_LATEST.txt");
            if (risk != null)
            {
                foreach (string line in File.ReadLines(risk).Take(8))
                    if (line.IndexOf('=') > 0) sb.AppendLine(line.Trim());
            }
            return sb.ToString();
        }

        public class TargetHit
        {
            public string Kind, Name, StagingHint, Raw;
            public static TargetHit Parse(string line)
            {
                // MAJORCHAR Name (~n) — stage path
                // QUEST Name — stage path
                // ARMORSET M_1 (~n) — stage path
                // LOOSE MOVIES: ...
                string kind = null;
                if (line.StartsWith("MAJORCHAR ", StringComparison.OrdinalIgnoreCase)) kind = "MAJORCHAR";
                else if (line.StartsWith("QUEST ", StringComparison.OrdinalIgnoreCase)) kind = "QUEST";
                else if (line.StartsWith("ARMORSET ", StringComparison.OrdinalIgnoreCase)) kind = "ARMORSET";
                else if (line.StartsWith("CINEMATIC ", StringComparison.OrdinalIgnoreCase)) kind = "CINEMATIC";
                else if (line.StartsWith("LOOSE ", StringComparison.OrdinalIgnoreCase)) kind = "LOOSE";
                else if (line.StartsWith("GAMEPLAY ", StringComparison.OrdinalIgnoreCase)) kind = "GAMEPLAY";
                else return null;
                string rest = line.Substring(kind.Length).Trim();
                string name = rest;
                int dash = rest.IndexOf("—");
                if (dash < 0) dash = rest.IndexOf(" - ");
                string stage = "";
                if (dash > 0)
                {
                    name = rest.Substring(0, dash).Trim();
                    stage = rest.Substring(dash).Trim().TrimStart('—', '-').Trim();
                    if (stage.StartsWith("stage ", StringComparison.OrdinalIgnoreCase))
                        stage = stage.Substring(6).Trim();
                }
                int tilde = name.IndexOf("(~");
                if (tilde > 0) name = name.Substring(0, tilde).Trim();
                // Strip accidental .uasset/.uexp from quest folder names
                if (name.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".uexp", StringComparison.OrdinalIgnoreCase))
                    name = Path.GetFileNameWithoutExtension(name);
                if (stage.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) ||
                    stage.EndsWith(".uexp", StringComparison.OrdinalIgnoreCase))
                {
                    stage = stage.Replace(".uasset", "").Replace(".uexp", "").Replace(".UASSET", "").Replace(".UEXP", "");
                    int slash = stage.LastIndexOf('/');
                    // keep folder path without filename if it looked like file
                }
                return new TargetHit { Kind = kind, Name = name, StagingHint = stage, Raw = line };
            }
        }
    }

    /// <summary>Suggest Speckage / Mod Loader order from analysis load-order bands + target kind.</summary>
    public static class ModInjectionPlanner
    {
        public static int SuggestOrder(string kindOrPath)
        {
            string s = kindOrPath ?? "";
            if (s.IndexOf("LOOSE", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Movie", StringComparison.OrdinalIgnoreCase) >= 0) return 80;
            if (s.IndexOf("CINEMATIC", StringComparison.OrdinalIgnoreCase) >= 0) return 80;
            if (s.IndexOf("QUEST", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Dialog", StringComparison.OrdinalIgnoreCase) >= 0) return 90;
            if (s.IndexOf("UI", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Widget", StringComparison.OrdinalIgnoreCase) >= 0) return 60;
            if (s.IndexOf("MAJORCHAR", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("ARMOR", StringComparison.OrdinalIgnoreCase) >= 0) return 50;
            if (s.IndexOf("FMOD", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Audio", StringComparison.OrdinalIgnoreCase) >= 0) return 70;
            if (s.IndexOf("LOCRES", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("CONFIG", StringComparison.OrdinalIgnoreCase) >= 0) return 20;
            if (s.IndexOf("Ship", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0) return 30;
            return 40;
        }

        public static string PlanSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Injection plan (from last ModTargets + bands)");
            sb.AppendLine("Use Speckage DefaultLoadOrder / Mod Loader order matching these bands.");
            foreach (var t in CrossToolHub.LoadModTargets().Take(40))
                sb.AppendLine(string.Format("  [{0,2}] {1} {2} → {3}", SuggestOrder(t.Kind + " " + t.Name), t.Kind, t.Name, t.StagingHint));
            return sb.ToString();
        }
    }
}
