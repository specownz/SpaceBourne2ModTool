using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Deep auto-detection for SpaceBourne 2 install and external modding tools.
    /// Searches Steam libraries (registry + libraryfolders.vdf), Epic, common drive roots,
    /// Program Files, user folders, and shallow recursive filename hits.
    /// </summary>
    public static class ToolAutoDetector
    {
        public const int SteamAppId = 1646850;
        public const string GameFolderName = "SpaceBourne 2";

        public class DetectResult
        {
            public string GameInstallPath { get; set; }
            public string UnrealPakPath { get; set; }
            public string FModelPath { get; set; }
            public string UModelPath { get; set; }
            public string UAssetGuiPath { get; set; }
            public string CheatEnginePath { get; set; }
            public string CodeEditorPath { get; set; }
            public string CodeEditorName { get; set; }
            public string ImageEditorPath { get; set; }
            public string ImageEditorName { get; set; }
            public string ModelingPath { get; set; }
            public string ModelingName { get; set; }
            public List<string> SteamLibraryRoots { get; set; } = new List<string>();
            public List<string> Log { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();

            public string SummaryText()
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== Auto-detect results ===");
                sb.AppendLine("Game:       " + (GameInstallPath ?? "(not found)"));
                sb.AppendLine("UnrealPak:  " + (UnrealPakPath ?? "(not found)"));
                sb.AppendLine("FModel:     " + (FModelPath ?? "(not found)"));
                sb.AppendLine("UModel:     " + (UModelPath ?? "(not found)"));
                sb.AppendLine("UAssetGUI:  " + (UAssetGuiPath ?? "(not found)"));
                sb.AppendLine("Cheat Engine: " + (CheatEnginePath ?? "(not found)"));
                sb.AppendLine("Code editor: " + (CodeEditorPath ?? "(not found)") + (string.IsNullOrEmpty(CodeEditorName) ? "" : " [" + CodeEditorName + "]"));
                sb.AppendLine("Image editor: " + (ImageEditorPath ?? "(not found)") + (string.IsNullOrEmpty(ImageEditorName) ? "" : " [" + ImageEditorName + "]"));
                sb.AppendLine("3D modeling: " + (ModelingPath ?? "(not found)") + (string.IsNullOrEmpty(ModelingName) ? "" : " [" + ModelingName + "]"));
                sb.AppendLine();
                sb.AppendLine("Steam libraries scanned: " + SteamLibraryRoots.Count);
                foreach (var l in SteamLibraryRoots)
                    sb.AppendLine("  " + l);
                if (Log.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("--- log ---");
                    foreach (var line in Log.Take(40))
                        sb.AppendLine(line);
                }
                if (Warnings.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("--- warnings ---");
                    foreach (var w in Warnings)
                        sb.AppendLine(w);
                }
                return sb.ToString();
            }
        }

        /// <param name="progress">Status text updates.</param>
        /// <param name="itemProgress">Per-target updates: (itemKey, found, detailPathOrMessage).</param>
        public static DetectResult DetectAll(Action<string> progress = null, Action<string, bool, string> itemProgress = null)
        {
            var r = new DetectResult();
            void P(string msg)
            {
                r.Log.Add(msg);
                progress?.Invoke(msg);
                LoggerService.Log("[AutoDetect] " + msg);
            }
            void Item(string key, bool found, string detail)
            {
                try { itemProgress?.Invoke(key, found, detail ?? ""); } catch { }
            }

            Item("SteamLibraries", false, "Scanning...");
            P("Collecting Steam library folders...");
            r.SteamLibraryRoots = GetSteamLibraryRoots(P);
            Item("SteamLibraries", r.SteamLibraryRoots.Count > 0, r.SteamLibraryRoots.Count + " libraries");
            P("Steam libraries: " + r.SteamLibraryRoots.Count);

            Item("Game", false, "Searching...");
            P("Searching for SpaceBourne 2...");
            r.GameInstallPath = FindSpaceBourne2(r.SteamLibraryRoots, P);
            if (r.GameInstallPath != null)
                P("Game found: " + r.GameInstallPath);
            else
            {
                P("Game not found via Steam — trying broad search...");
                r.GameInstallPath = BroadFindGameFolder(P);
            }
            Item("Game", !string.IsNullOrEmpty(r.GameInstallPath), r.GameInstallPath ?? "not found");

            Item("UnrealPak", false, "Searching...");
            P("Searching for UnrealPak.exe (prefer UE 4.27)...");
            r.UnrealPakPath = FindUnrealPak(r.GameInstallPath, P);
            Item("UnrealPak", !string.IsNullOrEmpty(r.UnrealPakPath), r.UnrealPakPath ?? "not found");

            Item("FModel", false, "Searching...");
            P("Searching for FModel.exe...");
            r.FModelPath = FindNamedExe("FModel.exe", PreferUserToolsRoots(), P, maxHits: 12);
            Item("FModel", !string.IsNullOrEmpty(r.FModelPath), r.FModelPath ?? "not found");

            Item("UModel", false, "Searching...");
            P("Searching for umodel.exe / UE Viewer...");
            r.UModelPath = FindUModel(P);
            Item("UModel", !string.IsNullOrEmpty(r.UModelPath), r.UModelPath ?? "not found");

            Item("UAssetGUI", false, "Searching...");
            P("Searching for UAssetGUI.exe...");
            r.UAssetGuiPath = FindNamedExe("UAssetGUI.exe", PreferUserToolsRoots(), P, maxHits: 10);
            Item("UAssetGUI", !string.IsNullOrEmpty(r.UAssetGuiPath), r.UAssetGuiPath ?? "not found");

            Item("CheatEngine", false, "Searching...");
            P("Searching for Cheat Engine...");
            r.CheatEnginePath = FindCheatEngine(P);
            Item("CheatEngine", !string.IsNullOrEmpty(r.CheatEnginePath), r.CheatEnginePath ?? "not found");

            // Validate game path has expected structure
            if (r.GameInstallPath != null && !LooksLikeSb2Install(r.GameInstallPath))
            {
                r.Warnings.Add("Candidate game path missing SB2/Content/Paks — may be incomplete: " + r.GameInstallPath);
            }

            P("Searching code / image / 3D authoring tools (paid preferred=" + ConfigService.PreferPaidTools + ")...");
            try
            {
                var auth = AuthoringToolDetector.Detect(P, itemProgress);
                r.CodeEditorPath = auth.CodeEditorPath;
                r.CodeEditorName = auth.CodeEditorName;
                r.ImageEditorPath = auth.ImageEditorPath;
                r.ImageEditorName = auth.ImageEditorName;
                r.ModelingPath = auth.ModelingPath;
                r.ModelingName = auth.ModelingName;
            }
            catch (Exception ex)
            {
                P("Authoring tool detect error: " + ex.Message);
            }

            P("Auto-detect finished.");
            return r;
        }

        public static void ApplyToConfig(DetectResult r)
        {
            if (!string.IsNullOrEmpty(r.GameInstallPath) && Directory.Exists(r.GameInstallPath))
                ConfigService.SetGamePath(r.GameInstallPath);
            if (!string.IsNullOrEmpty(r.UnrealPakPath) && File.Exists(r.UnrealPakPath))
                ConfigService.UnrealPakPath = r.UnrealPakPath;
            if (!string.IsNullOrEmpty(r.FModelPath) && File.Exists(r.FModelPath))
                ConfigService.FModelPath = r.FModelPath;
            if (!string.IsNullOrEmpty(r.UModelPath) && File.Exists(r.UModelPath))
                ConfigService.UModelPath = r.UModelPath;
            if (!string.IsNullOrEmpty(r.UAssetGuiPath) && File.Exists(r.UAssetGuiPath))
                ConfigService.UAssetGuiPath = r.UAssetGuiPath;
            if (!string.IsNullOrEmpty(r.CodeEditorPath) && File.Exists(r.CodeEditorPath))
                ConfigService.CodeEditorPath = r.CodeEditorPath;
            if (!string.IsNullOrEmpty(r.ImageEditorPath) && File.Exists(r.ImageEditorPath))
                ConfigService.ImageEditorPath = r.ImageEditorPath;
            if (!string.IsNullOrEmpty(r.ModelingPath) && File.Exists(r.ModelingPath))
                ConfigService.ModelingToolPath = r.ModelingPath;
            ConfigService.Save();
        }

        public static bool LooksLikeSb2Install(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;
            string paks = Path.Combine(path, "SB2", "Content", "Paks");
            string bin = Path.Combine(path, "SB2", "Binaries", "Win64", "SB2-Win64-Shipping.exe");
            return Directory.Exists(paks) || File.Exists(bin);
        }

        // ---------- Steam ----------

        public static List<string> GetSteamLibraryRoots(Action<string> log = null)
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Registry: Steam install path
            foreach (string regPath in new[]
            {
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam",
            })
            {
                try
                {
                    object v = Registry.GetValue(regPath, "SteamPath", null)
                               ?? Registry.GetValue(regPath, "InstallPath", null);
                    if (v != null)
                    {
                        string steam = v.ToString().Replace('/', '\\');
                        if (Directory.Exists(steam))
                        {
                            roots.Add(steam);
                            log?.Invoke("Registry Steam: " + steam);
                        }
                    }
                }
                catch { }
            }

            // Common default locations
            foreach (string drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).Select(d => d.Name))
            {
                foreach (string rel in new[]
                {
                    @"Program Files (x86)\Steam",
                    @"Program Files\Steam",
                    @"Steam",
                    @"SteamLibrary",
                    @"Games\Steam",
                    @"Epic Games", // not steam but useful later
                })
                {
                    string p = Path.Combine(drive, rel);
                    if (Directory.Exists(p))
                        roots.Add(p);
                }
            }

            // Parse libraryfolders.vdf for additional library paths
            var vdfCandidates = new List<string>();
            foreach (string steamRoot in roots.ToList())
            {
                vdfCandidates.Add(Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf"));
                vdfCandidates.Add(Path.Combine(steamRoot, "config", "libraryfolders.vdf"));
            }

            foreach (string vdf in vdfCandidates.Distinct())
            {
                if (!File.Exists(vdf)) continue;
                try
                {
                    string text = File.ReadAllText(vdf);
                    // "path"		"D:\\SteamLibrary"
                    foreach (Match m in Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase))
                    {
                        string lib = m.Groups[1].Value.Replace("\\\\", "\\");
                        if (Directory.Exists(lib))
                        {
                            roots.Add(lib);
                            log?.Invoke("VDF library: " + lib);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log?.Invoke("VDF parse failed: " + vdf + " — " + ex.Message);
                }
            }

            return roots.OrderBy(x => x).ToList();
        }

        public static string FindSpaceBourne2(IEnumerable<string> steamRoots, Action<string> log = null)
        {
            // 1) steamapps/appmanifest_1646850.acf → installdir
            foreach (string root in steamRoots)
            {
                string apps = Path.Combine(root, "steamapps");
                if (!Directory.Exists(apps)) continue;

                string acf = Path.Combine(apps, "appmanifest_" + SteamAppId + ".acf");
                if (File.Exists(acf))
                {
                    try
                    {
                        string text = File.ReadAllText(acf);
                        var m = Regex.Match(text, "\"installdir\"\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string dir = Path.Combine(apps, "common", m.Groups[1].Value);
                            if (LooksLikeSb2Install(dir))
                            {
                                log?.Invoke("Found via appmanifest: " + dir);
                                return dir;
                            }
                        }
                    }
                    catch { }
                }

                // 2) steamapps/common/SpaceBourne 2
                string common = Path.Combine(apps, "common", GameFolderName);
                if (LooksLikeSb2Install(common))
                {
                    log?.Invoke("Found via common folder: " + common);
                    return common;
                }

                // 3) any folder under common containing SB2-Win64-Shipping.exe
                string commonRoot = Path.Combine(apps, "common");
                if (Directory.Exists(commonRoot))
                {
                    try
                    {
                    foreach (string dir in SafeDirectory.GetDirectories(commonRoot))
                        {
                            if (LooksLikeSb2Install(dir))
                            {
                                log?.Invoke("Found via SB2 structure under common: " + dir);
                                return dir;
                            }
                        }
                    }
                    catch { }
                }
            }
            return null;
        }

        private static string BroadFindGameFolder(Action<string> log)
        {
            var roots = new List<string>();
            foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady && x.DriveType == DriveType.Fixed))
            {
                roots.Add(d.Name);
                roots.Add(Path.Combine(d.Name, "Games"));
                roots.Add(Path.Combine(d.Name, "SteamLibrary"));
            }
            roots.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            roots.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            foreach (string root in roots.Where(Directory.Exists))
            {
                try
                {
                    // Shallow: root/*/SpaceBourne 2 or root/SpaceBourne 2
                    string direct = Path.Combine(root, GameFolderName);
                    if (LooksLikeSb2Install(direct)) return direct;

                    foreach (string dir in SafeDirectory.GetDirectories(root))
                    {
                        string name = Path.GetFileName(dir);
                        if (name.IndexOf("SpaceBourne", StringComparison.OrdinalIgnoreCase) >= 0 && LooksLikeSb2Install(dir))
                            return dir;
                        string nested = Path.Combine(dir, GameFolderName);
                        if (LooksLikeSb2Install(nested)) return nested;
                        // steamapps/common pattern one level deeper
                        string common = Path.Combine(dir, "steamapps", "common", GameFolderName);
                        if (LooksLikeSb2Install(common)) return common;
                    }
                }
                catch (Exception ex)
                {
                    log?.Invoke("Broad search skip " + root + ": " + ex.Message);
                }
            }
            return null;
        }

        // ---------- Tools ----------

        private static List<string> PreferUserToolsRoots()
        {
            var list = new List<string>();
            string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string downloads = Path.Combine(user, "Downloads");

            list.AddRange(new[] { user, local, roaming, pf, pf86, desktop, docs, downloads });
            list.Add(Path.Combine(user, "Tools"));
            list.Add(Path.Combine(user, "Modding"));
            list.Add(Path.Combine(docs, "Tools"));
            list.Add(@"C:\Tools");
            list.Add(@"D:\Tools");
            list.Add(@"C:\Modding");
            list.Add(@"C:\UE");
            list.Add(@"D:\UE");
            list.Add(@"C:\Program Files\Epic Games");
            list.Add(@"C:\Program Files (x86)\Epic Games");

            foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady && x.DriveType == DriveType.Fixed))
            {
                list.Add(d.Name);
                list.Add(Path.Combine(d.Name, "Tools"));
                list.Add(Path.Combine(d.Name, "Games"));
                list.Add(Path.Combine(d.Name, "UE_4.27"));
                list.Add(Path.Combine(d.Name, "UnrealEngine"));
            }
            return list.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static string FindUnrealPak(string gameInstall, Action<string> log = null)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrEmpty(ConfigService.UnrealPakPath) && File.Exists(ConfigService.UnrealPakPath))
                return ConfigService.UnrealPakPath;

            if (!string.IsNullOrEmpty(gameInstall))
            {
                candidates.Add(Path.Combine(gameInstall, "Engine", "Binaries", "Win64", "UnrealPak.exe"));
                candidates.Add(Path.Combine(gameInstall, "SB2", "Binaries", "Win64", "UnrealPak.exe"));
            }

            // Explicit UE 4.27-ish paths first
            foreach (string root in PreferUserToolsRoots())
            {
                candidates.Add(Path.Combine(root, "UE_4.27", "Engine", "Binaries", "Win64", "UnrealPak.exe"));
                candidates.Add(Path.Combine(root, "UE_4.27", "Engine", "Binaries", "Win64", "UnrealPak-Win64-Shipping.exe"));
                candidates.Add(Path.Combine(root, "4.27", "Engine", "Binaries", "Win64", "UnrealPak.exe"));
                candidates.Add(Path.Combine(root, "Engine", "Binaries", "Win64", "UnrealPak.exe"));
            }

            foreach (string c in candidates)
            {
                if (File.Exists(c))
                {
                    log?.Invoke("UnrealPak candidate hit: " + c);
                    return c;
                }
            }

            // Deep-ish search for UnrealPak.exe, prefer path containing 4.27
            var found = SearchFilesNamed("UnrealPak.exe", PreferUserToolsRoots(), maxDepth: 6, maxHits: 15, log);
            string prefer427 = found.FirstOrDefault(f => f.IndexOf("4.27", StringComparison.OrdinalIgnoreCase) >= 0);
            if (prefer427 != null) return prefer427;
            return found.FirstOrDefault();
        }

        private static string FindUModel(Action<string> log)
        {
            if (!string.IsNullOrEmpty(ConfigService.UModelPath) && File.Exists(ConfigService.UModelPath))
                return ConfigService.UModelPath;
            foreach (string name in new[] { "umodel.exe", "umodel_64.exe", "UE Viewer.exe" })
            {
                string hit = FindNamedExe(name, PreferUserToolsRoots(), log, maxHits: 8);
                if (hit != null) return hit;
            }
            return null;
        }

        private static string FindCheatEngine(Action<string> log)
        {
            var roots = PreferUserToolsRoots();
            foreach (string name in new[] { "cheatengine-x86_64.exe", "cheatengine.exe", "Cheat Engine.exe" })
            {
                string hit = FindNamedExe(name, roots, log, maxHits: 6);
                if (hit != null) return hit;
            }
            // Program Files\Cheat Engine*
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            foreach (string root in new[] { pf86, pf })
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    foreach (string dir in SafeDirectory.GetDirectories(root))
                    {
                        if (dir.IndexOf("Cheat Engine", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        foreach (string exe in SafeDirectory.GetFiles(dir, "cheatengine-x86_64.exe"))
                            return exe;
                        foreach (string exe in SafeDirectory.GetFiles(dir, "cheatengine.exe"))
                            return exe;
                    }
                }
                catch { }
            }
            return null;
        }

        private static string FindNamedExe(string fileName, List<string> roots, Action<string> log, int maxHits = 10)
        {
            var hits = SearchFilesNamed(fileName, roots, maxDepth: 5, maxHits: maxHits, log);
            return hits.FirstOrDefault();
        }

        /// <summary>
        /// Bounded-depth recursive search for a file name under roots.
        /// Skips heavy/system folders to stay responsive.
        /// </summary>
        public static List<string> SearchFilesNamed(string fileName, IEnumerable<string> roots, int maxDepth, int maxHits, Action<string> log = null)
        {
            var results = new List<string>();
            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Windows", "WinSxS", "$Recycle.Bin", "System Volume Information",
                "node_modules", ".git", "AppData\\Local\\Temp", "Temp",
                "WindowsApps", "Package Cache"
            };

            foreach (string root in roots)
            {
                if (results.Count >= maxHits) break;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;
                try
                {
                    SearchRecursive(root, fileName, 0, maxDepth, results, maxHits, skip, log);
                }
                catch (Exception ex)
                {
                    log?.Invoke("Search error under " + root + ": " + ex.Message);
                }
            }
            return results;
        }

        private static void SearchRecursive(string dir, string fileName, int depth, int maxDepth,
            List<string> results, int maxHits, HashSet<string> skip, Action<string> log)
        {
            if (results.Count >= maxHits || depth > maxDepth) return;
            string leaf = Path.GetFileName(dir);
            if (skip.Contains(leaf)) return;

            try
            {
                foreach (string f in SafeDirectory.GetFiles(dir, fileName))
                {
                    results.Add(f);
                    log?.Invoke("Found " + fileName + ": " + f);
                    if (results.Count >= maxHits) return;
                }
            }
            catch { /* pattern or path rejected */ }

            if (depth >= maxDepth) return;
            foreach (string sub in SafeDirectory.GetDirectories(dir))
            {
                if (results.Count >= maxHits) return;
                SearchRecursive(sub, fileName, depth + 1, maxDepth, results, maxHits, skip, log);
            }
        }
    }
}
