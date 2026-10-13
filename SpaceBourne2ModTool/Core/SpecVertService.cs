using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// specVert: builds a *parallel modular workspace* from analysis of a cooked UE 4.27 game.
    /// Does NOT rewrite or recompile the shipping binary. Safety-first: never mutates base PAKs.
    ///
    /// What is possible without game source:
    /// - Organize extracted/listed assets into Modules (Armor, Characters, Dialogs, Quests, Ships, …)
    /// - Value override documents for intended numeric/string changes (apply later via UAssetGUI / cook)
    /// - Keep pace with updates by re-running Analyzer → re-specVert (diff by path lists)
    ///
    /// What is NOT possible here:
    /// - Full game source recovery or engine recompile of SB2-Win64-Shipping.exe
    /// - True Blueprint graph reconstruction from cooked bytecode alone
    /// </summary>
    public static class SpecVertService
    {
        public const string ManifestName = "SpecVert.json";
        public const string FlagKey = "SpecVerted";

        public static readonly string[] ModuleIds =
        {
            "Armor", "Characters", "Dialogs", "Quests", "Ships", "Weapons",
            "Items", "Cinematics", "Audio", "Movies", "Config", "Meshes", "Materials", "Widgets", "Locomotion", "Packes", "Galaxy", "Vfx", "Generic"
        };

        public class Result
        {
            public string WorkspaceRoot { get; set; }
            public int ModulesCreated { get; set; }
            public int PathNotes { get; set; }
            public List<string> Log { get; set; } = new List<string>();
        }

        public static string DefaultWorkspaceRoot()
        {
            string baseDir = ConfigService.ModsOutputPath ??
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpaceBourne2Mods");
            return Path.Combine(baseDir, "SpecVertWorkspace");
        }

        /// <summary>Create or refresh modular workspace from analyzer LATEST reports. Read-only w.r.t. game install.</summary>
        public static Result BuildWorkspace(string reportsDir, string workspaceRoot = null, Action<string> progress = null)
        {
            var r = new Result();
            void L(string m) { r.Log.Add(m); progress?.Invoke(m); }

            workspaceRoot = string.IsNullOrWhiteSpace(workspaceRoot) ? DefaultWorkspaceRoot() : workspaceRoot;
            r.WorkspaceRoot = workspaceRoot;
            Directory.CreateDirectory(workspaceRoot);
            L("specVert workspace: " + workspaceRoot);
            L("NOTE: This does not modify game PAKs or recompile the shipping executable.");

            string modules = Path.Combine(workspaceRoot, "Modules");
            Directory.CreateDirectory(modules);

            var pathBuckets = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in ModuleIds) pathBuckets[id] = new List<string>();

            if (!string.IsNullOrEmpty(reportsDir) && Directory.Exists(reportsDir))
            {
                foreach (string name in new[] { "SB2_ContentMap_LATEST.txt", "SB2_AuthorFeedback_LATEST.txt", "SB2_ModTargets_LATEST.txt", "SB2_Formats_LATEST.txt" })
                {
                    string f = Path.Combine(reportsDir, name);
                    if (!File.Exists(f)) continue;
                    L("Reading " + name);
                    int n = 0;
                    try
                    {
                        foreach (string raw in File.ReadLines(f))
                        {
                            if (++n > 250000) break;
                            string line = raw.Trim();
                            string path = ExtractContentPath(line);
                            if (path == null) continue;
                            string mod = Classify(path);
                            var list = pathBuckets[mod];
                            if (list.Count < 400 && !list.Any(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase)))
                                list.Add(path);
                        }
                    }
                    catch (Exception ex) { L("WARN " + name + ": " + ex.Message); }
                }
            }
            else L("No reports dir — creating empty module scaffolds only.");

            foreach (string id in ModuleIds)
            {
                string dir = Path.Combine(modules, id);
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(Path.Combine(dir, "Values"));
                Directory.CreateDirectory(Path.Combine(dir, "Staging"));
                Directory.CreateDirectory(Path.Combine(dir, "Exports"));

                var paths = pathBuckets[id];
                File.WriteAllLines(Path.Combine(dir, "TARGET_PATHS.txt"), paths, Encoding.UTF8);
                File.WriteAllText(Path.Combine(dir, "README.txt"),
                    "specVert module: " + id + "\r\n" +
                    "Paths listed in TARGET_PATHS.txt come from Game Analyzer reports.\r\n" +
                    "1) Export matching packages with FModel (UE 4.27).\r\n" +
                    "2) Place cooked .uasset+.uexp(+.ubulk) into Staging/ preserving relative layout under SB2/Content/.\r\n" +
                    "3) Edit Values/*.json for intended property overrides (apply in UAssetGUI).\r\n" +
                    "4) Pack Staging via UnrealPak / Speckage — never overwrite base WindowsNoEditor.pak.\r\n",
                    Encoding.UTF8);

                // Seed a sample values document for common numeric tuning categories
                if (id == "Armor" || id == "Weapons" || id == "Ships" || id == "Items")
                {
                    string sample = Path.Combine(dir, "Values", "Sample_" + id + "_Overrides.json");
                    if (!File.Exists(sample))
                    {
                        File.WriteAllText(sample,
                            "{\n  \"Module\": \"" + id + "\",\n  \"Note\": \"Document intended value overrides. Apply to cooked assets with UAssetGUI 4.27.\",\n" +
                            "  \"Properties\": {\n    \"Health\": \"\",\n    \"Damage\": \"\",\n    \"Armor\": \"\",\n    \"Speed\": \"\",\n    \"Cost\": \"\"\n  }\n}\n",
                            Encoding.UTF8);
                    }
                }

                r.ModulesCreated++;
                r.PathNotes += paths.Count;
                L("Module " + id + " — " + paths.Count + " sample paths");
            }

            // Root manifest
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"Format\": \"specVert_v1\",");
            sb.AppendLine("  \"CreatedUtc\": \"" + DateTime.UtcNow.ToString("o") + "\",");
            sb.AppendLine("  \"GameId\": \"1646850\",");
            sb.AppendLine("  \"ReportsDir\": \"" + JsonLite.Esc(reportsDir ?? "") + "\",");
            sb.AppendLine("  \"Safety\": \"Read-only w.r.t. game install. No base PAK mutation. No shipping EXE recompile.\",");
            sb.AppendLine("  \"Modules\": " + r.ModulesCreated + ",");
            sb.AppendLine("  \"PathNotes\": " + r.PathNotes);
            sb.AppendLine("}");
            File.WriteAllText(Path.Combine(workspaceRoot, ManifestName), sb.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(workspaceRoot, "SAFETY.txt"),
                "specVert SAFETY\r\n" +
                "- Does not modify SpaceBourne 2 installation files by default.\r\n" +
                "- Cannot recover full C++/Blueprint source from cooked builds.\r\n" +
                "- Value JSON files are documentation for UAssetGUI / re-cook workflows.\r\n" +
                "- After game updates: re-run Analyzer, then re-run specVert to refresh TARGET_PATHS.\r\n" +
                "- Inject only z_*_P.pak via Mod Loader.\r\n", Encoding.UTF8);

            L("specVert complete.");
            return r;
        }

        public static void MarkModSpecVerted(string modRoot, bool enabled, string workspaceRoot = null)
        {
            if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot)) return;
            string flag = Path.Combine(modRoot, "SPECVERTED.flag");
            if (enabled)
            {
                File.WriteAllText(flag,
                    "SpecVerted=true\r\nWorkspace=" + (workspaceRoot ?? DefaultWorkspaceRoot()) + "\r\n" +
                    "Created=" + DateTime.UtcNow.ToString("o") + "\r\n", Encoding.UTF8);
                // Link note
                string link = Path.Combine(modRoot, "SpecVert_README.txt");
                File.WriteAllText(link,
                    "This mod is marked SpecVerted.\r\n" +
                    "Use Modules under the SpecVert workspace for organized exports/values.\r\n" +
                    "Copy from Modules/<name>/Staging into this mod's SB2/Content when packing.\r\n", Encoding.UTF8);
            }
            else if (File.Exists(flag)) File.Delete(flag);
        }

        public static bool IsModSpecVerted(string modRoot)
        {
            return !string.IsNullOrEmpty(modRoot) && File.Exists(Path.Combine(modRoot, "SPECVERTED.flag"));
        }

        static string Classify(string path)
        {
            string p = path.Replace('\\', '/');
            if (p.IndexOf("Majorcharacters", StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("/Armors/", StringComparison.OrdinalIgnoreCase) >= 0) return "Armor";
            if (p.IndexOf("1_New_sets", StringComparison.OrdinalIgnoreCase) >= 0) return "Armor";
            if (p.IndexOf("/Dialogs/", StringComparison.OrdinalIgnoreCase) >= 0) return "Dialogs";
            if (p.IndexOf("/Quests/", StringComparison.OrdinalIgnoreCase) >= 0) return "Quests";
            if (p.IndexOf("111Cinematic", StringComparison.OrdinalIgnoreCase) >= 0) return "Cinematics";
            if (p.IndexOf("/FMOD/", StringComparison.OrdinalIgnoreCase) >= 0) return "Audio";
            if (p.IndexOf("/Movies/", StringComparison.OrdinalIgnoreCase) >= 0) return "Movies";
            if (p.IndexOf("/Config/", StringComparison.OrdinalIgnoreCase) >= 0 || p.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) return "Config";
            if (p.IndexOf("1Galaxy", StringComparison.OrdinalIgnoreCase) >= 0 || p.IndexOf("Ship", StringComparison.OrdinalIgnoreCase) >= 0) return "Ships";
            if (p.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0 || p.IndexOf("Ballistic", StringComparison.OrdinalIgnoreCase) >= 0) return "Weapons";
            if (p.IndexOf("1Meshs", StringComparison.OrdinalIgnoreCase) >= 0 || p.IndexOf("/Mesh", StringComparison.OrdinalIgnoreCase) >= 0) return "Meshes";
            if (p.IndexOf("Material", StringComparison.OrdinalIgnoreCase) >= 0) return "Materials";
            if (p.IndexOf("1Widgets", StringComparison.OrdinalIgnoreCase) >= 0) return "Widgets";
            if (p.IndexOf("Locomotion", StringComparison.OrdinalIgnoreCase) >= 0) return "Locomotion";
            if (p.IndexOf("Packes", StringComparison.OrdinalIgnoreCase) >= 0) return "Packes";
            if (p.IndexOf("Ballistics", StringComparison.OrdinalIgnoreCase) >= 0 || p.IndexOf("VFX", StringComparison.OrdinalIgnoreCase) >= 0) return "Vfx";
            if (p.IndexOf("1Data", StringComparison.OrdinalIgnoreCase) >= 0) return "Items";
            if (p.IndexOf("Character", StringComparison.OrdinalIgnoreCase) >= 0) return "Characters";
            return "Generic";
        }

        static string ExtractContentPath(string line)
        {
            int i = line.IndexOf("SB2/Content/", StringComparison.OrdinalIgnoreCase);
            if (i < 0) i = line.IndexOf("1Data/", StringComparison.OrdinalIgnoreCase);
            if (i < 0) i = line.IndexOf("Armors/", StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int end = i;
            while (end < line.Length)
            {
                char c = line[end];
                if (char.IsLetterOrDigit(c) || c == '/' || c == '_' || c == '-' || c == '.' || c == ' ') end++;
                else break;
            }
            string p = line.Substring(i, end - i).Trim();
            if (p.EndsWith(".uexp") || p.EndsWith(".ubulk")) return null;
            return p;
        }
    }
}
