using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;
using SpecFramework;

namespace SpecFramework
{
    public class ModLoadEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string PackagePath { get; set; }
        public string ExtractedPath { get; set; }
        public bool Enabled { get; set; }
        public int LoadOrder { get; set; }
        public List<string> DeployedFiles { get; set; } = new List<string>();
        public string Notes { get; set; }
    }

    public class ModLoaderState
    {
        public List<ModLoadEntry> Mods { get; set; } = new List<ModLoadEntry>();
        public DateTime? LastInjectUtc { get; set; }
        public string LastBackupFolder { get; set; }
    }

    /// <summary>
    /// Ordered mod loader for SpaceBourne 2.
    /// Safety: only manages files it deployed; never touches vanilla base PAKs;
    /// backups before inject; PathGuard on all writes under Content/Paks (and optional loose).
    /// </summary>
    public class ModLoaderService
    {
        private readonly string _libraryDir;
        private readonly string _statePath;

        public ModLoaderService()
        {
            _libraryDir = Path.Combine(
                ConfigService.ModsOutputPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpaceBourne2Mods"),
                "ModLibrary");
            Directory.CreateDirectory(_libraryDir);
            _statePath = Path.Combine(_libraryDir, "loader_state.json");
        }

        public string LibraryDirectory { get { return _libraryDir; } }

        public ModLoaderState LoadState()
        {
            var st = new ModLoaderState();
            try
            {
                if (!File.Exists(_statePath)) return st;
                // Line format: ID|Name|Version|Author|PackagePath|ExtractedPath|Enabled|LoadOrder|DeployedComma
                foreach (string line in File.ReadAllLines(_statePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("BACKUP="))
                    {
                        st.LastBackupFolder = line.Substring(7);
                        continue;
                    }
                    string[] p = line.Split('|');
                    if (p.Length < 8) continue;
                    var m = new ModLoadEntry
                    {
                        Id = p[0],
                        Name = p[1],
                        Version = p[2],
                        Author = p[3],
                        PackagePath = p[4],
                        ExtractedPath = p[5],
                        Enabled = p[6] == "1",
                        LoadOrder = 0
                    };
                    int.TryParse(p[7], out int lo);
                    m.LoadOrder = lo;
                    if (p.Length >= 9 && !string.IsNullOrEmpty(p[8]))
                        m.DeployedFiles = new System.Collections.Generic.List<string>(p[8].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    st.Mods.Add(m);
                }
            }
            catch { }
            return st;
        }

        public void SaveState(ModLoaderState state)
        {
            Directory.CreateDirectory(_libraryDir);
            var sb = new StringBuilder();
            sb.AppendLine("# specSB2modTool ModLoader state");
            if (!string.IsNullOrEmpty(state.LastBackupFolder))
                sb.AppendLine("BACKUP=" + state.LastBackupFolder);
            foreach (var m in state.Mods)
            {
                string dep = m.DeployedFiles != null ? string.Join(",", m.DeployedFiles.ToArray()) : "";
                sb.Append(m.Id).Append('|')
                  .Append(m.Name).Append('|')
                  .Append(m.Version).Append('|')
                  .Append(m.Author).Append('|')
                  .Append(m.PackagePath).Append('|')
                  .Append(m.ExtractedPath).Append('|')
                  .Append(m.Enabled ? "1" : "0").Append('|')
                  .Append(m.LoadOrder).Append('|')
                  .Append(dep)
                  .AppendLine();
            }
            File.WriteAllText(_statePath, sb.ToString(), Encoding.UTF8);
        }

        public ModLoadEntry ImportPackage(string speckagePath)
        {
            if (!File.Exists(speckagePath))
                throw new FileNotFoundException("Package not found", speckagePath);

            var man = SpeckagePack.ReadManifest(speckagePath);
            string id = PathGuard.SanitizeFileName(man.Name + "_" + man.Version);
            string dest = Path.Combine(_libraryDir, id);
            if (Directory.Exists(dest))
            {
                try { Directory.Delete(dest, true); } catch { }
            }
            Directory.CreateDirectory(dest);

            // Copy package into library
            string pkgCopy = Path.Combine(dest, Path.GetFileName(speckagePath));
            File.Copy(speckagePath, pkgCopy, true);
            string contentDir = Path.Combine(dest, "content");
            SpeckagePack.ExtractToFolder(pkgCopy, contentDir);

            var state = LoadState();
            state.Mods.RemoveAll(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
            var entry = new ModLoadEntry
            {
                Id = id,
                Name = man.Name,
                Version = man.Version,
                Author = man.Author,
                PackagePath = pkgCopy,
                ExtractedPath = contentDir,
                Enabled = true,
                LoadOrder = man.DefaultLoadOrder,
                Notes = man.Description
            };
            // Discover pak files in extract
            if (Directory.Exists(contentDir))
            {
                foreach (string pak in Directory.GetFiles(contentDir, "*.pak", SearchOption.AllDirectories))
                {
                    string fn = Path.GetFileName(pak);
                    if (!PathGuard.IsProtectedVanillaPak(fn))
                        entry.DeployedFiles.Add(fn);
                }
            }
            if (man.DeployedPakFiles != null)
            {
                foreach (var fn in man.DeployedPakFiles)
                    if (!entry.DeployedFiles.Contains(fn) && !PathGuard.IsProtectedVanillaPak(fn))
                        entry.DeployedFiles.Add(fn);
            }
            state.Mods.Add(entry);
            SaveState(state);
            return entry;
        }

        /// <summary>
        /// Inject enabled mods into game Content/Paks by load order.
        /// Returns log lines. Does not start the game.
        /// </summary>
        public List<string> InjectEnabledMods(out string backupFolder)
        {
            var log = new List<string>();
            backupFolder = null;
            if (!ConfigService.Game.IsValid || string.IsNullOrEmpty(ConfigService.Game.PaksPath))
            {
                log.Add("ERROR: Game path / Paks folder not configured.");
                return log;
            }
            string paks = ConfigService.Game.PaksPath;
            string reason;
            string resolved;
            if (!PathGuard.TryResolveUnderRoot(paks, ".", out resolved, out reason))
            {
                // root itself
            }

            var state = LoadState();
            // Backup registry folder
            backupFolder = Path.Combine(_libraryDir, "Backups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(backupFolder);
            state.LastBackupFolder = backupFolder;
            log.Add("Backup folder: " + backupFolder);

            // Remove previously deployed mod files that we tracked (safe uninstall before re-inject)
            foreach (var mod in state.Mods)
            {
                foreach (string fn in mod.DeployedFiles.ToList())
                {
                    if (PathGuard.IsVanillaEditBlocked(fn)) continue;
                    string target = Path.Combine(paks, fn);
                    if (File.Exists(target))
                    {
                        try
                        {
                            string bak = Path.Combine(backupFolder, fn);
                            File.Copy(target, bak, true);
                            // Only delete if this file was from our loader (name matches z_* or tracked)
                            if (IsLoaderManagedName(fn))
                            {
                                File.Delete(target);
                                log.Add("Removed previous: " + fn);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Add("WARN remove " + fn + ": " + ex.Message);
                        }
                    }
                }
            }

            // Inject enabled mods sorted by load order then name
            foreach (var mod in state.Mods.Where(m => m.Enabled).OrderBy(m => m.LoadOrder).ThenBy(m => m.Name))
            {
                if (string.IsNullOrEmpty(mod.ExtractedPath) || !Directory.Exists(mod.ExtractedPath))
                {
                    log.Add("SKIP " + mod.Name + " — extracted content missing");
                    continue;
                }
                log.Add("Inject order " + mod.LoadOrder + ": " + mod.Name);

                // Prefer .pak files — copy to Paks with order-prefixed name for sort control
                var paksFound = Directory.GetFiles(mod.ExtractedPath, "*.pak", SearchOption.AllDirectories);
                if (paksFound.Length == 0)
                {
                    log.Add("  No .pak in package — stage cooked files only (user must pack) or use loose deploy.");
                    continue;
                }
                mod.DeployedFiles.Clear();
                int idx = 0;
                foreach (string srcPak in paksFound)
                {
                    string originalName = Path.GetFileName(srcPak);
                    if (PathGuard.IsVanillaEditBlocked(originalName))
                    {
                        log.Add("  BLOCKED protected name: " + originalName);
                        continue;
                    }
                    // Ensure _P and z_ prefix for priority; include order for stable sort
                    string deployName = BuildDeployPakName(mod.LoadOrder, idx, mod.Name, originalName);
                    string dest = Path.Combine(paks, deployName);
                    string rsn;
                    string full;
                    if (!PathGuard.TryResolveUnderRoot(paks, deployName, out full, out rsn))
                    {
                        log.Add("  BLOCKED path: " + rsn);
                        continue;
                    }
                    try
                    {
                        File.Copy(srcPak, dest, true);
                        mod.DeployedFiles.Add(deployName);
                        log.Add("  Deployed: " + deployName);
                    }
                    catch (Exception ex)
                    {
                        log.Add("  ERROR copy: " + ex.Message);
                    }
                    idx++;
                }
            }

            state.LastInjectUtc = DateTime.UtcNow;
            SaveState(state);
            log.Add("Inject complete.");
            return log;
        }

        public List<string> UnloadAllManaged()
        {
            var log = new List<string>();
            if (!ConfigService.Game.IsValid) { log.Add("Game not configured"); return log; }
            string paks = ConfigService.Game.PaksPath;
            var state = LoadState();
            foreach (var mod in state.Mods)
            {
                foreach (string fn in mod.DeployedFiles.ToList())
                {
                    if (PathGuard.IsVanillaEditBlocked(fn)) continue;
                    string target = Path.Combine(paks, fn);
                    if (File.Exists(target) && IsLoaderManagedName(fn))
                    {
                        try { File.Delete(target); log.Add("Removed " + fn); }
                        catch (Exception ex) { log.Add("WARN " + ex.Message); }
                    }
                }
                mod.DeployedFiles.Clear();
            }
            SaveState(state);
            return log;
        }

        public bool StartGame(out string message)
        {
            message = "";
            try
            {
                string bin = ConfigService.Game.BinaryPath;
                if (!string.IsNullOrEmpty(bin) && File.Exists(bin))
                {
                    Process.Start(new ProcessStartInfo { FileName = bin, UseShellExecute = true });
                    message = "Started: " + bin;
                    return true;
                }
                // Steam
                Process.Start(new ProcessStartInfo
                {
                    FileName = "steam://run/1646850",
                    UseShellExecute = true
                });
                message = "Launched via Steam AppID 1646850";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool IsLoaderManagedName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            // Our deploy pattern: z_ord####_*.pak or existing z_*_P.pak tracked
            string n = Path.GetFileName(fileName);
            if (PathGuard.IsVanillaEditBlocked(n)) return false;
            if (n.StartsWith("z_", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.IndexOf("_P.pak", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public static string BuildDeployPakName(int order, int index, string modName, string originalName)
        {
            string safe = PathGuard.SanitizeFileName(modName).Replace(" ", "_");
            string ord = order.ToString("D4");
            // Prefer reshaping existing z_*_P names with order prefix
            if (originalName.StartsWith("z_", StringComparison.OrdinalIgnoreCase) &&
                originalName.IndexOf("_P", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string rest = originalName;
                if (rest.StartsWith("z_", StringComparison.OrdinalIgnoreCase))
                    rest = rest.Substring(2);
                return "z_" + ord + "_" + index + "_" + rest;
            }
            string baseName = Path.GetFileNameWithoutExtension(originalName);
            if (string.IsNullOrEmpty(baseName)) baseName = safe;
            if (!baseName.EndsWith("_P", StringComparison.OrdinalIgnoreCase))
                baseName = baseName + "_P";
            return "z_" + ord + "_" + index + "_" + baseName + ".pak";
        }
    }
}
