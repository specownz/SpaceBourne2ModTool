using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Complete mod templates + user custom templates. Automates scaffold, docs, objects,
    /// scripts, SpecVert links, load-order, response-file stubs — no external tools required
    /// for structure; cooked asset bytes still need FModel/UAssetGUI when replacing binaries.
    /// </summary>
    public static class ModTemplateService
    {
        public class Template
        {
            public string Id, Name, Description, Category;
            public int DefaultLoadOrder;
            public string[] StagingFolders;      // under SB2/Content
            public string[] ObjectTypes;         // ObjectTypeRegistry ids to seed
            public string[] ScriptKinds;         // Dialog/Quest stubs
            public string[] TargetPaths;         // TARGET_PATHS.txt
            public string WorkflowNotes;
            public bool SpecVerted;
            public bool IsUser;
            public override string ToString() { return (IsUser ? "[User] " : "") + Category + " — " + Name; }
        }

        public static string UserTemplatesDir
        {
            get
            {
                string d = Path.Combine(ConfigService.ModsOutputPath ?? "", "ModTemplates");
                Directory.CreateDirectory(d);
                return d;
            }
        }

        public static IEnumerable<Template> BuiltIn()
        {
            // Derived from real SB2 analysis layout
            yield return T("major_all", "All Major Characters", "Scaffold all 10 Majorcharacters folders + Values notes", "Characters", 50,
                Folders("Armors/Majorcharacters/B_Old_Lady", "Armors/Majorcharacters/N_GuzelWoman", "Armors/Majorcharacters/N_OldEvil",
                    "Armors/Majorcharacters/N_Good_Boy", "Armors/Majorcharacters/N_Cinlii", "Armors/Majorcharacters/N_GWomen",
                    "Armors/Majorcharacters/N_Zayif", "Armors/Majorcharacters/N_RoboGirl", "Armors/Majorcharacters/Good_OldMan", "Armors/Majorcharacters/Sister"),
                new[] { "Character" }, null, true,
                "Export mesh/mat trios per character from FModel → drop into matching Staging paths → pack z_*_P.");

            yield return T("armor_sets", "Armor Sets M/F", "1_New_sets M_1–M_6 and F_1–F_3", "Characters", 50,
                Folders("Armors/1_New_sets/M_1", "Armors/1_New_sets/M_2", "Armors/1_New_sets/M_3", "Armors/1_New_sets/M_4",
                    "Armors/1_New_sets/M_5", "Armors/1_New_sets/M_6", "Armors/1_New_sets/F_1", "Armors/1_New_sets/F_2", "Armors/1_New_sets/F_3"),
                new[] { "Armor" }, null, true, "Stage cooked armor meshes/materials; keep trio intact.");

            yield return T("quest_obsesif", "Quest: Obsesif", "Largest quest folder scaffold + script stub", "Narrative", 90,
                Folders("1Data/Quests/Obsesif"), new[] { "Quest" }, new[] { "Quest" }, true,
                "Edit quest properties via Object Editor; binary graphs via UAssetGUI when needed.");

            yield return T("quest_pack", "Quest Starter Pack", "Obsesif, Datas, and common numbered quests", "Narrative", 90,
                Folders("1Data/Quests/Obsesif", "1Data/Quests/Datas", "1Data/Quests"),
                new[] { "Quest" }, new[] { "Quest" }, true, "Use Script Editor after Analyzer for more quest paths.");

            yield return T("update_ccg", "Update: Card Game (1CCG)", "Cards, frames, and parallax from the latest game update", "Gameplay", 40,
                Folders("1CCG/Cards", "1CCG/Cards/Materials", "1CCG/Cards/Textures", "1CCG/Frames", "1CCG/Parallax"),
                new[] { "Item" }, null, true, "Override existing 1CCG packages. Keep uasset, uexp, and ubulk together. Pack as z_*_P.pak.");

            yield return T("update_dlcquest", "Update: DLC Quest Voices", "0Begin, 0Find1, 0Find2, and GetShip including new voice folders", "Narrative", 90,
                Folders("DLCQuest/0Begin", "DLCQuest/0Begin/Sounds", "DLCQuest/0Find1/Voices", "DLCQuest/0Find2/Voices", "DLCQuest/GetShip"),
                new[] { "Quest" }, new[] { "Quest" }, true, "Voice files are cooked packages. Replace the whole trio, do not rename them.");

            yield return T("update_news_ui", "Update: News UI", "1Widgets/News operations and solar-system screens", "UI", 60,
                Folders("1Widgets/News", "1Widgets/News/Operations", "1Widgets/News/Textures", "1Widgets/News/SolarSystems"),
                new[] { "Generic" }, null, true, "Widget text and textures. Export with FModel, edit with UAssetGUI set to 4.27.");

            yield return T("npc_dialogs", "NPC Dialogs", "1Data/NPCs/Dialogs tree + dialog objects", "Narrative", 90,
                Folders("1Data/NPCs/Dialogs", "1Data/NPCs"), new[] { "NPC", "Dialog" }, new[] { "Dialog" }, true,
                "Seed Script Editor from analysis; replace cooked dialog packages when ready.");

            yield return T("movies_loose", "Loose Movies Pack", "Documents all 82 on-disk Movies replacements", "Media", 80,
                Folders("../Movies"), null, null, false,
                "Copy replacement MP4s into game Content/Movies same filenames OR stage and copy via Loose File Mods. No PAK required.");

            yield return T("fmod_loose", "Loose FMOD Banks", "Content/FMOD Desktop bank replace workflow", "Media", 70,
                Folders("FMOD/Desktop"), null, null, false, "Replace banks with matching names; backup first.");

            yield return T("ui_widgets", "UI Widgets", "1Widgets HUD/menu override scaffold", "UI", 60,
                Folders("1Widgets"), new[] { "Generic" }, null, true, "WBP_/UMG packages via UAssetGUI after FModel export.");

            yield return T("galaxy_ships", "Galaxy / Ships", "1Galaxy content + ship value docs", "Gameplay", 30,
                Folders("1Galaxy"), new[] { "Ship" }, null, true, "Document intended stats in Objects; apply in UAssetGUI.");

            yield return T("weapons_vfx", "Weapons / Ballistics VFX", "BallisticsVFX + weapon notes", "Gameplay", 40,
                Folders("BallisticsVFX"), new[] { "Generic" }, null, true, "VFX/mesh swaps; keep cascade/niagara refs valid.");

            yield return T("meshes_lib", "Mesh Library", "1Meshs staging", "Art", 40,
                Folders("1Meshs"), new[] { "Generic" }, null, true, "Preview OBJ in 3D Viewer; stage cooked trios.");

            yield return T("locomotion", "Locomotion / Anims", "Locomotion folder scaffold", "Art", 40,
                Folders("Locomotion"), new[] { "Generic" }, null, true, "Anim overrides require matching skeleton.");

            yield return T("localization", "Localization", "Game locres staging + guide", "Text", 20,
                Folders("Localization/Game"), new[] { "Generic" }, null, false, "Use Localization panel; .locres tools outside app for binary locres.");

            yield return T("config_ini", "Config Overrides", "SB2/Config ini staging", "Text", 20,
                new string[0], new[] { "Config" }, null, false, "Place .ini under Staging SB2/Config — careful with shipping.");

            yield return T("packes", "Packes Content", "Packes folder for mixed pack overrides", "Art", 40,
                Folders("Packes"), new[] { "Generic" }, null, true, "Large marketplace-style content; override selectively.");

            yield return T("cinematic", "Cinematics / Radio", "111Cinematic + RadioAudio", "Media", 80,
                Folders("111Cinematic/RadioAudio", "111Cinematic"), new[] { "Cinematic" }, null, true, "LevelSequences / audio cinematic assets.");

            yield return T("specvert_full", "SpecVert Full Workspace Link", "Marks SpecVerted + opens module map", "Meta", 40,
                Folders("1Data", "Armors", "1Meshs"), new[] { "Generic" }, null, true,
                "Run Tools → specVert first; copy from Modules/*/Staging into this mod.");

            yield return T("steam_grammar", "Steam: Dialogue Grammar Fix", "Scaffold dialogs/quests + grammar workflow", "SteamRequests", 90,
                Folders("1Data/NPCs/Dialogs", "1Data/Quests", "Localization/Game"), new[] { "Dialog", "Quest" }, new[] { "Dialog", "Quest" }, false,
                "Use Mod Opportunity Studio grammar tab; stage cleaned text; UAssetGUI for cooked strings.");
            yield return T("steam_world_expose", "Steam: Graphics/Planets/Story", "Expose galaxy textures meshes quests cinematics", "SteamRequests", 50,
                Folders("1Galaxy", "_Textures", "1Meshs", "Armors", "1Data/Quests", "111Cinematic", "1Widgets"), new[] { "Character", "Cinematic", "Generic" }, null, true,
                "Update art over time; pack selective z_*_P overrides.");
            yield return T("steam_rebalance", "Steam: System Rebalance", "1Data/1Galaxy value override docs", "SteamRequests", 30,
                Folders("1Data", "1Galaxy"), new[] { "Item", "Ship", "Generic" }, null, true,
                "Document numeric changes; apply to DT_/stats assets with UAssetGUI when identified.");
            yield return T("blank_patch", "Blank z_P Patch", "Minimal SB2/Content + packaging checklist only", "Meta", 40,
                new string[0], null, null, false, "Empty staging ready for manual drops.");

            yield return T("balance_datatables", "Balance / DataTables", "1Data root for DT-style overrides", "Gameplay", 30,
                Folders("1Data"), new[] { "Item", "Generic" }, null, true, "If DT_ assets exist, edit with UAssetGUI; document in Objects.");

            yield return T("multi_layer", "Multi-Layer Starter", "Characters + Dialogs + Movies notes", "Meta", 50,
                Folders("Armors/Majorcharacters", "1Data/NPCs/Dialogs"), new[] { "Character", "Dialog" }, new[] { "Dialog" }, true,
                "Combines common paths; split into ordered Speckage packages for release.");
        }

        static Template T(string id, string name, string desc, string cat, int order, string[] folders, string[] objs, string[] scripts, bool sv, string notes) =>
            new Template
            {
                Id = id, Name = name, Description = desc, Category = cat, DefaultLoadOrder = order,
                StagingFolders = folders ?? new string[0], ObjectTypes = objs ?? new string[0],
                ScriptKinds = scripts ?? new string[0], SpecVerted = sv, WorkflowNotes = notes, IsUser = false,
                TargetPaths = folders ?? new string[0]
            };

        static string[] Folders(params string[] f) => f;

        public static List<Template> All()
        {
            var list = BuiltIn().ToList();
            list.AddRange(LoadUserTemplates());
            return list;
        }

        public static List<Template> LoadUserTemplates()
        {
            var list = new List<Template>();
            foreach (string f in Directory.GetFiles(UserTemplatesDir, "*.sb2template.txt"))
            {
                try
                {
                    var t = Deserialize(File.ReadAllText(f, Encoding.UTF8));
                    if (t != null) { t.IsUser = true; list.Add(t); }
                }
                catch { }
            }
            return list;
        }

        public static void SaveUserTemplate(Template t)
        {
            if (t == null || string.IsNullOrWhiteSpace(t.Name)) throw new ArgumentException("Template needs a name");
            if (string.IsNullOrEmpty(t.Id)) t.Id = "user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            t.IsUser = true;
            string path = Path.Combine(UserTemplatesDir, Sanitize(t.Id) + ".sb2template.txt");
            File.WriteAllText(path, Serialize(t), Encoding.UTF8);
        }

        public static void DeleteUserTemplate(string id)
        {
            string path = Path.Combine(UserTemplatesDir, Sanitize(id) + ".sb2template.txt");
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>Apply template onto existing mod root (automates structure).</summary>
        public static string Apply(Template t, string modRoot, ModInfo info = null, System.Windows.Forms.IWin32Window owner = null)
        {
            if (t == null) throw new ArgumentNullException("t");
            if (string.IsNullOrEmpty(modRoot)) throw new ArgumentException("modRoot");
            var report = new ScaffoldMergeService.Report();
            ScaffoldMergeService.EnsureDir(modRoot, report);

            string staging = null;
            try
            {
                staging = Directory.GetDirectories(modRoot)
                    .FirstOrDefault(d => d.EndsWith("_P", StringComparison.OrdinalIgnoreCase));
            }
            catch { }
            if (staging == null)
            {
                string folderName = (info != null ? info.GetModFolderName() : Path.GetFileName(modRoot) + "_P");
                staging = Path.Combine(modRoot, folderName);
            }
            string content = Path.Combine(staging, "SB2", "Content");
            ScaffoldMergeService.EnsureDir(content, report);

            var log = new StringBuilder();
            log.AppendLine("Applied template: " + t.Name + " (" + t.Id + ")");
            log.AppendLine("Load order band: " + t.DefaultLoadOrder);
            log.AppendLine(t.WorkflowNotes ?? "");
            log.AppendLine("Merge-safe scaffold (existing files prompt before overwrite).");

            foreach (string rel in t.StagingFolders ?? new string[0])
            {
                string r = (rel ?? "").Replace('\\', '/');
                if (r.StartsWith("../"))
                {
                    string movies = Path.Combine(staging, "SB2", "Content", "Movies");
                    ScaffoldMergeService.EnsureDir(movies, report);
                    log.AppendLine("  + Movies");
                    continue;
                }
                string full = Path.Combine(content, r.Replace('/', Path.DirectorySeparatorChar));
                ScaffoldMergeService.EnsureDir(full, report);
                string readme = "Template: " + t.Name + "\r\n" + (t.WorkflowNotes ?? "") + "\r\nStage cooked .uasset+.uexp here mirroring game paths.\r\n";
                ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(full, "README_TEMPLATE.txt"), readme, report, "template folder readme");
                log.AppendLine("  + " + r);
            }

            ScaffoldMergeService.MergeLines(Path.Combine(modRoot, "TARGET_PATHS.txt"), t.TargetPaths ?? new string[0], report);
            ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(modRoot, "TEMPLATE_APPLIED.txt"), log.ToString() + "\r\n" + report, report, "template applied log");
            if (!string.IsNullOrEmpty(t.WorkflowNotes))
                ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(modRoot, "WORKFLOW.txt"), t.WorkflowNotes, report, "workflow notes");
            ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(modRoot, "LOAD_ORDER.txt"), t.DefaultLoadOrder.ToString(), report, "load order");

            var rf = new StringBuilder();
            rf.AppendLine("; UnrealPak response-file stub — paths relative to staged content");
            foreach (string rel in t.StagingFolders ?? new string[0])
            {
                if (string.IsNullOrEmpty(rel) || rel.StartsWith("../")) continue;
                string v = "../../../SB2/Content/" + rel.Replace('\\', '/');
                rf.AppendLine("\"" + v + "/\" \"" + v + "/\"");
            }
            ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(modRoot, "response_template.txt"), rf.ToString(), report, "response file stub");

            string objDir = Path.Combine(modRoot, "Objects");
            ScaffoldMergeService.EnsureDir(objDir, report);
            if (t.ObjectTypes != null)
            {
                foreach (string ot in t.ObjectTypes)
                {
                    string stub = "{\r\n  \"type\": \"" + ot + "\",\r\n  \"fromTemplate\": \"" + t.Id + "\",\r\n  \"notes\": \"Edit in Object Editors\"\r\n}\r\n";
                    ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(objDir, ot + "_seed.json"), stub, report, "object seed");
                }
            }

            string scrDir = Path.Combine(modRoot, "Scripts");
            ScaffoldMergeService.EnsureDir(scrDir, report);
            if (t.ScriptKinds != null)
            {
                foreach (string sk in t.ScriptKinds)
                {
                    string stub = "Kind: " + sk + "\r\nFrom template: " + t.Name + "\r\nPaste dialogue or quest text here.\r\n";
                    ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(scrDir, sk + "_stub.txt"), stub, report, "script seed");
                }
            }

            string speck = "DefaultLoadOrder=" + t.DefaultLoadOrder + "\r\nTemplate=" + t.Id + "\r\nSpecVerted=" + t.SpecVerted + "\r\n";
            ScaffoldMergeService.WriteTextMerged(owner, Path.Combine(modRoot, "SPECKAGE_HINT.txt"), speck, report, "speckage hint");

            try
            {
                string fp = GameFingerprint.CurrentFromConfig() ?? GameFingerprint.ReadLatestFromReports();
                if (!string.IsNullOrEmpty(fp)) GameFingerprint.WriteToMod(modRoot, fp, "Stamped on template apply");
            }
            catch { }
            try { CurrentModContext.Open(modRoot); } catch { }

            return log.ToString() + "\r\n" + report.ToString();
        }

        public static string CreateModFromTemplate(Template t, string modName, string author = "spec")
        {
            var info = new ModInfo
            {
                Name = modName,
                Author = author,
                Version = "1.0",
                Description = "From template: " + t.Name + " — " + t.Description,
                ChunkId = 1000 + t.DefaultLoadOrder,
                Type = ModType.AssetReplacement
            };
            var mgr = new ModManager();
            var proj = mgr.CreateNewMod(info);
            Apply(t, proj.ProjectRoot, info);
            try { RecentModsStore.Add(proj.ProjectRoot, info.Name); } catch { }
            return proj.ProjectRoot;
        }

        /// <summary>Save current mod layout as a user template.</summary>
        public static Template CaptureFromMod(string modRoot, string templateName)
        {
            var folders = new List<string>();
            string content = null;
            foreach (string d in Directory.GetDirectories(modRoot))
            {
                string c = Path.Combine(d, "SB2", "Content");
                if (Directory.Exists(c)) { content = c; break; }
            }
            if (content != null)
            {
                foreach (string dir in Directory.GetDirectories(content, "*", SearchOption.AllDirectories))
                {
                    string rel = dir.Substring(content.Length).TrimStart('\\', '/').Replace('\\', '/');
                    if (!string.IsNullOrEmpty(rel) && rel.Split('/').Length <= 3)
                        folders.Add(rel);
                }
            }
            var t = new Template
            {
                Id = "user_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = templateName,
                Description = "Captured from " + modRoot,
                Category = "User",
                DefaultLoadOrder = 40,
                StagingFolders = folders.Distinct().Take(40).ToArray(),
                TargetPaths = folders.Distinct().Take(40).ToArray(),
                ObjectTypes = new string[0],
                ScriptKinds = new string[0],
                SpecVerted = SpecVertService.IsModSpecVerted(modRoot),
                WorkflowNotes = "User-captured template",
                IsUser = true
            };
            SaveUserTemplate(t);
            return t;
        }

        static string Serialize(Template t)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id=" + t.Id);
            sb.AppendLine("Name=" + t.Name);
            sb.AppendLine("Description=" + (t.Description ?? "").Replace('\n', ' '));
            sb.AppendLine("Category=" + t.Category);
            sb.AppendLine("DefaultLoadOrder=" + t.DefaultLoadOrder);
            sb.AppendLine("SpecVerted=" + t.SpecVerted);
            sb.AppendLine("WorkflowNotes=" + (t.WorkflowNotes ?? "").Replace('\n', ' '));
            sb.AppendLine("Folders=" + string.Join("|", t.StagingFolders ?? new string[0]));
            sb.AppendLine("Objects=" + string.Join("|", t.ObjectTypes ?? new string[0]));
            sb.AppendLine("Scripts=" + string.Join("|", t.ScriptKinds ?? new string[0]));
            return sb.ToString();
        }

        static Template Deserialize(string text)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = line.IndexOf('=');
                if (eq > 0) map[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }
            if (!map.ContainsKey("Name")) return null;
            Func<string, string[]> split = k =>
            {
                string v; map.TryGetValue(k, out v);
                return string.IsNullOrEmpty(v) ? new string[0] : v.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            };
            int order = 40;
            if (map.ContainsKey("DefaultLoadOrder")) int.TryParse(map["DefaultLoadOrder"], out order);
            return new Template
            {
                Id = map.ContainsKey("Id") ? map["Id"] : Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = map["Name"],
                Description = map.ContainsKey("Description") ? map["Description"] : "",
                Category = map.ContainsKey("Category") ? map["Category"] : "User",
                DefaultLoadOrder = order,
                SpecVerted = map.ContainsKey("SpecVerted") && map["SpecVerted"].Equals("True", StringComparison.OrdinalIgnoreCase),
                WorkflowNotes = map.ContainsKey("WorkflowNotes") ? map["WorkflowNotes"] : "",
                StagingFolders = split("Folders"),
                TargetPaths = split("Folders"),
                ObjectTypes = split("Objects"),
                ScriptKinds = split("Scripts")
            };
        }

        static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }
    }
}
