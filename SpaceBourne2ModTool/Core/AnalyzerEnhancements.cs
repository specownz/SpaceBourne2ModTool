using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Models;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// High-value analyzer extensions: deeper taxonomy, injection risk, naming patterns,
    /// deployed-mod inventory, SpecVert coverage — all derived safely from existing lists
    /// + light disk scans (never mutates game files).
    /// </summary>
    public static class AnalyzerEnhancements
    {
        // Prefix taxonomy for UE/SB2-style asset names (mod discovery)
        static readonly string[] NamePrefixes =
        {
            "BP_", "BP", "WBP_", "UI_", "UMG_", "DT_", "CT_", "Curve_", "DA_",
            "MI_", "M_", "Mat_", "T_", "TX_", "Tex_", "SK_", "SM_", "Skeletal",
            "Anim_", "ABP_", "Montage_", "NS_", "FX_", "P_", "Emitter_",
            "Font_", "Sound_", "Cue_", "MetaSound_", "Level_", "LS_", "Seq_"
        };

        static readonly string[] GameplayBuckets =
        {
            "Ship", "Weapon", "Gun", "Missile", "Armor", "Shield", "Hull", "Engine",
            "Inventory", "Item", "Loot", "Quest", "Dialog", "NPC", "Faction",
            "Galaxy", "Planet", "Station", "Trade", "Skill", "Perk", "Buff",
            "Damage", "Health", "Player", "Save", "Economy", "Credit"
        };

        public static void Enrich(GameAnalysisReport report, string installPath)
        {
            if (report.ExtraMetadata == null)
                report.ExtraMetadata = new Dictionary<string, string>();

            var allPaths = CollectPaths(report);
            MineNamingPatterns(report, allPaths);
            MineGameplayBuckets(report, allPaths);
            MineUiFxAnim(report, allPaths);
            BuildInjectionRisk(report, installPath);
            InventoryDeployedMods(report, installPath);
            BuildSpecVertCoverage(report);
            BuildModPipelineHints(report);
            ScanLooseConfigAndLoc(report, installPath);
            FingerprintPaks(report);
            SuggestLoadOrderBands(report);
            NoteLatestUpdate(report);
        }

        static readonly string[] UpdateFolders =
        {
            "1CCG", "sub:1CCG/Cards", "sub:1CCG/Frames", "sub:1CCG/Parallax",
            "DLCQuest", "sub:DLCQuest/0Begin", "sub:DLCQuest/0Find1", "sub:DLCQuest/0Find2", "sub:DLCQuest/GetShip",
            "sub:1Widgets/News", "sub:Packes/ProMainMenuKit", "sub:1Data/1RPG", "d3:1Data/1_PlayerFaction/PF_Diplomacy",
            "sub:1Data/Weapons", "THE_DECAL_FACTORY", "ScienceLab"
        };

        static void NoteLatestUpdate(GameAnalysisReport report)
        {
            if (report.ExtraMetadata == null) return;
            report.ExtraMetadata["updatewatch:_note"] = "October 2026 update grew 1CCG (cards), DLCQuest voices, News widgets, ProMainMenuKit, RPG curves, and diplomacy. Pak SB2-WindowsNoEditor.pak was rewritten.";
            foreach (string key in UpdateFolders)
            {
                string meta = "folder:" + key;
                string count;
                if (report.ExtraMetadata.TryGetValue(meta, out count))
                    report.ExtraMetadata["updatewatch:" + key] = count;
            }
            if (report.EasyModTargets != null)
            {
                report.EasyModTargets.Add("UPDATE: 1CCG cards/frames/parallax — new scaffold preset and template.");
                report.EasyModTargets.Add("UPDATE: DLCQuest 0Begin/0Find/GetShip voices — replace cooked trios, do not rename.");
                report.EasyModTargets.Add("UPDATE: 1Widgets/News and Packes/ProMainMenuKit — UI overrides.");
                report.EasyModTargets.Add("UPDATE: 1Data/1RPG/Curves and PF_Diplomacy — balance data.");
            }
        }

        public static void WriteExtraReports(GameAnalysisReport report, string outputDirectory)
        {
            if (string.IsNullOrEmpty(outputDirectory) || report == null) return;
            Directory.CreateDirectory(outputDirectory);

            WriteMetaSection(outputDirectory, "SB2_NamingPatterns_LATEST.txt", report, "namepat:");
            WriteMetaSection(outputDirectory, "SB2_GameplayBuckets_LATEST.txt", report, "gameplay:");
            WriteMetaSection(outputDirectory, "SB2_UiFxAnim_LATEST.txt", report, "sys:");
            WriteMetaSection(outputDirectory, "SB2_InjectionRisk_LATEST.txt", report, "risk:");
            WriteMetaSection(outputDirectory, "SB2_DeployedMods_LATEST.txt", report, "deployed:");
            WriteMetaSection(outputDirectory, "SB2_SpecVertCoverage_LATEST.txt", report, "specvert:");
            WriteMetaSection(outputDirectory, "SB2_PipelineHints_LATEST.txt", report, "pipeline:");
            WriteMetaSection(outputDirectory, "SB2_ConfigLoc_LATEST.txt", report, "cfgloc:");
            WriteMetaSection(outputDirectory, "SB2_LoadOrderBands_LATEST.txt", report, "loadorder:");
            WriteMetaSection(outputDirectory, "SB2_UpdateWatch_LATEST.txt", report, "updatewatch:");

            // Master “return to Grok” feedback file — dense, intentional
            var fb = new StringBuilder();
            fb.AppendLine("SB2_AUTHOR_FEEDBACK_DEEP");
            fb.AppendLine("Generated: " + report.AnalyzedAtUtc.ToString("o"));
            fb.AppendLine("Purpose: feed next tool iteration — taxonomy, risks, gaps, mod opportunities.");
            fb.AppendLine();
            foreach (string prefix in new[] { "namepat:", "gameplay:", "sys:", "risk:", "deployed:", "specvert:", "pipeline:", "cfgloc:", "loadorder:", "fingerprint:", "updatewatch:" })
            {
                var rows = report.ExtraMetadata.Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(kv => kv.Key).ToList();
                if (rows.Count == 0) continue;
                fb.AppendLine("## " + prefix.TrimEnd(':'));
                foreach (var kv in rows) fb.AppendLine(kv.Key + " = " + kv.Value);
                fb.AppendLine();
            }
            if (report.EasyModTargets != null)
            {
                fb.AppendLine("## targets_sample");
                foreach (var t in report.EasyModTargets.Take(80)) fb.AppendLine(t);
            }
            string deep = Path.Combine(outputDirectory, "SB2_AuthorFeedback_Deep_LATEST.txt");
            File.WriteAllText(deep, fb.ToString(), Encoding.UTF8);
            // Merge note into main feedback if present path known
            try
            {
                string mainFb = Path.Combine(outputDirectory, "SB2_AuthorFeedback_LATEST.txt");
                if (File.Exists(mainFb))
                    File.AppendAllText(mainFb, Environment.NewLine + "--- DEEP feedback also written: SB2_AuthorFeedback_Deep_LATEST.txt ---" + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        static void WriteMetaSection(string dir, string file, GameAnalysisReport report, string prefix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(file);
            sb.AppendLine("Generated: " + report.AnalyzedAtUtc.ToString("o"));
            sb.AppendLine();
            foreach (var kv in report.ExtraMetadata.Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x =>
            {
                int n; return int.TryParse(x.Value.Split(' ')[0], out n) ? n : 0;
            }))
                sb.AppendLine(kv.Key + " = " + kv.Value);
            File.WriteAllText(Path.Combine(dir, file), sb.ToString(), Encoding.UTF8);
        }

        static List<string> CollectPaths(GameAnalysisReport report)
        {
            var list = new List<string>();
            void AddRange(IEnumerable<string> src)
            {
                if (src == null) return;
                foreach (var s in src)
                    if (!string.IsNullOrWhiteSpace(s)) list.Add(s.Replace('\\', '/'));
            }
            if (report.PakListSamples != null)
                foreach (var kv in report.PakListSamples)
                    AddRange(kv.Value);
            AddRange(report.SampleGameAssetPaths);
            AddRange(report.SampleEnginePaths);
            AddRange(report.UmapPathsSampled);
            AddRange(report.PluginPathsSampled);
            AddRange(report.LocresPathsFound);
            AddRange(report.ConfigPathsInPak);
            if (report.ExtraMetadata != null)
            {
                foreach (var kv in report.ExtraMetadata)
                {
                    if (kv.Key.StartsWith("samples:", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.StartsWith("d3:", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var part in (kv.Value ?? "").Split(new[] { '|', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            list.Add(part.Trim().Replace('\\', '/'));
                    }
                }
            }
            return list;
        }

        static void MineNamingPatterns(GameAnalysisReport report, List<string> paths)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var samples = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                string name = SafeBaseName(path);
                if (string.IsNullOrEmpty(name)) continue;
                foreach (string pre in NamePrefixes)
                {
                    if (name.StartsWith(pre, StringComparison.OrdinalIgnoreCase) ||
                        name.IndexOf("_" + pre, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Inc(counts, pre);
                        List<string> s;
                        if (!samples.TryGetValue(pre, out s)) { s = new List<string>(); samples[pre] = s; }
                        if (s.Count < 5 && !s.Contains(path)) s.Add(path);
                    }
                }
                // Soft path tokens
                if (path.IndexOf("/Blueprints/", StringComparison.OrdinalIgnoreCase) >= 0) Inc(counts, "folder:Blueprints");
                if (path.IndexOf("/DataTable", StringComparison.OrdinalIgnoreCase) >= 0) Inc(counts, "folder:DataTable");
                if (path.IndexOf("/UI/", StringComparison.OrdinalIgnoreCase) >= 0) Inc(counts, "folder:UI");
            }
            foreach (var kv in counts.OrderByDescending(x => x.Value).Take(40))
            {
                string sample = samples.ContainsKey(kv.Key) ? " | " + string.Join(" ; ", samples[kv.Key]) : "";
                report.ExtraMetadata["namepat:" + kv.Key] = kv.Value + sample;
            }
            report.ExtraMetadata["namepat:_summary"] = "Prefix hits help target BP/UI/material/texture/anim assets for UAssetGUI edits.";
        }

        static void MineGameplayBuckets(GameAnalysisReport report, List<string> paths)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var samples = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                foreach (string token in GameplayBuckets)
                {
                    if (path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Inc(counts, token);
                        List<string> s;
                        if (!samples.TryGetValue(token, out s)) { s = new List<string>(); samples[token] = s; }
                        if (s.Count < 4) s.Add(path);
                    }
                }
            }
            foreach (var kv in counts.OrderByDescending(x => x.Value).Take(30))
            {
                string sample = samples.ContainsKey(kv.Key) ? " | " + string.Join(" ; ", samples[kv.Key].Take(3)) : "";
                report.ExtraMetadata["gameplay:" + kv.Key] = kv.Value + sample;
                if (report.EasyModTargets != null && kv.Value >= 3)
                    report.EasyModTargets.Add("GAMEPLAY " + kv.Key + " (~" + kv.Value + " path hits) — candidate for value/asset override mods.");
            }
        }

        static void MineUiFxAnim(GameAnalysisReport report, List<string> paths)
        {
            string[] systems = { "Widget", "UMG", "Slate", "Niagara", "Cascade", "Particle", "AnimBlueprint", "AnimSequence", "Physics", "Chaos", "Landscape", "Foliage", "MediaPlayer", "LevelSequence", "MovieScene" };
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
                foreach (string s in systems)
                    if (path.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) Inc(counts, s);
            foreach (var kv in counts.OrderByDescending(x => x.Value).Take(25))
                report.ExtraMetadata["sys:" + kv.Key] = kv.Value.ToString();
        }

        static void BuildInjectionRisk(GameAnalysisReport report, string install)
        {
            report.ExtraMetadata["risk:base_pak"] = "Never replace WindowsNoEditor / primary non-_P chunks unless AllowVanillaCoreEdits + backup.";
            report.ExtraMetadata["risk:oodie"] = report.UsesOodle ? "Oodle compression — pack with UnrealPak 4.27 only." : "Oodle not detected; still prefer Engine 4.27 UnrealPak.";
            report.ExtraMetadata["risk:io_store"] = report.HasIoStore ? "IoStore present — some assets may be utoc/ucas; verify before assuming pure PAK override." : "No IoStore flag — classic PAK override path preferred.";
            report.ExtraMetadata["risk:monolith"] = report.UsesSingleMonolithicPak ? "Large monolithic pak — only ship small z_*_P.pak diffs." : "Multiple paks — still prefer additive _P patches.";
            report.ExtraMetadata["risk:uexp"] = "Always stage .uasset+.uexp(+.ubulk) together; incomplete trios crash loads.";
            report.ExtraMetadata["risk:mount"] = "Mount point typically ../../../ with staged SB2/Content/...";
            if (!string.IsNullOrEmpty(install))
            {
                string paks = Path.Combine(install, "SB2", "Content", "Paks");
                if (!Directory.Exists(paks)) paks = Path.Combine(install, "Content", "Paks");
                if (Directory.Exists(paks))
                {
                    int baseCount = 0, patchCount = 0;
                    try
                    {
                        foreach (string f in Directory.GetFiles(paks, "*.pak"))
                        {
                            string n = Path.GetFileName(f);
                            if (n.IndexOf("_P", StringComparison.OrdinalIgnoreCase) >= 0 || n.StartsWith("z_", StringComparison.OrdinalIgnoreCase))
                                patchCount++;
                            else baseCount++;
                        }
                    }
                    catch { }
                    report.ExtraMetadata["risk:pak_counts"] = "baseish=" + baseCount + " patchlike=" + patchCount;
                }
            }
        }

        static void InventoryDeployedMods(GameAnalysisReport report, string install)
        {
            if (string.IsNullOrEmpty(install)) return;
            string paks = Path.Combine(install, "SB2", "Content", "Paks");
            if (!Directory.Exists(paks)) paks = Path.Combine(install, "Content", "Paks");
            if (!Directory.Exists(paks)) { report.ExtraMetadata["deployed:none"] = "Paks folder not found"; return; }
            try
            {
                int i = 0;
                foreach (string f in Directory.GetFiles(paks, "*.pak").OrderBy(x => x))
                {
                    string n = Path.GetFileName(f);
                    var fi = new FileInfo(f);
                    bool patch = n.IndexOf("_P", StringComparison.OrdinalIgnoreCase) >= 0 || n.StartsWith("z_", StringComparison.OrdinalIgnoreCase);
                    report.ExtraMetadata["deployed:" + (i++)] = (patch ? "PATCH " : "BASE  ") + n + " size=" + fi.Length + " mtime=" + fi.LastWriteTimeUtc.ToString("o");
                    if (i >= 80) break;
                }
                report.ExtraMetadata["deployed:summary"] = "Listed up to 80 paks — PATCH entries are existing overrides to coordinate load order with.";
            }
            catch (Exception ex) { report.ExtraMetadata["deployed:error"] = ex.Message; }
        }

        static void BuildSpecVertCoverage(GameAnalysisReport report)
        {
            foreach (string mod in SpecVertService.ModuleIds)
            {
                int hits = 0;
                if (report.EasyModTargets != null)
                    hits += report.EasyModTargets.Count(t => t.IndexOf(mod, StringComparison.OrdinalIgnoreCase) >= 0);
                if (report.ExtraMetadata != null)
                    hits += report.ExtraMetadata.Count(kv => kv.Key.IndexOf(mod, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (kv.Value != null && kv.Value.IndexOf(mod, StringComparison.OrdinalIgnoreCase) >= 0));
                report.ExtraMetadata["specvert:" + mod] = hits + " related signals";
            }
            report.ExtraMetadata["specvert:_hint"] = "Low signals → re-run deep pak list or expand FModel browse under that module.";
        }

        static void BuildModPipelineHints(GameAnalysisReport report)
        {
            report.ExtraMetadata["pipeline:textures"] = "FModel export PNG → edit → UE4.27 import/cook → stage trio → z_*_P.pak";
            report.ExtraMetadata["pipeline:materials"] = "UAssetGUI 4.27 on MI_/Mat_ packages; keep parent material refs valid.";
            report.ExtraMetadata["pipeline:meshes"] = "UModel preview OBJ; replace skeletal carefully (skeleton compatibility).";
            report.ExtraMetadata["pipeline:dialog"] = "Script Editor stubs + UAssetGUI string props under Dialogs/<NPC>/.";
            report.ExtraMetadata["pipeline:quest"] = "Quest data under 1Data/Quests; document stages in Object Editor Quest type.";
            report.ExtraMetadata["pipeline:audio"] = "Loose FMOD Desktop banks if on disk; else pack banks matching paths.";
            report.ExtraMetadata["pipeline:movies"] = "Loose Content/Movies mp4 same filename — easiest visual mod.";
            report.ExtraMetadata["pipeline:ui"] = "WBP_/UMG packages via UAssetGUI; test resolution scaling.";
            report.ExtraMetadata["pipeline:datatable"] = "DT_ assets strong for balance numbers if present in namepat.";
            report.ExtraMetadata["pipeline:config"] = "SB2/Config ini overrides when loose config exists; else Engine.ini is risky.";
            report.ExtraMetadata["pipeline:live"] = "Optional UE4SS for runtime only — not a substitute for file mods.";
            report.ExtraMetadata["pipeline:injection"] = "Mod Loader managed z_<order>_*_P.pak only; PathGuard blocks vanilla unless setting enabled.";
        }

        static void ScanLooseConfigAndLoc(GameAnalysisReport report, string install)
        {
            if (string.IsNullOrEmpty(install)) return;
            string[] cfgRoots =
            {
                Path.Combine(install, "SB2", "Config"),
                Path.Combine(install, "Engine", "Config"),
                Path.Combine(install, "SB2", "Content", "Localization")
            };
            int n = 0;
            foreach (string root in cfgRoots)
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    foreach (string f in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
                    {
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        if (ext != ".ini" && ext != ".locres" && ext != ".locmeta") continue;
                        report.ExtraMetadata["cfgloc:" + (n++)] = RelativeSafe(install, f);
                        if (n >= 60) return;
                    }
                }
                catch { }
            }
            if (n == 0) report.ExtraMetadata["cfgloc:none"] = "No loose ini/locres found under standard roots (may be pak-only).";
        }

        static void FingerprintPaks(GameAnalysisReport report)
        {
            if (report.PakFiles == null) return;
            long sum = 0; int c = 0;
            foreach (var p in report.PakFiles)
            {
                sum ^= p.SizeBytes;
                sum ^= (p.FileName ?? "").GetHashCode();
                c++;
            }
            report.ExtraMetadata["fingerprint:pakset"] = "count=" + c + " xor=" + sum.ToString("X16") + " totalBytes=" + report.TotalPakBytes;
            report.ExtraMetadata["fingerprint:use"] = "Compare fingerprint across Analyzer runs after game updates to detect content changes.";
        }

        static void SuggestLoadOrderBands(GameAnalysisReport report)
        {
            report.ExtraMetadata["loadorder:10"] = "Core fixes / crash patches";
            report.ExtraMetadata["loadorder:20"] = "Config & localization";
            report.ExtraMetadata["loadorder:30"] = "DataTables / balance numbers";
            report.ExtraMetadata["loadorder:40"] = "Meshes materials textures";
            report.ExtraMetadata["loadorder:50"] = "Characters armor sets";
            report.ExtraMetadata["loadorder:60"] = "UI widgets";
            report.ExtraMetadata["loadorder:70"] = "Audio FMOD";
            report.ExtraMetadata["loadorder:80"] = "Movies cinematics";
            report.ExtraMetadata["loadorder:90"] = "Quest dialog narrative";
            report.ExtraMetadata["loadorder:99"] = "Personal overrides (last wins)";
        }

        static string RelativeSafe(string root, string full)
        {
            try
            {
                if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    return full.Substring(root.Length).TrimStart('\\', '/');
            }
            catch { }
            return full;
        }

        static string SafeBaseName(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string s = path.Replace('\\', '/').Trim().Trim('"', '\'');
            // Take last path segment only (avoid Path.* — pak lines can have illegal chars)
            int slash = s.LastIndexOf('/');
            if (slash >= 0 && slash + 1 < s.Length)
                s = s.Substring(slash + 1);
            var sb = new StringBuilder(s.Length);
            foreach (char ch in s)
            {
                if (ch < 32) continue;
                if (ch == '<' || ch == '>' || ch == ':' || ch == '|' || ch == '?' || ch == '*' || ch == '"')
                    continue;
                if (ch == ' ' || ch == '\t')
                    break;
                sb.Append(ch);
            }
            s = sb.ToString();
            int dot = s.LastIndexOf('.');
            if (dot > 0) s = s.Substring(0, dot);
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        static void Inc(Dictionary<string, int> d, string key)
        {
            int v; d.TryGetValue(key, out v); d[key] = v + 1;
        }
    }
}
