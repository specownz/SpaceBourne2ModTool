using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Loads the latest Game Analyzer reports from disk so the UI can refresh
    /// depth-3 presets (character folders, armor sets, etc.) without recompiling.
    /// </summary>
    public static class AnalysisImporter
    {
        public class Snapshot
        {
            public Dictionary<string, int> Depth3Counts { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> MajorCharacters { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> NewArmorSets { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> NpcChildren { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public List<string> SourceFiles { get; set; } = new List<string>();
            public Dictionary<string, int> QuestFolders { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> CinematicFolders { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public List<string> ToolStatusLines { get; set; } = new List<string>();
            public List<string> EasyTargets { get; set; } = new List<string>();
            public string SummaryText()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Analysis snapshot");
                sb.AppendLine("Sources: " + string.Join(", ", SourceFiles.Select(Path.GetFileName)));
                sb.AppendLine("Depth-3 entries: " + Depth3Counts.Count);
                sb.AppendLine("Major characters: " + MajorCharacters.Count);
                foreach (var kv in MajorCharacters.OrderByDescending(x => x.Value))
                    sb.AppendLine("  " + kv.Value + "  " + kv.Key);
                sb.AppendLine("1_New_sets: " + NewArmorSets.Count);
                foreach (var kv in NewArmorSets.OrderByDescending(x => x.Value).Take(15))
                    sb.AppendLine("  " + kv.Value + "  " + kv.Key);
                sb.AppendLine("NPC children: " + NpcChildren.Count);
                foreach (var kv in NpcChildren.OrderByDescending(x => x.Value).Take(10))
                    sb.AppendLine("  " + kv.Value + "  " + kv.Key);
                sb.AppendLine("Quest folders: " + QuestFolders.Count);
                foreach (var kv in QuestFolders.OrderByDescending(x => x.Value).Take(8))
                    sb.AppendLine("  " + kv.Value + "  " + kv.Key);
                sb.AppendLine("Cinematic: " + CinematicFolders.Count);
                sb.AppendLine("Easy targets: " + EasyTargets.Count);
                return sb.ToString();
            }
        }

        static Snapshot _cache;
        static DateTime _cacheTime;

        public static Snapshot LoadFromDefaultLocations()
        {
            if (_cache != null && (DateTime.UtcNow - _cacheTime).TotalSeconds < 30) return _cache;

            var roots = new List<string>();
            if (!string.IsNullOrEmpty(ConfigService.ModsOutputPath))
            {
                roots.Add(ConfigService.ModsOutputPath);
                roots.Add(Path.Combine(ConfigService.ModsOutputPath, "Analysis"));
            }
            string docs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpaceBourne2ModTool");
            roots.Add(docs);
            if (!string.IsNullOrEmpty(ConfigService.Game.InstallPath))
                roots.Add(Path.Combine(ConfigService.Game.InstallPath, "SB2ModTool_Reports"));

            var snap = new Snapshot();
            foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;
                foreach (string name in new[] { "SB2_ContentMap_LATEST.txt", "SB2_AuthorFeedback_LATEST.txt", "SB2_ModTargets_LATEST.txt", "SB2_ToolCrossRef_LATEST.txt", "SB2_AssetHealth_LATEST.txt" })
                {
                    string path = Path.Combine(root, name);
                    if (File.Exists(path))
                    {
                        ParseFile(path, snap);
                        snap.SourceFiles.Add(path);
                        EnrichFromFile(snap, path);
                    }
                }
            }
            _cache = snap; _cacheTime = DateTime.UtcNow; return snap;
        }

        public static void ParseFile(string path, Snapshot snap)
        {
            string[] lines;
            try { lines = File.ReadAllLines(path); }
            catch { return; }

            bool inMaj = false, inNpc = false, inSets = false, inD3 = false;
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.StartsWith("== Majorcharacters", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("--- DEPTH3_CHARACTER", StringComparison.OrdinalIgnoreCase))
                { inMaj = true; inNpc = inSets = inD3 = false; continue; }
                if (line.StartsWith("== 1Data/NPCs", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("--- DEPTH3_NPCS", StringComparison.OrdinalIgnoreCase))
                { inNpc = true; inMaj = inSets = inD3 = false; continue; }
                if (line.StartsWith("== Armors/1_New_sets", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("--- DEPTH3_ARMOR", StringComparison.OrdinalIgnoreCase))
                { inSets = true; inMaj = inNpc = inD3 = false; continue; }
                if (line.StartsWith("== depth-3", StringComparison.OrdinalIgnoreCase))
                { inD3 = true; inMaj = inNpc = inSets = false; continue; }
                if (line.StartsWith("==") || line.StartsWith("---"))
                { inMaj = inNpc = inSets = inD3 = false; continue; }

                int tab = line.IndexOf('\t');
                if (tab <= 0) continue;
                int count;
                if (!int.TryParse(line.Substring(0, tab).Trim(), out count) || count <= 0) continue;
                string pathPart = line.Substring(tab + 1).Trim();
                if (pathPart.StartsWith("d3:", StringComparison.OrdinalIgnoreCase))
                    pathPart = pathPart.Substring(3);

                if (inMaj)
                {
                    string name = pathPart.Contains("/") ? pathPart.Split('/').Last() : pathPart;
                    if (pathPart.StartsWith("Armors/Majorcharacters/", StringComparison.OrdinalIgnoreCase))
                        name = pathPart.Substring("Armors/Majorcharacters/".Length).Split('/')[0];
                    snap.MajorCharacters[name] = count;
                    snap.Depth3Counts["Armors/Majorcharacters/" + name] = count;
                }
                else if (inNpc)
                {
                    string name = pathPart.Contains("/") ? pathPart.Split('/').Last() : pathPart;
                    snap.NpcChildren[name] = count;
                    snap.Depth3Counts["1Data/NPCs/" + name] = count;
                }
                else if (inSets)
                {
                    // Armors/1_New_sets/M_2 or just M_2
                    string rest = pathPart;
                    if (rest.StartsWith("Armors/1_New_sets/", StringComparison.OrdinalIgnoreCase))
                        rest = rest.Substring("Armors/1_New_sets/".Length);
                    string setName = rest.Split('/')[0];
                    if (!string.IsNullOrEmpty(setName) && !setName.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
                    {
                        int prev;
                        snap.NewArmorSets.TryGetValue(setName, out prev);
                        if (count > prev) snap.NewArmorSets[setName] = count;
                        snap.Depth3Counts["Armors/1_New_sets/" + setName] = snap.NewArmorSets[setName];
                    }
                }
                else if (inD3 || pathPart.Contains("/"))
                {
                    snap.Depth3Counts[pathPart] = count;
                    if (pathPart.StartsWith("Armors/Majorcharacters/", StringComparison.OrdinalIgnoreCase))
                    {
                        string name = pathPart.Substring("Armors/Majorcharacters/".Length).Split('/')[0];
                        snap.MajorCharacters[name] = count;
                    }
                }
            }
        }

        public static List<ContentCatalog.NamedPreset> ToPresets(Snapshot snap)
        {
            var list = new List<ContentCatalog.NamedPreset>();
            foreach (var kv in snap.MajorCharacters.OrderByDescending(x => x.Value))
            {
                list.Add(new ContentCatalog.NamedPreset
                {
                    Id = "import_char_" + kv.Key,
                    DisplayName = "Majorcharacter / " + kv.Key + "  (imported)",
                    StagingPath = "Armors/Majorcharacters/" + kv.Key,
                    ApproxCount = kv.Value,
                    Role = "Imported from analysis — named major character",
                    SamplePaths = new[] { "SB2/Content/Armors/Majorcharacters/" + kv.Key + "/" }
                });
            }
            foreach (var kv in snap.NewArmorSets.OrderByDescending(x => x.Value))
            {
                list.Add(new ContentCatalog.NamedPreset
                {
                    Id = "import_set_" + kv.Key,
                    DisplayName = "1_New_sets / " + kv.Key + "  (imported)",
                    StagingPath = "Armors/1_New_sets/" + kv.Key,
                    ApproxCount = kv.Value,
                    Role = "Imported armor set",
                    SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/" + kv.Key + "/" }
                });
            }
            
            foreach (var kv in snap.QuestFolders.OrderByDescending(x => x.Value).Take(12))
            {
                list.Add(new ContentCatalog.NamedPreset
                {
                    Id = "quest_" + kv.Key,
                    DisplayName = "Quest / " + kv.Key,
                    StagingPath = "1Data/Quests/" + kv.Key,
                    ApproxCount = kv.Value,
                    Role = "Imported quest folder from analysis"
                });
            }
            foreach (var kv in snap.CinematicFolders.OrderByDescending(x => x.Value).Take(8))
            {
                list.Add(new ContentCatalog.NamedPreset
                {
                    Id = "cine_" + kv.Key,
                    DisplayName = "Cinematic / " + kv.Key,
                    StagingPath = "111Cinematic/" + kv.Key,
                    ApproxCount = kv.Value,
                    Role = "Imported cinematic folder from analysis"
                });
            }

            return list;
        }
        private static void EnrichFromFile(Snapshot snap, string path)
        {
            string name = Path.GetFileName(path);
            try
            {
                foreach (string raw in File.ReadLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0) continue;
                    if (name.IndexOf("ModTargets", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("AuthorFeedback", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (line.StartsWith("QUEST ", StringComparison.OrdinalIgnoreCase))
                        {
                            // QUEST Obsesif (~181) — stage 1Data/Quests/Obsesif
                            ParseNamedCount(line, "QUEST ", snap.QuestFolders);
                            snap.EasyTargets.Add(line);
                        }
                        else if (line.StartsWith("CINEMATIC ", StringComparison.OrdinalIgnoreCase))
                        {
                            ParseNamedCount(line, "CINEMATIC ", snap.CinematicFolders);
                            snap.EasyTargets.Add(line);
                        }
                        else if (line.StartsWith("MAJORCHAR ", StringComparison.OrdinalIgnoreCase)
                                 || line.StartsWith("ARMORSET ", StringComparison.OrdinalIgnoreCase)
                                 || line.StartsWith("LOOSE ", StringComparison.OrdinalIgnoreCase)
                                 || line.StartsWith("NPC ", StringComparison.OrdinalIgnoreCase)
                                 || line.StartsWith("CONFIG ", StringComparison.OrdinalIgnoreCase)
                                 || line.StartsWith("LOCRES ", StringComparison.OrdinalIgnoreCase))
                        {
                            snap.EasyTargets.Add(line);
                        }
                    }
                    if (name.IndexOf("ToolCrossRef", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (line.StartsWith("[OK]") || line.StartsWith("[--]") || line.StartsWith("• "))
                            snap.ToolStatusLines.Add(line);
                    }
                }
            }
            catch { }
        }

        private static void ParseNamedCount(string line, string prefix, Dictionary<string, int> dest)
        {
            try
            {
                string rest = line.Substring(prefix.Length).Trim();
                int tilde = rest.IndexOf("(~");
                string name = tilde > 0 ? rest.Substring(0, tilde).Trim() : rest.Split(' ')[0];
                int count = 0;
                int open = rest.IndexOf("(~");
                int close = rest.IndexOf(')');
                if (open >= 0 && close > open)
                    int.TryParse(rest.Substring(open + 2, close - open - 2).Trim(), out count);
                if (!string.IsNullOrEmpty(name))
                    dest[name] = count;
            }
            catch { }
        }

    }
}
