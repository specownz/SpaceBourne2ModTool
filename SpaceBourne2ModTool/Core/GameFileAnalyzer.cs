using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Long-running safe analyzer for SpaceBourne 2.
    ///
    /// Hang fix: UnrealPak -List previously deadlocked because stdout was read
    /// while stderr filled its pipe buffer. Both streams are drained; process
    /// is killable via CancellationToken; progress is reported frequently.
    ///
    /// Deep dive classifies paths into textures, materials, meshes, blueprints,
    /// audio, localization, config, movies, FX — what a modder needs to know.
    /// </summary>
    public class GameFileAnalyzer
    {
        public event Action<AnalysisProgress> ProgressChanged;
        public event Action<string> LogLine;

        // Tunables
        /// <summary>0 = unlimited (cancel + timeouts still apply). Prefer unlimited for full maps.</summary>
        public int MaxListLines { get; set; } = 0;
        public int ProgressEveryNLines { get; set; } = 1000;
        public int ProcessIdleTimeoutMs { get; set; } = 120000; // no output for 2 min → kill
        public int HardTimeoutMs { get; set; } = 3 * 60 * 60 * 1000; // 3 hours hard cap per pak (cancel still works)

        private void Log(string msg)
        {
            LogLine?.Invoke(msg);
            LoggerService.Log("[Analyzer] " + msg);
        }

        private void Emit(AnalysisProgress p)
        {
            p.Utc = DateTime.UtcNow;
            ProgressChanged?.Invoke(p);
        }

        public GameAnalysisReport Analyze(string installPath, bool tryPakList, CancellationToken ct)
        {
            var report = new GameAnalysisReport
            {
                InstallPath = installPath,
                AnalyzedAtUtc = DateTime.UtcNow,
                ToolVersion = "2.2.0"
            };

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                report.PathLooksValid = false;
                report.Warnings.Add("Install path does not exist.");
                return report;
            }

            ct.ThrowIfCancellationRequested();
            Emit(new AnalysisProgress { Phase = "Install", Message = "Scanning install root...", OverallPercent = 2, FilePercent = 0 });

            try
            {
                report.TopLevelInstallEntries = Directory.GetFileSystemEntries(installPath)
                    .Select(p => Path.GetFileName(p) + (Directory.Exists(p) ? "/" : ""))
                    .OrderBy(x => x).ToList();
                foreach (var e in report.TopLevelInstallEntries)
                    Log("  entry: " + e);
            }
            catch (Exception ex)
            {
                report.Warnings.Add("Could not list install root: " + ex.Message);
            }

            string sb2 = Path.Combine(installPath, "SB2");
            if (!Directory.Exists(sb2))
                sb2 = Path.Combine(installPath, "SpaceBourne 2", "SB2");

            string content = Path.Combine(sb2, "Content");
            string paks = Path.Combine(content, "Paks");
            string binary = Path.Combine(sb2, "Binaries", "Win64", "SB2-Win64-Shipping.exe");
            if (!File.Exists(binary))
                binary = Path.Combine(installPath, "SB2", "Binaries", "Win64", "SB2-Win64-Shipping.exe");

            report.DetectedContentPath = Directory.Exists(content) ? content : null;
            report.DetectedPaksPath = Directory.Exists(paks) ? paks : null;
            report.DetectedBinaryPath = File.Exists(binary) ? binary : null;
            report.PathLooksValid = Directory.Exists(content) && Directory.Exists(paks);

            ct.ThrowIfCancellationRequested();
            Emit(new AnalysisProgress { Phase = "Tools", Message = "Probing external modding tools...", OverallPercent = 5, FilePercent = 0 });
            try
            {
                var probe = ExternalToolProbe.ProbeAll(Log);
                report.ToolProbeSummary = probe.SummaryText();
                report.ToolWorkflowRecommendations = probe.WorkflowRecommendations;
                report.ToolCrossRefNotes = probe.CrossRefNotes;
                foreach (var line in probe.SummaryText().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    Log(line);
                foreach (var n in probe.CrossRefNotes)
                    report.Suggestions.Add("[Tools] " + n);
                foreach (var w in probe.WorkflowRecommendations.Take(4))
                    report.Suggestions.Add("[Workflow] " + w);
            }
            catch (Exception ex)
            {
                report.Warnings.Add("Tool probe failed: " + ex.Message);
                Log("Tool probe error: " + ex.Message);
            }

            ct.ThrowIfCancellationRequested();
            Emit(new AnalysisProgress { Phase = "Binary", Message = "Reading shipping binary...", OverallPercent = 8, FilePercent = 0, CurrentFile = binary ?? "" });

            TryReadSteamAppId(installPath, report);
            if (File.Exists(binary))
            {
                try
                {
                    report.BinarySizeBytes = new FileInfo(binary).Length;
                    var vi = FileVersionInfo.GetVersionInfo(binary);
                    report.BinaryFileVersion = vi.FileVersion ?? "";
                    report.BinaryProductVersion = vi.ProductVersion ?? "";
                    Log("Binary: " + binary + " size=" + FileService.FormatSize(report.BinarySizeBytes)
                        + " ver=" + report.BinaryFileVersion);
                }
                catch (Exception ex)
                {
                    report.Warnings.Add("Binary version read failed: " + ex.Message);
                }
            }

            ct.ThrowIfCancellationRequested();
            Emit(new AnalysisProgress { Phase = "Paks", Message = "Inventorying PAK files...", OverallPercent = 15, FilePercent = 0 });

            if (Directory.Exists(paks))
                InventoryPaks(paks, report, ct);

            ct.ThrowIfCancellationRequested();
            Emit(new AnalysisProgress { Phase = "Content", Message = "Scanning Content folders...", OverallPercent = 25, FilePercent = 0 });

            if (Directory.Exists(content))
                ScanContentTree(content, sb2, installPath, report, ct);

            CollectNotableFiles(installPath, sb2, report);

            // Disk-side deep inventory (FMOD, Movies) — one status line per file group
            ScanLooseModdableFolders(content, report, ct);

            if (tryPakList && report.PakFiles.Count > 0)
            {
                string unrealPak = ResolveUnrealPakPath(installPath, report);
                if (!string.IsNullOrEmpty(unrealPak) && File.Exists(unrealPak))
                {
                    Log("Using UnrealPak: " + unrealPak);
                    report.ExtraMetadata = report.ExtraMetadata ?? new Dictionary<string, string>();
                    report.ExtraMetadata["unrealPakUsed"] = unrealPak;
                    Emit(new AnalysisProgress
                    {
                        Phase = "PakList",
                        Message = "Deep parsing PAK contents (safe stream drain)...",
                        OverallPercent = 35,
                        FilePercent = 0,
                        FilesTotal = Math.Min(3, report.PakFiles.Count)
                    });
                    DeepParsePakLists(unrealPak, report, ct);
                    if (report.PakListLinesParsed == 0)
                    {
                        report.Warnings.Add("UnrealPak ran but produced 0 parseable list lines. Wrong engine version of UnrealPak, or output format mismatch.");
                        report.Suggestions.Add("Use UnrealPak from a UE 4.27 Engine/Binaries/Win64 build (matching the game).");
                        SeedFallbackTargets(report);
                    }
                }
                else
                {
                    Log("UnrealPak not found — applying known layout insights only.");
                    report.Warnings.Add("UnrealPak.exe not configured and not auto-detected near the install.");
                    report.Suggestions.Add("Set UnrealPak path in Settings (UE 4.27 Engine\\Binaries\\Win64\\UnrealPak.exe) for full pak maps.");
                    report.DetectedMountPoint = report.DetectedMountPoint ?? GameInsights.KnownMountPoint;
                    report.UsesOodle = true;
                    SeedFallbackTargets(report);
                }
            }
            else
            {
                SeedFallbackTargets(report);
            }

            ct.ThrowIfCancellationRequested();
            BuildEasyModTargets(report);
            try { AnalyzerEnhancements.Enrich(report, installPath); }
            catch (Exception exEnrich)
            {
                Log("Enhancement pass warning: " + exEnrich.Message);
                if (report.Warnings != null) report.Warnings.Add("Enhancement: " + exEnrich.Message);
            }
            FillCountsFromExtensions(report);
            BuildAssetTrioHealth(report);
            BuildUe427PracticeNotes(report);
            BuildFormatGuide(report);
            BuildSuggestions(report);

            Emit(new AnalysisProgress
            {
                Phase = "Done",
                Message = "Analysis complete.",
                OverallPercent = 100,
                FilePercent = 100,
                LinesProcessed = report.PakListLinesParsed
            });
            Log("Done. Lines parsed=" + report.PakListLinesParsed + " targets=" + report.EasyModTargets.Count);
            return report;
        }

        private void InventoryPaks(string paksDir, GameAnalysisReport report, CancellationToken ct)
        {
            var files = Directory.GetFiles(paksDir, "*", SearchOption.TopDirectoryOnly);
            int i = 0;
            foreach (string f in files)
            {
                ct.ThrowIfCancellationRequested();
                i++;
                string name = Path.GetFileName(f);
                string ext = Path.GetExtension(f).ToLowerInvariant();
                Emit(new AnalysisProgress
                {
                    Phase = "Paks",
                    Message = "PAK file " + i + "/" + files.Length + ": " + name,
                    CurrentFile = name,
                    OverallPercent = 15 + (int)(10.0 * i / Math.Max(1, files.Length)),
                    FilePercent = 100,
                    FilesDone = i,
                    FilesTotal = files.Length
                });
                Log("  PAK: " + name);

                if (ext == ".pak")
                {
                    var fi = new FileInfo(f);
                    bool endsP = name.EndsWith("_P.pak", StringComparison.OrdinalIgnoreCase)
                                 || name.EndsWith("_p.pak", StringComparison.OrdinalIgnoreCase);
                    bool vanilla = name.Equals(GameInsights.KnownMainPakName, StringComparison.OrdinalIgnoreCase)
                                   || (Regex.IsMatch(name, @"pakchunk\d+", RegexOptions.IgnoreCase) && !endsP);
                    bool mod = endsP || name.StartsWith("z_", StringComparison.OrdinalIgnoreCase)
                               || name.StartsWith("zzz", StringComparison.OrdinalIgnoreCase);

                    report.PakFiles.Add(new PakFileInfo
                    {
                        FileName = name,
                        FullPath = f,
                        SizeBytes = fi.Length,
                        SizeHuman = FileService.FormatSize(fi.Length),
                        LastWriteUtc = fi.LastWriteTimeUtc,
                        NameEndsWith_P = endsP,
                        LooksLikeVanilla = vanilla,
                        LooksLikeMod = mod && !vanilla,
                        Notes = vanilla ? "Vanilla main package" : (endsP ? "Patch (_P)" : "")
                    });
                    report.TotalPakBytes += fi.Length;
                }
                else if (ext == ".utoc" || ext == ".ucas")
                {
                    report.IoStoreFiles.Add(name);
                    report.HasIoStore = true;
                }
            }

            report.PakFiles = report.PakFiles.OrderBy(p => p.FileName, StringComparer.OrdinalIgnoreCase).ToList();
            report.TotalPakCount = report.PakFiles.Count;
            report.UsesSingleMonolithicPak = report.TotalPakCount == 1
                && report.PakFiles.Any(p => p.FileName.Equals(GameInsights.KnownMainPakName, StringComparison.OrdinalIgnoreCase));
        }

        private void ScanContentTree(string content, string sb2, string install, GameAnalysisReport report, CancellationToken ct)
        {
            try
            {
                var dirs = Directory.GetDirectories(content);
                int i = 0;
                foreach (string d in dirs)
                {
                    ct.ThrowIfCancellationRequested();
                    i++;
                    string name = Path.GetFileName(d);
                    report.ContentSubfolders.Add(name);
                    Emit(new AnalysisProgress
                    {
                        Phase = "Content",
                        Message = "Content folder: " + name,
                        CurrentFile = name,
                        OverallPercent = 25 + (int)(5.0 * i / Math.Max(1, dirs.Length)),
                        FilePercent = 100,
                        FilesDone = i,
                        FilesTotal = dirs.Length
                    });
                    Log("  Content/" + name);
                }

                if (Directory.GetFiles(content, "*", SearchOption.TopDirectoryOnly).Length > 0)
                    report.HasLooseContentBeyondPaks = true;

                string locRoot = Path.Combine(content, "Localization", "Game");
                if (Directory.Exists(locRoot))
                {
                    report.LocalizationLanguages = Directory.GetDirectories(locRoot)
                        .Select(Path.GetFileName).OrderBy(x => x).ToList();
                    foreach (var lang in report.LocalizationLanguages)
                        Log("  Localization language: " + lang);
                }

                string configDir = Path.Combine(sb2, "Saved", "Config");
                if (Directory.Exists(configDir))
                {
                    report.ConfigFiles = Directory.GetFiles(configDir, "*.ini", SearchOption.AllDirectories)
                        .Select(f => MakeRelative(f, install)).Take(40).ToList();
                    foreach (var c in report.ConfigFiles)
                        Log("  Config: " + c);
                }
            }
            catch (Exception ex)
            {
                report.Warnings.Add("Content scan issue: " + ex.Message);
            }
        }

        private void ScanLooseModdableFolders(string content, GameAnalysisReport report, CancellationToken ct)
        {
            if (!Directory.Exists(content)) return;

            report.ExtraMetadata = report.ExtraMetadata ?? new Dictionary<string, string>();
            string[] interesting = { "FMOD", "Movies", "Splash", "Localization" };
            var looseInventory = new StringBuilder();
            looseInventory.AppendLine("LOOSE_FILES_ON_DISK (outside main pak — easiest mods)");
            looseInventory.AppendLine("Generated: " + DateTime.UtcNow.ToString("o"));
            looseInventory.AppendLine();

            foreach (string folder in interesting)
            {
                ct.ThrowIfCancellationRequested();
                string path = Path.Combine(content, folder);
                if (!Directory.Exists(path))
                {
                    Log("  loose folder missing: " + folder);
                    continue;
                }

                Emit(new AnalysisProgress
                {
                    Phase = "LooseFiles",
                    Message = "Scanning loose folder: " + folder,
                    CurrentFile = folder,
                    OverallPercent = 32,
                    FilePercent = 0
                });

                try
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    var byExt = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    long totalBytes = 0;
                    int n = 0;
                    foreach (string f in files)
                    {
                        ct.ThrowIfCancellationRequested();
                        n++;
                        var fi = new FileInfo(f);
                        totalBytes += fi.Length;
                        string rel = f.Substring(content.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Replace('\\', '/');
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        if (string.IsNullOrEmpty(ext)) ext = "(none)";
                        Inc(report.ExtensionCounts, ext);
                        if (!byExt.ContainsKey(ext)) byExt[ext] = new List<string>();
                        if (byExt[ext].Count < 500)
                            byExt[ext].Add(rel + "\t" + fi.Length);

                        if (n <= 50 || n % 25 == 0)
                        {
                            Emit(new AnalysisProgress
                            {
                                Phase = "LooseFiles",
                                Message = folder + ": " + Path.GetFileName(f),
                                CurrentFile = rel,
                                OverallPercent = 32,
                                FilePercent = Math.Min(100, (int)(100.0 * n / Math.Max(1, files.Length))),
                                FilesDone = n,
                                FilesTotal = files.Length
                            });
                            Log("  loose: " + rel + " (" + FileService.FormatSize(fi.Length) + ")");
                        }
                    }

                    Log("  " + folder + " total files: " + files.Length + " size=" + FileService.FormatSize(totalBytes));
                    if (files.Length > 0)
                        report.HasLooseContentBeyondPaks = true;

                    report.ExtraMetadata["loose:" + folder + ":count"] = files.Length.ToString();
                    report.ExtraMetadata["loose:" + folder + ":bytes"] = totalBytes.ToString();

                    looseInventory.AppendLine("== " + folder + " (" + files.Length + " files, " + FileService.FormatSize(totalBytes) + ") ==");
                    foreach (var kv in byExt.OrderByDescending(x => x.Value.Count))
                    {
                        looseInventory.AppendLine("  ext " + kv.Key + " x" + kv.Value.Count);
                        report.ExtraMetadata["loose:" + folder + ":ext:" + kv.Key] = kv.Value.Count.ToString();
                        // Store sample names for Movies/FMOD (full lists capped)
                        int take = Math.Min(kv.Value.Count, folder.Equals("Movies", StringComparison.OrdinalIgnoreCase) ? 200 : 100);
                        report.ExtraMetadata["looseFiles:" + folder + ":" + kv.Key] =
                            string.Join(" || ", kv.Value.Take(take).Select(s => s.Split('\t')[0]));
                        foreach (var line in kv.Value.Take(take))
                            looseInventory.AppendLine("    " + line.Replace('\t', ' '));
                    }
                    looseInventory.AppendLine();

                    if (folder.Equals("Movies", StringComparison.OrdinalIgnoreCase) && files.Length > 0)
                    {
                        report.EasyModTargets.Add("LOOSE MOVIES: " + files.Length + " files on disk under Content/Movies — replace same filename for cutscene/trailer mods (no PAK needed).");
                        report.Suggestions.Add("Use Loose File Mods tool to swap Content/Movies/*.mp4 (backup first).");
                    }
                    if (folder.Equals("FMOD", StringComparison.OrdinalIgnoreCase) && files.Length > 0)
                    {
                        report.ExtraMetadata["fmod:layout"] = "Content/FMOD/Desktop/Master.bank + Master.strings.bank — FMOD Studio bank format; replace whole bank or rebuild in FMOD Studio matching event paths.";
                        report.EasyModTargets.Add("LOOSE FMOD: " + files.Length + " files on disk under Content/FMOD — bank/file replace may work without packing.");
                        report.Suggestions.Add("Use Loose File Mods tool for Content/FMOD replacements.");
                    }
                }
                catch (Exception ex)
                {
                    report.Warnings.Add("Loose scan " + folder + ": " + ex.Message);
                }
            }

            // Stash inventory text for WriteReports
            report.ExtraMetadata["looseInventoryText"] = looseInventory.ToString();
        }

        /// <summary>
        /// Safe UnrealPak -List: drains stdout+stderr, supports cancel, timeouts, frequent progress.
        /// </summary>
        private void DeepParsePakLists(string unrealPak, GameAnalysisReport report, CancellationToken ct)
        {
            var toSample = report.PakFiles
                .OrderByDescending(p => p.LooksLikeVanilla)
                .ThenByDescending(p => p.SizeBytes)
                .Take(3)
                .ToList();

            var extCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var prefixCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var prefix3Counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var compCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var sampleSb2 = new List<string>();
            var sampleEngine = new List<string>();
            var locres = new List<string>();
            var configs = new List<string>();
            var textures = new List<string>();
            var materials = new List<string>();
            var meshes = new List<string>();
            var blueprints = new List<string>();
            var sampleLines = new List<string>();
            var contentFolderCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var samplesByFolder = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int totalLines = 0;

            var entryRx = new Regex(
                @"LogPakFile:\s*Display:\s*""([^""]+)""\s*offset:\s*\d+,\s*size:\s*\d+\s*bytes,.*?compression:\s*([A-Za-z0-9]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var mountRx = new Regex(@"Mount point\s+(\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            int fileIndex = 0;
            foreach (var pak in toSample)
            {
                ct.ThrowIfCancellationRequested();
                fileIndex++;
                Log("Starting PAK list: " + pak.FileName + " (" + pak.SizeHuman + ")");
                Emit(new AnalysisProgress
                {
                    Phase = "PakList",
                    Message = "Listing " + pak.FileName,
                    CurrentFile = pak.FileName,
                    OverallPercent = 35 + (int)(55.0 * (fileIndex - 1) / Math.Max(1, toSample.Count)),
                    FilePercent = 0,
                    FilesDone = fileIndex - 1,
                    FilesTotal = toSample.Count
                });

                int linesThisPak = 0;
                var localSamples = new List<string>();

                try
                {
                    RunUnrealPakList(unrealPak, pak.FullPath, ct, line =>
                    {
                        linesThisPak++;
                        totalLines++;
                        if (localSamples.Count < 150)
                            localSamples.Add(line);

                        var mm = mountRx.Match(line);
                        if (mm.Success)
                            report.DetectedMountPoint = mm.Groups[1].Value.Trim();

                        var m = entryRx.Match(line);
                        if (m.Success)
                        {
                            string path = m.Groups[1].Value.Replace('\\', '/');
                            string comp = m.Groups[2].Value;
                            Inc(compCounts, comp);
                            if (comp.Equals("Oodle", StringComparison.OrdinalIgnoreCase))
                                report.UsesOodle = true;

                            string ext = Path.GetExtension(path);
                            if (string.IsNullOrEmpty(ext)) ext = "(none)";
                            Inc(extCounts, ext);
                            Inc(prefixCounts, GetPathPrefix(path, 2));
                            Inc(prefix3Counts, GetPathPrefix(path, 3));

                            string cat = ClassifyAssetPath(path);
                            Inc(categoryCounts, cat);

                            // SB2/Content/<Folder>/... hierarchy for mod scaffolding
                            if (path.StartsWith("SB2/Content/", StringComparison.OrdinalIgnoreCase))
                            {
                                string rest = path.Substring("SB2/Content/".Length);
                                int slash = rest.IndexOf('/');
                                string folder = slash > 0 ? rest.Substring(0, slash) : rest;
                                if (!string.IsNullOrEmpty(folder))
                                {
                                    Inc(contentFolderCounts, folder);
                                    if (!samplesByFolder.ContainsKey(folder))
                                        samplesByFolder[folder] = new List<string>();
                                    if (samplesByFolder[folder].Count < 40)
                                        samplesByFolder[folder].Add(path);

                                    // depth-2 Folder/Sub + depth-3 Folder/Sub/Child (full counts for character names etc.)
                                    if (slash > 0)
                                    {
                                        int slash2 = rest.IndexOf('/', slash + 1);
                                        string sub = slash2 > 0 ? rest.Substring(0, slash2) : rest;
                                        string subKey = "sub:" + sub;
                                        Inc(contentFolderCounts, subKey);
                                        if (!samplesByFolder.ContainsKey(subKey))
                                            samplesByFolder[subKey] = new List<string>();
                                        if (samplesByFolder[subKey].Count < 40)
                                            AddDiversifiedSample(samplesByFolder[subKey], path, sub);

                                        if (slash2 > 0)
                                        {
                                            int slash3 = rest.IndexOf('/', slash2 + 1);
                                            string d3 = slash3 > 0 ? rest.Substring(0, slash3) : rest;
                                            string d3Key = "d3:" + d3;
                                            Inc(contentFolderCounts, d3Key);
                                            if (!samplesByFolder.ContainsKey(d3Key))
                                                samplesByFolder[d3Key] = new List<string>();
                                            if (samplesByFolder[d3Key].Count < 12)
                                                samplesByFolder[d3Key].Add(path);
                                        }
                                    }
                                }
                            }

                            CollectSamples(path, cat, sampleSb2, sampleEngine, locres, configs, textures, materials, meshes, blueprints);
                        }

                        if (linesThisPak % ProgressEveryNLines == 0)
                        {
                            // Unknown total when unlimited — pulse file % by log scale of lines
                            int filePct;
                            if (MaxListLines > 0)
                                filePct = Math.Min(99, (int)(100.0 * linesThisPak / MaxListLines));
                            else
                                filePct = Math.Min(99, 10 + (int)(80.0 * Math.Log10(Math.Max(10, linesThisPak)) / 6.0));
                            int overall = 35 + (int)(55.0 * ((fileIndex - 1) + filePct / 100.0) / Math.Max(1, toSample.Count));
                            Emit(new AnalysisProgress
                            {
                                Phase = "PakList",
                                Message = pak.FileName + " — " + linesThisPak + " lines (unlimited)",
                                CurrentFile = pak.FileName,
                                OverallPercent = Math.Min(94, overall),
                                FilePercent = filePct,
                                LinesProcessed = totalLines,
                                FilesDone = fileIndex - 1,
                                FilesTotal = toSample.Count
                            });
                            if (linesThisPak % (ProgressEveryNLines * 5) == 0)
                                Log("  " + pak.FileName + ": " + linesThisPak + " lines (total " + totalLines + ")");
                        }

                        // Soft optional cap only if MaxListLines > 0
                        if (MaxListLines > 0 && linesThisPak >= MaxListLines)
                            return false;
                        return true;
                    });
                }
                catch (OperationCanceledException)
                {
                    Log("Cancelled during " + pak.FileName);
                    throw;
                }
                catch (Exception ex)
                {
                    report.Warnings.Add("PAK list failed for " + pak.FileName + ": " + ex.Message);
                    Log("ERROR listing " + pak.FileName + ": " + ex.Message);
                }

                report.PakListSamples[pak.FileName] = localSamples.Take(120).ToList();
                Log("Finished " + pak.FileName + " lines=" + linesThisPak);
                Emit(new AnalysisProgress
                {
                    Phase = "PakList",
                    Message = "Finished " + pak.FileName,
                    CurrentFile = pak.FileName,
                    OverallPercent = 35 + (int)(55.0 * fileIndex / Math.Max(1, toSample.Count)),
                    FilePercent = 100,
                    LinesProcessed = totalLines,
                    FilesDone = fileIndex,
                    FilesTotal = toSample.Count
                });
            }

            report.PakListLinesParsed = totalLines;
            report.CompressionCounts = SortDict(compCounts);
            report.ExtensionCounts = SortDict(extCounts);
            report.PathPrefixCounts = SortDict(prefixCounts);
            // Store depth-3 under path prefixes too (merged top)
            foreach (var kv in prefix3Counts.OrderByDescending(x => x.Value).Take(40))
            {
                if (!report.PathPrefixCounts.ContainsKey(kv.Key))
                    report.PathPrefixCounts[kv.Key] = kv.Value;
            }
            report.SampleGameAssetPaths = sampleSb2;
            report.SampleEnginePaths = sampleEngine;
            report.LocresPathsFound = locres;
            report.ConfigPathsInPak = configs;
            report.UmapPathsSampled = sampleSb2.Where(x => x.EndsWith(".umap", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            if (report.UmapPathsSampled.Count == 0 && sampleEngine != null)
                report.UmapPathsSampled = sampleEngine.Where(x => x.EndsWith(".umap", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            report.PluginPathsSampled = sampleSb2.Concat(sampleEngine ?? Enumerable.Empty<string>())
                .Where(x => x.EndsWith(".uplugin", StringComparison.OrdinalIgnoreCase) || x.IndexOf("/Plugins/", StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(40).ToList();

            // Category breakdown into easy targets + samples
            report.ExtraMetadata = report.ExtraMetadata ?? new Dictionary<string, string>();
            foreach (var kv in categoryCounts.OrderByDescending(x => x.Value))
                report.ExtraMetadata["cat:" + kv.Key] = kv.Value.ToString();

            foreach (var kv in contentFolderCounts.OrderByDescending(x => x.Value))
                report.ExtraMetadata["folder:" + kv.Key] = kv.Value.ToString();

            foreach (var kv in samplesByFolder)
                report.ExtraMetadata["folderSamples:" + kv.Key] = string.Join(" | ", kv.Value.Take(10));

            // Promote depth-3 major characters & armor sets into easy mod targets
            foreach (var kv in contentFolderCounts.Where(x => x.Key.StartsWith("d3:Armors/Majorcharacters/", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Value).Take(20))
            {
                string name = kv.Key.Substring("d3:Armors/Majorcharacters/".Length);
                report.EasyModTargets.Add("MAJORCHAR " + name + " (~" + kv.Value + ") — stage Armors/Majorcharacters/" + name);
            }
            // Quest depth-3 folders
            foreach (var kv in contentFolderCounts.Where(x => x.Key.StartsWith("d3:1Data/Quests/", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Value).Take(12))
            {
                string name = kv.Key.Substring("d3:1Data/Quests/".Length);
                if (name.IndexOf('/') >= 0) continue;
                report.EasyModTargets.Add("QUEST " + name + " (~" + kv.Value + ") — stage 1Data/Quests/" + name);
            }
            // Cinematic / DialogCam
            foreach (var kv in contentFolderCounts.Where(x => x.Key.StartsWith("d3:111Cinematic/", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Value).Take(8))
            {
                string name = kv.Key.Substring("d3:111Cinematic/".Length);
                if (name.IndexOf('/') >= 0) name = name.Split('/')[0];
                report.EasyModTargets.Add("CINEMATIC " + name + " (~" + kv.Value + ") — stage 111Cinematic/" + name);
            }
            // Config paths already sampled — surface as target
            if (report.ConfigPathsInPak != null && report.ConfigPathsInPak.Count > 0)
                report.EasyModTargets.Add("CONFIG INI (~" + report.ConfigPathsInPak.Count + " sampled) — stage SB2/Config or matching pak path");
            if (report.LocresPathsFound != null && report.LocresPathsFound.Count > 0)
                report.EasyModTargets.Add("LOCRES (~" + report.LocresPathsFound.Count + " sampled) — localization workflow");

            foreach (var kv in contentFolderCounts.Where(x => x.Key.StartsWith("d3:Armors/1_New_sets/", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Value).Take(15))
            {
                string name = kv.Key.Substring("d3:Armors/1_New_sets/".Length);
                report.EasyModTargets.Add("ARMORSET " + name + " (~" + kv.Value + ") — stage Armors/1_New_sets/" + name);
            }
            if (contentFolderCounts.ContainsKey("d3:1Data/NPCs/Dialogs"))
                report.EasyModTargets.Add("NPC DIALOGS (~" + contentFolderCounts["d3:1Data/NPCs/Dialogs"] + ") — stage 1Data/NPCs/Dialogs");

            AppendSampleList(report, "textures", textures);
            AppendSampleList(report, "materials", materials);
            AppendSampleList(report, "meshes", meshes);
            AppendSampleList(report, "blueprints", blueprints);

            // Prefer SB2-only texture/material samples for mod authors (Engine samples are less useful)
            var sb2Tex = textures.Where(x => x.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            var sb2Mat = materials.Where(x => x.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            var sb2Mesh = meshes.Where(x => x.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            var sb2Bp = blueprints.Where(x => x.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
            if (sb2Tex.Count > 0) AppendSampleList(report, "sb2_textures", sb2Tex);
            if (sb2Mat.Count > 0) AppendSampleList(report, "sb2_materials", sb2Mat);
            if (sb2Mesh.Count > 0) AppendSampleList(report, "sb2_meshes", sb2Mesh);
            if (sb2Bp.Count > 0) AppendSampleList(report, "sb2_blueprints", sb2Bp);
        }

        private void CollectSamples(string path, string cat,
            List<string> sb2, List<string> engine, List<string> locres, List<string> configs,
            List<string> textures, List<string> materials, List<string> meshes, List<string> blueprints)
        {
            if (path.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("Game/", StringComparison.OrdinalIgnoreCase))
            {
                if (sb2.Count < 300) sb2.Add(path);
            }
            else if (path.StartsWith("Engine/", StringComparison.OrdinalIgnoreCase) && engine.Count < 60)
                engine.Add(path);

            if ((path.IndexOf("Localization", StringComparison.OrdinalIgnoreCase) >= 0
                 || path.EndsWith(".locres", StringComparison.OrdinalIgnoreCase)) && locres.Count < 60)
                locres.Add(path);
            if (path.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) && configs.Count < 50)
                configs.Add(path);

            if (cat == "Texture" && textures.Count < 80) textures.Add(path);
            if (cat == "Material" && materials.Count < 80) materials.Add(path);
            if (cat == "Mesh" && meshes.Count < 80) meshes.Add(path);
            if (cat == "Blueprint" && blueprints.Count < 80) blueprints.Add(path);
        }

        private static void AppendSampleList(GameAnalysisReport report, string key, List<string> samples)
        {
            if (samples == null || samples.Count == 0) return;
            report.ExtraMetadata = report.ExtraMetadata ?? new Dictionary<string, string>();
            report.ExtraMetadata["samples:" + key] = string.Join(" | ", samples.Take(25));
        }

        /// <summary>
        /// Classify path for modding intelligence.
        /// </summary>
        public static string ClassifyAssetPath(string path)
        {
            string p = path.ToLowerInvariant();
            string ext = Path.GetExtension(p);

            if (ext == ".locres" || p.Contains("/localization/")) return "Localization";
            if (ext == ".ini" || p.Contains("/config/")) return "Config";
            if (ext == ".bank" || ext == ".wav" || ext == ".ogg" || p.Contains("/fmod/") || p.Contains("/sound") || p.Contains("/audio"))
                return "Audio";
            if (ext == ".mp4" || ext == ".bk2" || p.Contains("/movies/") || p.Contains("/cinematics"))
                return "Movie";
            if (p.Contains("/material") || p.Contains("/mi_") || p.Contains("materialinstance") || p.Contains("/mat_"))
                return "Material";
            if (p.Contains("/texture") || p.Contains("/textures") || p.Contains("/ui/") ||
                p.Contains("_diff") || p.Contains("_norm") || p.Contains("_masc") || p.Contains("t_") ||
                ext == ".png" || ext == ".tga" || ext == ".jpg" || ext == ".dds")
                return "Texture";
            if (p.Contains("/mesh") || p.Contains("/staticmesh") || p.Contains("/skeletalmesh") ||
                p.Contains("/model") || p.Contains("sm_") || p.Contains("sk_"))
                return "Mesh";
            if (p.Contains("/blueprint") || p.Contains("/blueprints") || p.Contains("/bp_") ||
                p.Contains("/game/") && ext == ".uasset")
                return "Blueprint";
            if (p.Contains("/anim") || p.Contains("/animation")) return "Animation";
            if (p.Contains("/particle") || p.Contains("/vfx") || p.Contains("/fx/") || p.Contains("/niagara"))
                return "VFX";
            if (p.Contains("/map") || p.Contains("/level") || ext == ".umap") return "Map";
            if (p.Contains("/font") || p.Contains("/slate")) return "UI";
            if (ext == ".uasset" || ext == ".uexp" || ext == ".ubulk") return "CookedAsset";
            return "Other";
        }

        /// <summary>
        /// Settings path first, then common UE 4.27 locations relative to install / Program Files.
        /// </summary>
        private string ResolveUnrealPakPath(string installPath, GameAnalysisReport report)
        {
            // Prefer deep auto-detector (Steam-aware, UE 4.27 preference)
            try
            {
                string detected = ToolAutoDetector.FindUnrealPak(installPath, msg => Log(msg));
                if (!string.IsNullOrEmpty(detected) && File.Exists(detected))
                {
                    report.Suggestions.Add("UnrealPak via auto-detect: " + detected);
                    return detected;
                }
            }
            catch (Exception ex) { Log("ToolAutoDetector.FindUnrealPak: " + ex.Message); }

            string configured = ConfigService.UnrealPakPath;
            if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
                return configured;

            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(installPath))
            {
                candidates.Add(Path.Combine(installPath, "Engine", "Binaries", "Win64", "UnrealPak.exe"));
                candidates.Add(Path.Combine(installPath, "SB2", "Binaries", "Win64", "UnrealPak.exe"));
            }
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            foreach (string root in new[] { pf, pf86, @"C:\UE", @"D:\UE", @"C:\Program Files\Epic Games" })
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;
                try
                {
                    foreach (string hit in ToolAutoDetector.SearchFilesNamed("UnrealPak.exe", new[] { root }, 5, 8, Log))
                    {
                        if (hit.IndexOf("4.27", StringComparison.OrdinalIgnoreCase) >= 0)
                            candidates.Insert(0, hit);
                        else
                            candidates.Add(hit);
                    }
                }
                catch { }
            }

            foreach (string c in candidates.Distinct())
            {
                if (File.Exists(c))
                {
                    Log("Auto-detected UnrealPak: " + c);
                    report.Suggestions.Add("Auto-detected UnrealPak at " + c + " — save it in Settings for next run.");
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Run UnrealPak -List with dual-stream drain, cancel, idle + hard timeouts.
        /// callback returns false to stop early (e.g. line cap).
        /// </summary>
        private void RunUnrealPakList(string unrealPak, string pakPath, CancellationToken ct, Func<string, bool> onLine)
        {
            var psi = new ProcessStartInfo
            {
                FileName = unrealPak,
                Arguments = "\"" + pakPath + "\" -List",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                // Avoid UI culture issues
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var proc = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                var errBag = new ConcurrentQueue<string>();
                var lastOutput = DateTime.UtcNow;
                var started = DateTime.UtcNow;
                bool stop = false;

                proc.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        errBag.Enqueue(e.Data);
                        lastOutput = DateTime.UtcNow;
                    }
                };

                if (!proc.Start())
                    throw new InvalidOperationException("Failed to start UnrealPak.");

                // Async stderr drain prevents pipe deadlock
                proc.BeginErrorReadLine();

                // Sync stdout read with cancel + timeouts
                using (ct.Register(() =>
                {
                    try { if (!proc.HasExited) proc.Kill(); } catch { }
                }))
                {
                    string line;
                    while (!stop && (line = proc.StandardOutput.ReadLine()) != null)
                    {
                        ct.ThrowIfCancellationRequested();
                        lastOutput = DateTime.UtcNow;

                        if (!onLine(line))
                        {
                            stop = true;
                            try { if (!proc.HasExited) proc.Kill(); } catch { }
                            break;
                        }

                        if ((DateTime.UtcNow - started).TotalMilliseconds > HardTimeoutMs)
                        {
                            Log("Hard timeout on UnrealPak list — killing process.");
                            try { if (!proc.HasExited) proc.Kill(); } catch { }
                            break;
                        }
                        if ((DateTime.UtcNow - lastOutput).TotalMilliseconds > ProcessIdleTimeoutMs)
                        {
                            Log("Idle timeout (no output) — killing process.");
                            try { if (!proc.HasExited) proc.Kill(); } catch { }
                            break;
                        }
                    }

                    // Drain remaining if process still running briefly
                    try
                    {
                        if (!proc.HasExited)
                        {
                            if (!proc.WaitForExit(15000))
                            {
                                try { proc.Kill(); } catch { }
                                proc.WaitForExit(5000);
                            }
                        }
                    }
                    catch { }
                }

                // Log a few stderr lines if any
                int n = 0;
                while (errBag.TryDequeue(out string el) && n++ < 5)
                    Log("  (stderr) " + el);
            }
        }

        private void SeedFallbackTargets(GameAnalysisReport report)
        {
            report.EasyModTargets.Add("PACKING: UE 4.27 UnrealPak -compress (vanilla uses Oodle).");
            report.EasyModTargets.Add("LAYOUT: Single SB2-WindowsNoEditor.pak — name mods z_*_P.pak.");
            report.EasyModTargets.Add("MOUNT: ../../../ — stage SB2/Content/... and Engine/Content/...");
            report.EasyModTargets.Add("DISK: Content/FMOD and Content/Movies outside pak — easiest loose-file mods.");
            report.EasyModTargets.Add("LOC: No loose Localization on disk; locres likely inside main pak.");
            report.EasyModTargets.Add("FORMAT: Cooked assets need .uasset + .uexp (+ .ubulk). Edit with UAssetGUI @ UE 4.27.");
            report.EasyModTargets.Add("TEXTURES: Export via FModel → edit → re-import in UE 4.27 blank project → cook → pak.");
            report.EasyModTargets.Add("MATERIALS: Same cooked pipeline; path often under *Material* / MI_ / Mat_.");
            report.EasyModTargets.Add("TRAINERS: Process SB2-Win64-Shipping.exe — CE pointer scan.");
        }

        private void BuildEasyModTargets(GameAnalysisReport report)
        {
            var t = new List<string>(report.EasyModTargets ?? new List<string>());
            if (report.UsesOodle) t.Add("PACKING: Oodle confirmed — use UE 4.27 UnrealPak -compress.");
            if (report.UsesSingleMonolithicPak)
                t.Add("LAYOUT: Monolithic SB2-WindowsNoEditor.pak — mods must be z_*_P.pak.");
            if (!string.IsNullOrEmpty(report.DetectedMountPoint))
                t.Add("MOUNT: " + report.DetectedMountPoint);

            if (report.ExtraMetadata != null)
            {
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("cat:")))
                    t.Add("CATEGORY " + kv.Key.Substring(4) + ": " + kv.Value + " entries in sampled list");
            }

            foreach (var kv in report.ExtensionCounts.Take(15))
            {
                string e = kv.Key.ToLowerInvariant();
                if (e == ".uasset" || e == ".uexp" || e == ".ubulk")
                    t.Add("COOKED: " + kv.Value + "x " + e + " — keep trio together; UAssetGUI 4.27.");
                else if (e == ".locres") t.Add("LOCRES: " + kv.Value + "x — extract/edit/repack.");
                else if (e == ".ini") t.Add("INI: " + kv.Value + "x — config override candidates.");
                else if (e == ".bank") t.Add("FMOD BANK: " + kv.Value + "x — also check Content/FMOD on disk.");
            }

            foreach (var kv in report.PathPrefixCounts.Take(20))
                t.Add("PREFIX: " + kv.Key + "/* (" + kv.Value + ")");

            if (report.SampleGameAssetPaths.Count > 0)
                t.Add("GAME EXAMPLE: " + report.SampleGameAssetPaths[0]);
            if (report.LocresPathsFound.Count > 0)
                t.Add("LOC EXAMPLE: " + report.LocresPathsFound[0]);

            t.Add("INJECTION: File-level mods via _P pak override; runtime via CE on SB2-Win64-Shipping.exe.");
            t.Add("VARIABLES: No reflection dump here — use UE4SS / CE structure tools for live UObject fields.");
            t.Add("SCRIPT DIALOGS: Cooked under 1Data/NPCs/Dialogs/<NPC>/ — Script Editor organizes by NPC; full text via FModel+UAssetGUI 4.27.");
            t.Add("SCRIPT QUESTS: 1Data/Quests/<Name>/ — document stages in Script Editor; pack path overrides.");
            t.Add("OBJECTS: Use Object Editors hub for NPC/Dialog/Quest/Character/Armor/Ship modular docs → import staging.");
            t.Add("UE427 API: AssetRegistry resolves existing paths; new packages may need registry-aware placement. Mount ../../../ .");
            t.Add("UE427 PATCH: Highest priority patch paks are z_*_P.pak after base name sort; Oodle via Engine 4.27 UnrealPak -compress.");

            report.EasyModTargets = t.Distinct().ToList();
        }

        private void BuildFormatGuide(GameAnalysisReport report)
        {
            report.ExtraMetadata = report.ExtraMetadata ?? new Dictionary<string, string>();
            report.ExtraMetadata["format:cooked"] = ".uasset + .uexp (+ optional .ubulk) — never ship only .uasset";
            report.ExtraMetadata["format:texture_pipeline"] = "FModel export → PNG/TGA edit → UE4.27 import → Cook Content for Windows → copy cooked files into mod staging → UnrealPak -compress";
            report.ExtraMetadata["format:locres"] = "QuickBMS extract → unreal_locres_texttool → edit txt → import → place under SB2/Content/Localization/Game/{lang}/";
            report.ExtraMetadata["format:audio"] = "FMOD banks under Content/FMOD may be replaceable as loose files; in-pak audio needs cooked path match";
            report.ExtraMetadata["format:movies"] = "Content/Movies on disk — try direct file swap of same name/extension";
            report.ExtraMetadata["format:pak_name"] = GameInsights.RecommendModPakFileName("YourMod", 1000);
            report.ExtraMetadata["format:mount"] = report.DetectedMountPoint ?? GameInsights.KnownMountPoint;
            report.ExtraMetadata["format:virtual_game"] = "/Game/... maps to ../../../SB2/Content/...";
            report.ExtraMetadata["format:asset_registry"] = "SB2/AssetRegistry.bin is the cooked content catalog (FAssetData). Prefer overriding existing package paths.";
            report.ExtraMetadata["format:ue4_guide"] = "See UE4_Pak_And_AssetRegistry.txt in mod projects / Catalog export.";
        }

        private void BuildSuggestions(GameAnalysisReport report)
        {
            if (!report.HasIoStore)
                report.Suggestions.Add("Classic .pak only — UnrealPak workflow is correct.");
            if (report.UsesOodle)
                report.Suggestions.Add("Oodle compression — pack with UE 4.27 UnrealPak -compress.");
            if (report.UsesSingleMonolithicPak)
                report.Suggestions.Add("Use z_*_P.pak so load order is after SB2-WindowsNoEditor.pak.");
            report.Suggestions.Add("Pak list has no line cap. Every line is counted until you cancel or the time limit is reached.");
            report.Suggestions.Add("Return the LATEST reports after a completed deep run (tool 2.2.0).");
        }

        private void CollectNotableFiles(string install, string sb2, GameAnalysisReport report)
        {
            foreach (string c in new[]
            {
                Path.Combine(install, "steam_appid.txt"),
                Path.Combine(install, "SpaceBourne2.exe"),
                Path.Combine(sb2, "Content", "Paks", GameInsights.KnownMainPakName)
            })
            {
                if (File.Exists(c)) report.NotableFiles.Add(MakeRelative(c, install));
            }
            try
            {
                foreach (string exe in ToolAutoDetector.SearchFilesNamed("SB2-Win64-Shipping.exe", new[] { install }, 4, 8, Log))
                    report.NotableFiles.Add(MakeRelative(exe, install));
            }
            catch { }
        }

        private void TryReadSteamAppId(string install, GameAnalysisReport report)
        {
            foreach (string p in new[]
            {
                Path.Combine(install, "steam_appid.txt"),
                Path.Combine(install, "SB2", "steam_appid.txt")
            })
            {
                if (!File.Exists(p)) continue;
                try { report.SteamAppId = File.ReadAllText(p).Trim(); return; } catch { }
            }
            report.SteamAppId = GameInsights.SteamAppId + " (expected; steam_appid.txt not found)";
        }

        /// <summary>
        /// Prefer samples from distinct depth-3 children so Majorcharacters/B_Old_Lady and
        /// Majorcharacters/OtherChar both appear instead of filling the list with one character.
        /// </summary>
        private static void AddDiversifiedSample(List<string> list, string path, string subPrefix)
        {
            if (list.Count >= 40) return;
            int idx = path.IndexOf(subPrefix, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                string after = path.Substring(idx + subPrefix.Length).TrimStart('/');
                int slash = after.IndexOf('/');
                string child = slash > 0 ? after.Substring(0, slash) : after;
                if (!string.IsNullOrEmpty(child))
                {
                    string marker = "/" + subPrefix.Replace("\\", "/") + "/" + child + "/";
                    // normalize path markers
                    string pathNorm = path.Replace("\\", "/");
                    int sameChild = 0;
                    foreach (var existing in list)
                    {
                        string en = existing.Replace("\\", "/");
                        if (en.IndexOf("/" + child + "/", StringComparison.OrdinalIgnoreCase) >= 0
                            && en.IndexOf(subPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                            sameChild++;
                    }
                    if (sameChild >= 3 && list.Count > 10)
                        return;
                }
            }
            list.Add(path);
        }

        private static void Inc(Dictionary<string, int> d, string key)
        {
            if (string.IsNullOrEmpty(key)) key = "(unknown)";
            if (!d.ContainsKey(key)) d[key] = 0;
            d[key]++;
        }

        private static Dictionary<string, int> SortDict(Dictionary<string, int> d)
        {
            return d.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static string GetPathPrefix(string path, int segments)
        {
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "(root)";
            return string.Join("/", parts.Take(Math.Min(segments, parts.Length)));
        }

        private static string MakeRelative(string full, string root)
        {
            try
            {
                full = Path.GetFullPath(full);
                root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    return full.Substring(root.Length).Replace('\\', '/');
            }
            catch { }
            return full;
        }


        private static void FillCountsFromExtensions(GameAnalysisReport report)
        {
            if (report.ExtensionCounts == null) return;
            int Get(string ext)
            {
                int n;
                return report.ExtensionCounts.TryGetValue(ext, out n) ? n : 0;
            }
            report.UassetCount = Get(".uasset");
            report.UexpCount = Get(".uexp");
            report.UbulkCount = Get(".ubulk");
            report.UmapCount = Get(".umap");
            report.PluginCount = Get(".uplugin");
            report.IniCount = Get(".ini");
            report.LocresCount = Get(".locres");
            if (report.AssetTrioHealthNotes == null)
                report.AssetTrioHealthNotes = new List<string>();
            int delta = Math.Abs(report.UassetCount - report.UexpCount);
            if (delta > 0)
                report.AssetTrioHealthNotes.Add("uasset/uexp count delta=" + delta + " (uasset=" + report.UassetCount + " uexp=" + report.UexpCount + ").");
            else if (report.UassetCount > 0)
                report.AssetTrioHealthNotes.Add("uasset and uexp counts match (" + report.UassetCount + ") — healthy cooked pair pattern.");
            report.AssetTrioHealthNotes.Add("ubulk=" + report.UbulkCount + " umap=" + report.UmapCount + " uplugin=" + report.PluginCount + " ini=" + report.IniCount + " locres=" + report.LocresCount);
        }

        private static void BuildAssetTrioHealth(GameAnalysisReport report)
        {
            if (report.AssetTrioHealthNotes == null)
                report.AssetTrioHealthNotes = new List<string>();
            int pairs = 0;
            if (report.SampleGameAssetPaths != null)
            {
                foreach (var p in report.SampleGameAssetPaths)
                {
                    if (p.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
                        pairs++;
                }
            }
            report.AssetTrioHealthNotes.Add("Sampled .uasset paths: " + pairs + ". Stage matching .uexp (+ .ubulk when present).");
            report.AssetTrioHealthNotes.Add("Mod PAK naming: z_<Name>_P.pak — mount ../../../ — override existing package paths.");
        }

        private static void BuildUe427PracticeNotes(GameAnalysisReport report)
        {
            if (report.Ue427PracticeNotes == null)
                report.Ue427PracticeNotes = new List<string>();
            var list = report.Ue427PracticeNotes;
            list.Add("SB2 is cooked UE 4.27 with a monolithic Oodle-compressed PAK and mount ../../../.");
            list.Add("Use UnrealPak from UE 4.27 only — wrong builds fail on Oodle or -List format.");
            list.Add("FModel/CUE4Parse: set engine version to 4.27 before opening the main PAK.");
            list.Add("UAssetGUI: select UE 4.27 serialization; keep matching .uexp with every .uasset.");
            list.Add("Patch priority: later z_*_P.pak wins for the same virtual path.");
            list.Add("AssetRegistry.bin catalogs assets — prefer overriding existing ObjectPaths.");
            list.Add("Response file: \"C:\\\\abs\\\\file.uasset\" \"../../../SB2/Content/relative/file.uasset\"");
            list.Add("Loose Movies/FMOD on disk need no PAK — use Loose File Mods with backup.");
            list.Add("Config INI under SB2/Config can be staged and packed for careful gameplay tweaks.");
            list.Add("Localization: extract .locres, edit with locres tools, repack same path.");
            list.Add("Do not casually replace GlobalShaderCache / ushaderbytecode.");
            list.Add("Marketplace pack content is normal cooked Content — path overrides still apply.");
            if (report.UsesOodle)
                list.Add("Always pack mods with -compress on UE 4.27 UnrealPak (Oodle-dominant base).");
            if (report.UsesSingleMonolithicPak)
                list.Add("Never rebuild the multi-GB base pak — ship small z_*_P.pak overrides only.");
        }

        public string WriteReports(GameAnalysisReport report, string outputDirectory)
        {
            FileService.EnsureDirectory(outputDirectory);
            var previousScan = GameUpdateDiff.Capture(outputDirectory);
            string stamp = report.AnalyzedAtUtc.ToString("yyyyMMdd_HHmmss");
            string baseName = "SB2_GameAnalysis_" + stamp;
            string jsonPath = Path.Combine(outputDirectory, baseName + ".json");
            string mdPath = Path.Combine(outputDirectory, baseName + ".md");
            string txtPath = Path.Combine(outputDirectory, baseName + "_inventory.txt");
            string targetsPath = Path.Combine(outputDirectory, baseName + "_mod_targets.txt");
            string formatsPath = Path.Combine(outputDirectory, baseName + "_formats.txt");

            File.WriteAllText(jsonPath, ToJson(report), Encoding.UTF8);
            File.WriteAllText(mdPath, ToMarkdown(report), Encoding.UTF8);
            File.WriteAllText(txtPath, ToInventoryTxt(report), Encoding.UTF8);
            File.WriteAllText(targetsPath, string.Join(Environment.NewLine, report.EasyModTargets ?? new List<string>()), Encoding.UTF8);

            var fmt = new StringBuilder();
            fmt.AppendLine("SpaceBourne 2 – Asset format & conversion guide");
            fmt.AppendLine("Generated: " + report.AnalyzedAtUtc.ToString("o"));
            fmt.AppendLine();
            if (report.ExtraMetadata != null)
            {
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("format:")))
                    fmt.AppendLine(kv.Key.Substring(7) + ": " + kv.Value);
                fmt.AppendLine();
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("cat:")))
                    fmt.AppendLine(kv.Key + " = " + kv.Value);
                fmt.AppendLine();
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("samples:")))
                    fmt.AppendLine(kv.Key + " = " + kv.Value);
            }
            File.WriteAllText(formatsPath, fmt.ToString(), Encoding.UTF8);

            File.Copy(jsonPath, Path.Combine(outputDirectory, "SB2_GameAnalysis_LATEST.json"), true);
            File.Copy(mdPath, Path.Combine(outputDirectory, "SB2_GameAnalysis_LATEST.md"), true);
            File.Copy(targetsPath, Path.Combine(outputDirectory, "SB2_ModTargets_LATEST.txt"), true);
            AnalyzerEnhancements.WriteExtraReports(report, outputDirectory);
            File.Copy(formatsPath, Path.Combine(outputDirectory, "SB2_Formats_LATEST.txt"), true);
            string ue4Guide = Path.Combine(outputDirectory, baseName + "_ue4_pak_structure.txt");
            File.WriteAllText(ue4Guide, Ue4PakKnowledge.GetGuide(), Encoding.UTF8);
            File.Copy(ue4Guide, Path.Combine(outputDirectory, "SB2_UE4_PakStructure_LATEST.txt"), true);

            if (report.ExtraMetadata != null && report.ExtraMetadata.ContainsKey("looseInventoryText"))
            {
                string loosePath = Path.Combine(outputDirectory, baseName + "_loose_files.txt");
                File.WriteAllText(loosePath, report.ExtraMetadata["looseInventoryText"], Encoding.UTF8);
                File.Copy(loosePath, Path.Combine(outputDirectory, "SB2_LooseFiles_LATEST.txt"), true);
            }

            // Extensive machine-readable maps for the mod-tool author
            string mapPath = Path.Combine(outputDirectory, baseName + "_content_map.txt");
            var mapSb = new StringBuilder();
            mapSb.AppendLine("SB2 Content Folder Map (from pak list)");
            mapSb.AppendLine("Generated: " + report.AnalyzedAtUtc.ToString("o"));
            mapSb.AppendLine("LinesParsed: " + report.PakListLinesParsed);
            mapSb.AppendLine();
            if (report.ExtraMetadata != null)
            {
                Func<string, int> parseN = s => { int n; return int.TryParse(s, out n) ? n : 0; };
                var folderKvs = report.ExtraMetadata
                    .Where(x => x.Key.StartsWith("folder:") && !x.Key.StartsWith("folderSamples:"))
                    .Select(x => new { Key = x.Key.Substring(7), Val = parseN(x.Value) })
                    .OrderByDescending(x => x.Val)
                    .ToList();

                mapSb.AppendLine("== folder counts (top-level under Content) ==");
                foreach (var kv in folderKvs.Where(x => !x.Key.StartsWith("sub:") && !x.Key.StartsWith("d3:")))
                    mapSb.AppendLine(kv.Val + "	" + kv.Key);
                mapSb.AppendLine();
                mapSb.AppendLine("== depth-2 (Folder/Sub) ==");
                foreach (var kv in folderKvs.Where(x => x.Key.StartsWith("sub:")))
                    mapSb.AppendLine(kv.Val + "	" + kv.Key);
                mapSb.AppendLine();
                mapSb.AppendLine("== depth-3 (Folder/Sub/Child — e.g. Majorcharacters/<CharacterName>) ==");
                foreach (var kv in folderKvs.Where(x => x.Key.StartsWith("d3:")))
                    mapSb.AppendLine(kv.Val + "	" + kv.Key);
                mapSb.AppendLine();
                mapSb.AppendLine("== Majorcharacters character folders ==");
                foreach (var kv in folderKvs.Where(x => x.Key.StartsWith("d3:Armors/Majorcharacters/", StringComparison.OrdinalIgnoreCase)))
                {
                    string child = kv.Key.Substring("d3:Armors/Majorcharacters/".Length);
                    mapSb.AppendLine(kv.Val + "	" + child);
                }
                mapSb.AppendLine();
                mapSb.AppendLine("== 1Data/NPCs children ==");
                foreach (var kv in folderKvs.Where(x => x.Key.StartsWith("d3:1Data/NPCs/", StringComparison.OrdinalIgnoreCase)))
                {
                    string child = kv.Key.Substring("d3:1Data/NPCs/".Length);
                    mapSb.AppendLine(kv.Val + "	" + child);
                }
                mapSb.AppendLine();
                mapSb.AppendLine("== Armors/1_New_sets children ==");
                foreach (var kv in folderKvs.Where(x => x.Key.StartsWith("d3:Armors/1_New_sets/", StringComparison.OrdinalIgnoreCase)))
                {
                    string child = kv.Key.Substring("d3:Armors/1_New_sets/".Length);
                    mapSb.AppendLine(kv.Val + "	" + child);
                }
                mapSb.AppendLine();
                mapSb.AppendLine("== folder samples ==");
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("folderSamples:")))
                {
                    mapSb.AppendLine(kv.Key.Substring("folderSamples:".Length) + ":");
                    foreach (var part in kv.Value.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries))
                        mapSb.AppendLine("  " + part);
                }
                mapSb.AppendLine();
                mapSb.AppendLine("== categories ==");
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("cat:")))
                    mapSb.AppendLine(kv.Value + "	" + kv.Key.Substring(4));
                mapSb.AppendLine();
                mapSb.AppendLine("== format hints ==");
                foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith("format:")))
                    mapSb.AppendLine(kv.Key.Substring(7) + ": " + kv.Value);
                mapSb.AppendLine();
                mapSb.AppendLine("== SB2 asset samples ==");
                foreach (var key in new[] { "samples:sb2_textures", "samples:sb2_materials", "samples:sb2_meshes", "samples:sb2_blueprints", "samples:textures", "samples:materials", "samples:meshes", "samples:blueprints" })
                {
                    if (report.ExtraMetadata.ContainsKey(key))
                        mapSb.AppendLine(key + " = " + report.ExtraMetadata[key]);
                }
            }
            File.WriteAllText(mapPath, mapSb.ToString(), Encoding.UTF8);
            File.Copy(mapPath, Path.Combine(outputDirectory, "SB2_ContentMap_LATEST.txt"), true);

            // Author feedback payload — everything useful for improving the tool
            string feedbackPath = Path.Combine(outputDirectory, baseName + "_author_feedback.txt");
            var fb = new StringBuilder();
            fb.AppendLine("AUTHOR_FEEDBACK_PACKET toolVersion=" + report.ToolVersion);
            fb.AppendLine("Return this file + SB2_GameAnalysis_LATEST.json to improve mod tooling.");
            fb.AppendLine();
            fb.AppendLine("MOUNT=" + (report.DetectedMountPoint ?? ""));
            fb.AppendLine("OODLE=" + report.UsesOodle);
            fb.AppendLine("MONOLITHIC=" + report.UsesSingleMonolithicPak);
            fb.AppendLine("LINES=" + report.PakListLinesParsed);
            fb.AppendLine("PAKS=" + report.TotalPakCount + " bytes=" + report.TotalPakBytes);
            fb.AppendLine();
            fb.AppendLine("--- PREFIXES ---");
            foreach (var kv in report.PathPrefixCounts.Take(80))
                fb.AppendLine(kv.Value + "	" + kv.Key);
            fb.AppendLine("--- DEPTH3_CHARACTER_FOLDERS ---");
            if (report.ExtraMetadata != null)
            {
                foreach (var kv in report.ExtraMetadata
                    .Where(x => x.Key.StartsWith("folder:d3:Armors/Majorcharacters/"))
                    .OrderByDescending(x => { int n; return int.TryParse(x.Value, out n) ? n : 0; }))
                    fb.AppendLine(kv.Value + "	" + kv.Key.Substring("folder:d3:".Length));
                fb.AppendLine("--- DEPTH3_NPCS ---");
                foreach (var kv in report.ExtraMetadata
                    .Where(x => x.Key.StartsWith("folder:d3:1Data/NPCs/"))
                    .OrderByDescending(x => { int n; return int.TryParse(x.Value, out n) ? n : 0; }))
                    fb.AppendLine(kv.Value + "	" + kv.Key.Substring("folder:d3:".Length));
                fb.AppendLine("--- DEPTH3_ARMOR_SETS ---");
                foreach (var kv in report.ExtraMetadata
                    .Where(x => x.Key.StartsWith("folder:d3:Armors/1_New_sets/") || x.Key.StartsWith("folder:d3:Armors/Set_"))
                    .OrderByDescending(x => { int n; return int.TryParse(x.Value, out n) ? n : 0; }))
                    fb.AppendLine(kv.Value + "	" + kv.Key.Substring("folder:d3:".Length));
            }
            fb.AppendLine("--- EXTENSIONS ---");
            foreach (var kv in report.ExtensionCounts)
                fb.AppendLine(kv.Value + "	" + kv.Key);
            fb.AppendLine("--- COMPRESSION ---");
            foreach (var kv in report.CompressionCounts)
                fb.AppendLine(kv.Value + "	" + kv.Key);
            fb.AppendLine("--- GAME PATH SAMPLES ---");
            foreach (var s in report.SampleGameAssetPaths.Take(150))
                fb.AppendLine(s);
            fb.AppendLine("--- LOCRES ---");
            foreach (var s in report.LocresPathsFound) fb.AppendLine(s);
            fb.AppendLine("--- CONFIG IN PAK ---");
            foreach (var s in report.ConfigPathsInPak) fb.AppendLine(s);
            fb.AppendLine("--- TARGETS ---");
            foreach (var s in report.EasyModTargets) fb.AppendLine(s);
            fb.AppendLine("--- EXTRA ---");
            if (report.ExtraMetadata != null)
                foreach (var kv in report.ExtraMetadata)
                    fb.AppendLine(kv.Key + "=" + kv.Value);
            File.WriteAllText(feedbackPath, fb.ToString(), Encoding.UTF8);
            
            // Tool cross-reference
            var toolSb = new StringBuilder();
            toolSb.AppendLine("specSB2modTool — External Tool Cross-Reference");
            toolSb.AppendLine("Generated: " + report.AnalyzedAtUtc.ToString("o"));
            toolSb.AppendLine();
            if (!string.IsNullOrEmpty(report.ToolProbeSummary))
                toolSb.AppendLine(report.ToolProbeSummary);
            toolSb.AppendLine();
            toolSb.AppendLine("=== How tools map to SB2 mod tasks ===");
            toolSb.AppendLine("List/map PAK          → UnrealPak -List");
            toolSb.AppendLine("Browse/export asset   → FModel (UE 4.27)");
            toolSb.AppendLine("Edit cooked package   → UAssetGUI (4.27) or re-cook in Editor");
            toolSb.AppendLine("Create patch PAK      → UnrealPak -Create=response.txt -compress");
            toolSb.AppendLine("Preview mesh/texture  → UModel");
            toolSb.AppendLine("Loose movie/audio     → Loose File Mods (no UnrealPak)");
            toolSb.AppendLine("Localization          → extract locres + text tools + repack");
            toolSb.AppendLine();
            toolSb.AppendLine("=== Workflow recommendations ===");
            if (report.ToolWorkflowRecommendations != null)
                foreach (var w in report.ToolWorkflowRecommendations)
                    toolSb.AppendLine("• " + w);
            toolSb.AppendLine();
            toolSb.AppendLine("=== Cross-ref notes ===");
            if (report.ToolCrossRefNotes != null)
                foreach (var n in report.ToolCrossRefNotes)
                    toolSb.AppendLine("• " + n);
            string toolPath = Path.Combine(outputDirectory, "SB2_ToolCrossRef_" + stamp + ".txt");
            File.WriteAllText(toolPath, toolSb.ToString(), Encoding.UTF8);
            File.Copy(toolPath, Path.Combine(outputDirectory, "SB2_ToolCrossRef_LATEST.txt"), true);

            // Asset health
            var healthSb = new StringBuilder();
            healthSb.AppendLine("specSB2modTool — Cooked Asset Health / Trio Notes");
            healthSb.AppendLine("uasset=" + report.UassetCount + " uexp=" + report.UexpCount + " ubulk=" + report.UbulkCount +
                " umap=" + report.UmapCount + " uplugin=" + report.PluginCount + " ini=" + report.IniCount + " locres=" + report.LocresCount);
            healthSb.AppendLine();
            if (report.AssetTrioHealthNotes != null)
                foreach (var n in report.AssetTrioHealthNotes)
                    healthSb.AppendLine("• " + n);
            string healthPath = Path.Combine(outputDirectory, "SB2_AssetHealth_" + stamp + ".txt");
            File.WriteAllText(healthPath, healthSb.ToString(), Encoding.UTF8);
            File.Copy(healthPath, Path.Combine(outputDirectory, "SB2_AssetHealth_LATEST.txt"), true);

            // UE 4.27 practice guide (generated + static knowledge)
            var ueSb = new StringBuilder();
            ueSb.AppendLine("specSB2modTool — UE 4.27 Modding Practice Guide (SB2-focused)");
            ueSb.AppendLine("Mount: " + (report.DetectedMountPoint ?? "../../../") + " | Oodle: " + report.UsesOodle + " | Monolithic: " + report.UsesSingleMonolithicPak);
            ueSb.AppendLine();
            if (report.Ue427PracticeNotes != null)
                foreach (var n in report.Ue427PracticeNotes)
                    ueSb.AppendLine("• " + n);
            ueSb.AppendLine();
            ueSb.AppendLine("=== Community UE4.27 patterns that apply ===");
            ueSb.AppendLine("• ~mods or z_ prefix pak naming for load order control");
            ueSb.AppendLine("• Never strip .uexp when shipping .uasset overrides");
            ueSb.AppendLine("• Test one package at a time; crashes often mean version mismatch or incomplete trio");
            ueSb.AppendLine("• Keep response file UTF-8 without BOM when possible");
            ueSb.AppendLine("• For Sig/encryption games this workflow differs — SB2 remains unencrypted (no AES key)");
            ueSb.AppendLine("• Blueprint cooked assets are editable only in limited ways via UAssetGUI; logic-heavy changes prefer data assets the game already reads");
            string uePath = Path.Combine(outputDirectory, "SB2_UE427_ModdingGuide_" + stamp + ".txt");
            File.WriteAllText(uePath, ueSb.ToString(), Encoding.UTF8);
            File.Copy(uePath, Path.Combine(outputDirectory, "SB2_UE427_ModdingGuide_LATEST.txt"), true);

            File.Copy(feedbackPath, Path.Combine(outputDirectory, "SB2_AuthorFeedback_LATEST.txt"), true);

            LoggerService.Log("Reports written to " + outputDirectory);
            try
            {
                string fp = GameFingerprint.ComputeFromInstall((report.InstallPath ?? ConfigService.Game.InstallPath) ?? "");
                GameFingerprint.WriteAnalysisReport(outputDirectory, fp);
            }
            catch { }
            try { GameUpdateDiff.Emit(outputDirectory, previousScan, report); }
            catch { }
            
            try
            {
                var cook = new StringBuilder();
                cook.AppendLine("SpaceBourne 2 – Easy mod cookbook (from this scan)");
                cook.AppendLine("Generated: " + DateTime.UtcNow.ToString("o"));
                cook.AppendLine();
                cook.AppendLine("STRUCTURE");
                cook.AppendLine("  Monolithic base package archive + small z_*_P.pak overrides");
                cook.AppendLine("  Mount point: ../../../");
                cook.AppendLine("  Cooked pair rule: always ship .uasset with matching .uexp (and .ubulk if present)");
                cook.AppendLine("  Virtual /Game/... maps under SB2/Content/...");
                cook.AppendLine();
                cook.AppendLine("EASIEST TARGETS");
                cook.AppendLine("  Movies on disk under Content/Movies — same file name replace, no packing required");
                cook.AppendLine("  FMOD banks under Content/FMOD — same file name replace when loose files exist");
                cook.AppendLine("  Majorcharacters under Armors/Majorcharacters/<Name>");
                cook.AppendLine("  Armor sets under Armors/1_New_sets/M_* and F_*");
                cook.AppendLine("  Quests under 1Data/Quests/ (Obsesif, Datas, numbered starters)");
                cook.AppendLine("  NPC dialogs under 1Data/NPCs/Dialogs");
                cook.AppendLine();
                cook.AppendLine("CATEGORY COUNTS (from list)");
                if (report.ExtraMetadata != null)
                {
                    foreach (var kv in report.ExtraMetadata)
                        if (kv.Key.StartsWith("cat:", StringComparison.OrdinalIgnoreCase))
                            cook.AppendLine("  " + kv.Key.Substring(4) + " = " + kv.Value);
                }
                cook.AppendLine();
                cook.AppendLine("PIPELINE");
                cook.AppendLine("  Scan → Template or Community studio → Stage → Speckage or UnrealPak → Mod Loader");
                string cookPath = Path.Combine(outputDirectory, "SB2_EasyModCookbook_LATEST.txt");
                File.WriteAllText(cookPath, cook.ToString(), Encoding.UTF8);
            }
            catch { }

            try { CrossToolHub.NotifyAnalysisComplete(outputDirectory); } catch { }
            return outputDirectory;
        }

        private static string ToJson(GameAnalysisReport r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            Action<string, string> S = (k, v) => sb.AppendLine("  \"" + k + "\": \"" + Esc(v) + "\",");
            Action<string, long> N = (k, v) => sb.AppendLine("  \"" + k + "\": " + v + ",");
            Action<string, bool> Bb = (k, v) => sb.AppendLine("  \"" + k + "\": " + (v ? "true" : "false") + ",");
            S("toolVersion", r.ToolVersion); S("analyzedAtUtc", r.AnalyzedAtUtc.ToString("o"));
            S("installPath", r.InstallPath); Bb("pathLooksValid", r.PathLooksValid);
            S("detectedContentPath", r.DetectedContentPath); S("detectedPaksPath", r.DetectedPaksPath);
            S("detectedBinaryPath", r.DetectedBinaryPath); S("binaryFileVersion", r.BinaryFileVersion);
            S("binaryProductVersion", r.BinaryProductVersion); N("binarySizeBytes", r.BinarySizeBytes);
            S("steamAppId", r.SteamAppId); S("engineHint", r.EngineHint); S("detectedMountPoint", r.DetectedMountPoint);
            Bb("usesOodle", r.UsesOodle); Bb("usesSingleMonolithicPak", r.UsesSingleMonolithicPak);
            N("pakListLinesParsed", r.PakListLinesParsed); N("totalPakCount", r.TotalPakCount); N("totalPakBytes", r.TotalPakBytes);
            Bb("hasIoStore", r.HasIoStore); Bb("hasLooseContentBeyondPaks", r.HasLooseContentBeyondPaks);
            sb.AppendLine("  \"topLevelInstallEntries\": " + Arr(r.TopLevelInstallEntries) + ",");
            sb.AppendLine("  \"contentSubfolders\": " + Arr(r.ContentSubfolders) + ",");
            sb.AppendLine("  \"localizationLanguages\": " + Arr(r.LocalizationLanguages) + ",");
            sb.AppendLine("  \"compressionCounts\": " + Dict(r.CompressionCounts) + ",");
            sb.AppendLine("  \"extensionCounts\": " + Dict(r.ExtensionCounts) + ",");
            sb.AppendLine("  \"pathPrefixCounts\": " + Dict(r.PathPrefixCounts) + ",");
            sb.AppendLine("  \"sampleGameAssetPaths\": " + Arr(r.SampleGameAssetPaths) + ",");
            sb.AppendLine("  \"sampleEnginePaths\": " + Arr(r.SampleEnginePaths) + ",");
            sb.AppendLine("  \"locresPathsFound\": " + Arr(r.LocresPathsFound) + ",");
            sb.AppendLine("  \"configPathsInPak\": " + Arr(r.ConfigPathsInPak) + ",");
            sb.AppendLine("  \"easyModTargets\": " + Arr(r.EasyModTargets) + ",");
            sb.AppendLine("  \"extraMetadata\": " + DictStr(r.ExtraMetadata) + ",");
            sb.AppendLine("  \"pakFiles\": [");
            for (int i = 0; i < r.PakFiles.Count; i++)
            {
                var p = r.PakFiles[i];
                sb.AppendLine("    { \"fileName\": \"" + Esc(p.FileName) + "\", \"sizeBytes\": " + p.SizeBytes + ", \"sizeHuman\": \"" + Esc(p.SizeHuman) + "\", \"lastWriteUtc\": \"" + p.LastWriteUtc.ToString("o") + "\", \"nameEndsWith_P\": " + (p.NameEndsWith_P ? "true" : "false") + ", \"looksLikeVanilla\": " + (p.LooksLikeVanilla ? "true" : "false") + ", \"looksLikeMod\": " + (p.LooksLikeMod ? "true" : "false") + ", \"notes\": \"" + Esc(p.Notes) + "\" }" + (i < r.PakFiles.Count - 1 ? "," : ""));
            }
            sb.AppendLine("  ],");
            sb.AppendLine("  \"ioStoreFiles\": " + Arr(r.IoStoreFiles) + ",");
            sb.AppendLine("  \"notableFiles\": " + Arr(r.NotableFiles) + ",");
            sb.AppendLine("  \"warnings\": " + Arr(r.Warnings) + ",");
            sb.AppendLine("  \"suggestions\": " + Arr(r.Suggestions) + ",");
            sb.AppendLine("  \"pakListSamples\": {");
            var keys = r.PakListSamples.Keys.ToList();
            for (int k = 0; k < keys.Count; k++)
                sb.AppendLine("    \"" + Esc(keys[k]) + "\": " + Arr(r.PakListSamples[keys[k]]) + (k < keys.Count - 1 ? "," : ""));
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Arr(IList<string> list)
        {
            if (list == null || list.Count == 0) return "[]";
            return "[" + string.Join(", ", list.Select(s => "\"" + Esc(s) + "\"")) + "]";
        }
        private static string Dict(Dictionary<string, int> d)
        {
            if (d == null || d.Count == 0) return "{}";
            return "{" + string.Join(", ", d.Select(kv => "\"" + Esc(kv.Key) + "\": " + kv.Value)) + "}";
        }
        private static string DictStr(Dictionary<string, string> d)
        {
            if (d == null || d.Count == 0) return "{}";
            return "{" + string.Join(", ", d.Select(kv => "\"" + Esc(kv.Key) + "\": \"" + Esc(kv.Value) + "\"")) + "}";
        }

        private static string ToMarkdown(GameAnalysisReport r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# SpaceBourne 2 – Deep Game Analysis v" + r.ToolVersion);
            sb.AppendLine();
            sb.AppendLine("- **Mount:** `" + (r.DetectedMountPoint ?? "(n/a)") + "` | **Oodle:** " + r.UsesOodle + " | **Monolithic:** " + r.UsesSingleMonolithicPak);
            sb.AppendLine("- **Lines parsed:** " + r.PakListLinesParsed);
            sb.AppendLine();
            sb.AppendLine("## Compression"); foreach (var kv in r.CompressionCounts) sb.AppendLine("- " + kv.Key + ": " + kv.Value);
            sb.AppendLine();
            sb.AppendLine("## Extensions"); foreach (var kv in r.ExtensionCounts.Take(30)) sb.AppendLine("- `" + kv.Key + "`: " + kv.Value);
            sb.AppendLine();
            sb.AppendLine("## Path prefixes"); foreach (var kv in r.PathPrefixCounts.Take(40)) sb.AppendLine("- `" + kv.Key + "/`: " + kv.Value);
            sb.AppendLine();
            sb.AppendLine("## Easy mod targets"); foreach (var t in r.EasyModTargets) sb.AppendLine("- " + t);
            sb.AppendLine();
            sb.AppendLine("## Sample game paths"); foreach (var p in r.SampleGameAssetPaths.Take(50)) sb.AppendLine("- `" + p + "`");
            return sb.ToString();
        }

        private static string ToInventoryTxt(GameAnalysisReport r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PAKs: " + r.TotalPakCount + " / " + FileService.FormatSize(r.TotalPakBytes));
            sb.AppendLine("Mount: " + r.DetectedMountPoint + " Oodle: " + r.UsesOodle + " Lines: " + r.PakListLinesParsed);
            foreach (var p in r.PakFiles) sb.AppendLine(p.SizeHuman + "	" + p.FileName);
            sb.AppendLine("-- PREFIXES --"); foreach (var kv in r.PathPrefixCounts.Take(50)) sb.AppendLine(kv.Value + "	" + kv.Key);
            sb.AppendLine("-- EXTS --"); foreach (var kv in r.ExtensionCounts) sb.AppendLine(kv.Value + "	" + kv.Key);
            return sb.ToString();
        }

        private static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
