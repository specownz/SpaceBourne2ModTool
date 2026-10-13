using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Probes configured/auto-detected external tools and records capability notes
    /// for the Game File Analyzer and cross-reference reports.
    /// </summary>
    public static class ExternalToolProbe
    {
        public class ToolProbeResult
        {
            public string Key { get; set; }
            public string DisplayName { get; set; }
            public string Path { get; set; }
            public bool Found { get; set; }
            public string VersionHint { get; set; }
            public string Role { get; set; }
            public string StatusNote { get; set; }
            public List<string> Capabilities { get; set; } = new List<string>();
            public List<string> Gaps { get; set; } = new List<string>();
        }

        public class ProbeBundle
        {
            public List<ToolProbeResult> Tools { get; set; } = new List<ToolProbeResult>();
            public List<string> WorkflowRecommendations { get; set; } = new List<string>();
            public List<string> CrossRefNotes { get; set; } = new List<string>();
            public string SummaryText()
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== External tool probe ===");
                foreach (var t in Tools)
                {
                    sb.AppendLine((t.Found ? "[OK] " : "[--] ") + t.DisplayName);
                    sb.AppendLine("     Path: " + (t.Path ?? "(not found)"));
                    if (!string.IsNullOrEmpty(t.VersionHint))
                        sb.AppendLine("     Version: " + t.VersionHint);
                    sb.AppendLine("     " + t.StatusNote);
                    foreach (var c in t.Capabilities)
                        sb.AppendLine("     + " + c);
                    foreach (var g in t.Gaps)
                        sb.AppendLine("     - " + g);
                    sb.AppendLine();
                }
                if (WorkflowRecommendations.Count > 0)
                {
                    sb.AppendLine("--- recommended pipelines ---");
                    foreach (var w in WorkflowRecommendations)
                        sb.AppendLine("• " + w);
                }
                return sb.ToString();
            }
        }

        public static ProbeBundle ProbeAll(Action<string> log = null)
        {
            var b = new ProbeBundle();
            void L(string m) { log?.Invoke(m); LoggerService.Log("[ToolProbe] " + m); }

            L("Probing UnrealPak...");
            b.Tools.Add(ProbeUnrealPak());
            L("Probing FModel...");
            b.Tools.Add(ProbeNamed("FModel", ConfigService.FModelPath, "FModel.exe",
                "Browse/export cooked assets from PAKs (CUE4Parse)",
                new[] { "List packages", "Export raw uasset/uexp/ubulk", "UE version profiles" },
                new[] { "Does not pack PAKs", "Set UE 4.27 profile for SB2" }));
            L("Probing UModel...");
            b.Tools.Add(ProbeNamed("UModel", ConfigService.UModelPath, "umodel.exe",
                "View/export meshes and textures",
                new[] { "Mesh/texture preview", "Batch export" },
                new[] { "Not for arbitrary property edit", "Pair with UAssetGUI for package edits" }));
            L("Probing UAssetGUI...");
            b.Tools.Add(ProbeNamed("UAssetGUI", ConfigService.UAssetGuiPath, "UAssetGUI.exe",
                "Edit cooked .uasset packages",
                new[] { "Name map / exports view", "Property edit on many types" },
                new[] { "Must match UE 4.27 serialization", "Always keep matching .uexp" }));

            // Cross-ref logic
            bool hasPak = b.Tools.Exists(t => t.Key == "UnrealPak" && t.Found);
            bool hasF = b.Tools.Exists(t => t.Key == "FModel" && t.Found);
            bool hasU = b.Tools.Exists(t => t.Key == "UAssetGUI" && t.Found);

            if (hasPak && hasF && hasU)
            {
                b.WorkflowRecommendations.Add(
                    "Full cooked override: FModel export → UAssetGUI edit (or UE 4.27 re-cook) → stage SB2/Content/... → UnrealPak -Create=response.txt -compress → z_*_P.pak");
                b.CrossRefNotes.Add("All primary tools present — analyzer can recommend complete pipelines.");
            }
            else
            {
                if (!hasPak)
                    b.WorkflowRecommendations.Add("Install UE 4.27 and set UnrealPak path — required to list the monolithic PAK and create patch packs.");
                if (!hasF)
                    b.WorkflowRecommendations.Add("Install FModel (UE 4.27) to browse/export assets without the full editor.");
                if (!hasU)
                    b.WorkflowRecommendations.Add("Install UAssetGUI (4.27) for in-place cooked package edits.");
            }

            b.WorkflowRecommendations.Add(
                "Loose-only path (no tools required beyond Explorer): replace Content/Movies/*.mp4 or Content/FMOD/*.bank with backups via Loose File Mods.");
            b.WorkflowRecommendations.Add(
                "New mesh/texture content: author in UE 4.27 editor → Cook Content for Windows → copy cooked trio into mod staging → UnrealPak.");
            b.WorkflowRecommendations.Add(
                "Localization: extract .locres from PAK (FModel or UnrealPak extract) → community locres text tools → rebuild → _P.pak under same path.");
            b.WorkflowRecommendations.Add(
                "Prefer overriding existing package paths so AssetRegistry.bin still resolves assets; brand-new packages may not be discovered by class queries.");

            if (hasPak)
            {
                var pak = b.Tools.Find(t => t.Key == "UnrealPak");
                if (pak != null && !string.IsNullOrEmpty(pak.Path) &&
                    pak.Path.IndexOf("4.27", StringComparison.OrdinalIgnoreCase) < 0 &&
                    pak.Path.IndexOf("UE_4.27", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    b.CrossRefNotes.Add("UnrealPak path does not clearly contain 4.27 — Oodle/list format mismatches are common with wrong engine builds.");
                }
            }

            return b;
        }

        private static ToolProbeResult ProbeUnrealPak()
        {
            var r = new ToolProbeResult
            {
                Key = "UnrealPak",
                DisplayName = "UnrealPak.exe (UE 4.27)",
                Role = "List PAK contents; create compressed patch PAKs",
                Path = ConfigService.UnrealPakPath
            };
            if (string.IsNullOrWhiteSpace(r.Path) || !File.Exists(r.Path))
            {
                // try auto path from last detect
                r.Found = false;
                r.StatusNote = "Not configured — Auto-Detect or Settings required for deep PAK list.";
                r.Gaps.Add("Cannot enumerate 85k+ cooked paths without UnrealPak -List");
                r.Gaps.Add("Cannot create Oodle-compatible patch PAKs reliably");
                return r;
            }
            r.Found = true;
            r.VersionHint = TryFileVersion(r.Path);
            r.StatusNote = "Present — prefer Engine build from UE 4.27 for SB2 Oodle PAKs.";
            r.Capabilities.Add("UnrealPak -List <pak>");
            r.Capabilities.Add("UnrealPak <out.pak> -Create=response.txt -compress");
            r.Capabilities.Add("Extract with -Extract (when needed)");
            if (r.Path.IndexOf("4.27", StringComparison.OrdinalIgnoreCase) >= 0)
                r.Capabilities.Add("Path suggests UE 4.27 — good match for SB2");
            else
                r.Gaps.Add("Path may not be 4.27 — verify if list/pack fails");
            return r;
        }

        private static ToolProbeResult ProbeNamed(string key, string configured, string exeName, string role,
            string[] caps, string[] gaps)
        {
            var r = new ToolProbeResult
            {
                Key = key,
                DisplayName = key,
                Role = role,
                Path = configured
            };
            if (string.IsNullOrWhiteSpace(r.Path) || !File.Exists(r.Path))
            {
                r.Found = false;
                r.StatusNote = "Not found at configured path.";
                r.Gaps.AddRange(gaps);
                r.Gaps.Insert(0, "Install and set path in Settings / Auto-Detect");
                return r;
            }
            r.Found = true;
            r.VersionHint = TryFileVersion(r.Path);
            r.StatusNote = "Present.";
            r.Capabilities.AddRange(caps);
            r.Gaps.AddRange(gaps);
            return r;
        }

        private static string TryFileVersion(string path)
        {
            try
            {
                var vi = FileVersionInfo.GetVersionInfo(path);
                string v = vi.FileVersion ?? vi.ProductVersion;
                if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
            }
            catch { }
            try
            {
                return new FileInfo(path).Length + " bytes, " + File.GetLastWriteTime(path).ToString("yyyy-MM-dd");
            }
            catch { return ""; }
        }
    }
}
