using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// A folder of files ripped out of SpaceBourne 2 paks. It is never the Steam install.
    /// Classifies parts (weapon, ship, armor, helmet, …) and names the external tool that wrote each file.
    /// Header peeks follow the public UE 4.27 package, locres, and pak layouts (the same layouts FModel and UAssetGUI document). Those programs are not embedded.
    /// </summary>
    public static class WorkingProjectService
    {
        public const string ReadmeName = "READ_ME_WORKING_FOLDER.txt";

        public static string DefaultPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "specSB2modTool", "WorkingProject");
            }
        }

        public static string Resolve()
        {
            string p = ConfigService.WorkingProjectPath;
            if (string.IsNullOrWhiteSpace(p)) p = DefaultPath;
            return Path.GetFullPath(p);
        }

        public static string Ensure(out string error)
        {
            error = null;
            string path = Resolve();
            string why;
            if (!IsSafeWorkingFolder(path, out why))
            {
                error = why;
                return null;
            }
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, ".specsb2"));
            string readme = Path.Combine(path, ReadmeName);
            if (!File.Exists(readme))
            {
                File.WriteAllText(readme,
                    "specSB2modTool working project folder\r\n" +
                    "This is a copy area for files ripped from SpaceBourne 2 .pak archives.\r\n" +
                    "It is not the game install. Do not point this folder at Steam\\common\\SpaceBourne 2.\r\n" +
                    "Keep .uasset, .uexp, and .ubulk together when you edit a cooked item.\r\n" +
                    "Patch mods still ship as z_YourMod_P.pak. Oodle compression belongs in UnrealPak, not in this folder.\r\n");
            }
            return path;
        }

        public static bool IsSafeWorkingFolder(string path, out string why)
        {
            why = null;
            if (string.IsNullOrWhiteSpace(path))
            {
                why = "Choose a folder first.";
                return false;
            }
            string full;
            try { full = Path.GetFullPath(path); }
            catch (Exception ex) { why = ex.Message; return false; }

            string game = ConfigService.Game != null ? ConfigService.Game.InstallPath : null;
            if (!string.IsNullOrWhiteSpace(game) && Directory.Exists(game))
            {
                string g = Path.GetFullPath(game).TrimEnd('\\', '/');
                if (full.Equals(g, StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith(g + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    why = "That path is inside the game install. The working folder must stay separate so a scan cannot rewrite vanilla files.";
                    return false;
                }
            }
            if (full.IndexOf(Path.DirectorySeparatorChar + "Paks", StringComparison.OrdinalIgnoreCase) >= 0 &&
                full.IndexOf("SpaceBourne", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                why = "That looks like the game Paks folder. Pick a folder under Documents instead.";
                return false;
            }
            return true;
        }

        public class Item
        {
            public string FullPath;
            public string Relative;
            public long Bytes;
            public string Component;
            public string Part;
            public string Tool;
            public string What;
            public string Trio;
            public string Peek;
        }

        public class ScanResult
        {
            public string Folder;
            public List<Item> Items = new List<Item>();
            public string Error;
            public int NewSinceLast;
            public int Reused;
            public long UsedBytes;
        }

        public static ScanResult Scan(string folder, IProgress<string> progress, bool hardRip = false)
        {
            var result = new ScanResult();
            string why;
            if (!IsSafeWorkingFolder(folder, out why))
            {
                result.Error = why;
                return result;
            }
            if (!Directory.Exists(folder))
            {
                result.Error = "Folder does not exist yet. Create it first.";
                return result;
            }
            result.Folder = Path.GetFullPath(folder);
            var cache = LoadCache(result.Folder);
            var seen = new HashSet<string>(cache.Keys, StringComparer.OrdinalIgnoreCase);
            int n = 0;
            foreach (string file in EnumerateSafe(result.Folder))
            {
                n++;
                if (progress != null && (n % 400) == 0)
                    progress.Report((hardRip ? "Hard rip, file " : "Checking file ") + n + "…");
                string rel = file.Substring(result.Folder.Length).TrimStart('\\', '/');
                long bytes = 0;
                long ticks = 0;
                try
                {
                    var fi = new FileInfo(file);
                    bytes = fi.Length;
                    ticks = fi.LastWriteTimeUtc.Ticks;
                }
                catch { }
                Item item = null;
                CacheRow row;
                if (!hardRip && cache.TryGetValue(rel, out row) && row.Bytes == bytes && row.Ticks == ticks && row.Item != null)
                {
                    item = row.Item;
                    item.FullPath = file;
                    item.Bytes = bytes;
                    result.Reused++;
                }
                else
                {
                    item = Inspect(result.Folder, file);
                    if (item != null && !seen.Contains(item.Relative)) result.NewSinceLast++;
                }
                if (item == null) continue;
                result.UsedBytes += item.Bytes;
                result.Items.Add(item);
                cache[item.Relative] = new CacheRow { Item = item, Bytes = item.Bytes, Ticks = ticks };
            }
            SaveCache(result.Folder, cache, result.Items);
            if (progress != null)
                progress.Report("Finished. " + result.Items.Count + " files. " + result.Reused + " reused, " + result.NewSinceLast + " new.");
            return result;
        }

        class CacheRow
        {
            public Item Item;
            public long Bytes;
            public long Ticks;
        }

        static Dictionary<string, CacheRow> LoadCache(string folder)
        {
            var map = new Dictionary<string, CacheRow>(StringComparer.OrdinalIgnoreCase);
            string log = Path.Combine(folder, ".specsb2", "rip-index.txt");
            if (!File.Exists(log)) return map;
            try
            {
                foreach (string line in File.ReadLines(log))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] p = line.Split('\t');
                    if (p.Length < 9) continue;
                    long bytes, ticks;
                    long.TryParse(p[1], out bytes);
                    long.TryParse(p[2], out ticks);
                    var item = new Item
                    {
                        Relative = p[0],
                        Bytes = bytes,
                        Component = p[3],
                        Part = p[4],
                        Tool = p[5],
                        What = p[6],
                        Trio = p[7],
                        Peek = p[8]
                    };
                    map[item.Relative] = new CacheRow { Item = item, Bytes = bytes, Ticks = ticks };
                }
            }
            catch { }
            return map;
        }

        static void SaveCache(string folder, Dictionary<string, CacheRow> cache, List<Item> live)
        {
            try
            {
                string dir = Path.Combine(folder, ".specsb2");
                Directory.CreateDirectory(dir);
                var keep = new HashSet<string>(live.Select(i => i.Relative), StringComparer.OrdinalIgnoreCase);
                var sb = new StringBuilder();
                foreach (var kv in cache)
                {
                    if (!keep.Contains(kv.Key) || kv.Value.Item == null) continue;
                    var i = kv.Value.Item;
                    sb.Append(Clean(i.Relative)).Append('\t')
                      .Append(kv.Value.Bytes).Append('\t')
                      .Append(kv.Value.Ticks).Append('\t')
                      .Append(Clean(i.Component)).Append('\t')
                      .Append(Clean(i.Part)).Append('\t')
                      .Append(Clean(i.Tool)).Append('\t')
                      .Append(Clean(i.What)).Append('\t')
                      .Append(Clean(i.Trio)).Append('\t')
                      .Append(Clean(i.Peek)).AppendLine();
                }
                File.WriteAllText(Path.Combine(dir, "rip-index.txt"), sb.ToString());
            }
            catch { }
        }

        static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }

        public static string StorageText(string folder, long usedBytes)
        {
            double usedGb = usedBytes / 1073741824.0;
            try
            {
                string root = Path.GetPathRoot(Path.GetFullPath(folder));
                var drive = new DriveInfo(root);
                double freeGb = drive.AvailableFreeSpace / 1073741824.0;
                double totalGb = drive.TotalSize / 1073741824.0;
                return string.Format(
                    "Working folder {0:0.00} GB   ·   free on {1} {2:0.00} GB of {3:0.00} GB   ·   a full rip can be larger than the game pak",
                    usedGb, root, freeGb, totalGb);
            }
            catch
            {
                return string.Format("Working folder {0:0.00} GB", usedGb);
            }
        }

        static IEnumerable<string> EnumerateSafe(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                string name = Path.GetFileName(dir);
                if (name.Equals(".specsb2", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("node_modules", StringComparison.OrdinalIgnoreCase))
                    continue;
                string[] subs = null;
                string[] files = null;
                try { subs = Directory.GetDirectories(dir); } catch { }
                try { files = Directory.GetFiles(dir); } catch { }
                if (subs != null) foreach (string s in subs) stack.Push(s);
                if (files == null) continue;
                foreach (string f in files)
                {
                    if (f.EndsWith(ReadmeName, StringComparison.OrdinalIgnoreCase)) continue;
                    yield return f;
                }
            }
        }

        public static Item Inspect(string root, string file)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (!Interesting(ext)) return null;
            var item = new Item();
            item.FullPath = file;
            item.Relative = file.Substring(root.Length).TrimStart('\\', '/');
            try { item.Bytes = new FileInfo(file).Length; } catch { }
            item.Component = Classify(item.Relative);
            item.Part = PartOf(ext);
            item.Trio = TrioNote(file, ext);
            string tool, what;
            DetectTool(file, item.Relative, ext, out tool, out what);
            item.Tool = tool;
            item.What = what;
            item.Peek = Peek(file, ext);
            return item;
        }

        static bool Interesting(string ext)
        {
            switch (ext)
            {
                case ".uasset": case ".uexp": case ".ubulk": case ".umap":
                case ".locres": case ".locmeta": case ".ini": case ".json":
                case ".png": case ".tga": case ".dds": case ".bmp":
                case ".fbx": case ".obj": case ".gltf": case ".glb": case ".psk": case ".psa": case ".blend":
                case ".wav": case ".ogg": case ".bank": case ".xcf":
                case ".lua": case ".txt": case ".pak": case ".ct": case ".uplugin":
                    return true;
                default:
                    return false;
            }
        }

        public static string Classify(string relative)
        {
            string p = (relative ?? "").Replace('\\', '/');
            string low = p.ToLowerInvariant();
            if (low.Contains("helmet") || low.Contains("/hat") || low.Contains("headgear") || low.Contains("/mask"))
                return "Helmet";
            if (low.Contains("/armors/") || low.Contains("majorcharacters"))
                return "Armor";
            if (low.Contains("weapon") || low.Contains("rifle") || low.Contains("pistol") || low.Contains("ballistic") || low.Contains("/sword"))
                return "Weapon";
            if (low.Contains("packes") || low.Contains("/ship") || low.Contains("hull") || low.Contains("starterbundle"))
                return "Ship";
            if (low.Contains("/dialogs/") || low.Contains("dialog"))
                return "Dialogue";
            if (low.Contains("/1data/npcs") || low.Contains("/npcs/"))
                return "NPC";
            if (low.Contains("1galaxy") || low.Contains("galaxy"))
                return "Galaxy";
            if (low.Contains("shipai") || low.Contains("ship_ai") || low.Contains("/eve") ||
                (low.Contains("fmod") && (low.Contains("voice") || low.Contains("eve"))))
                return "EvE";
            if (low.Contains("fmod") || low.EndsWith(".bank") || low.EndsWith(".wav") || low.EndsWith(".ogg"))
                return "Audio";
            if (low.Contains("1widgets") || low.Contains("/ui/"))
                return "Interface";
            if (low.EndsWith(".locres") || low.Contains("localization"))
                return "Text";
            if (low.EndsWith(".ini") || low.Contains("/config/"))
                return "Config";
            if (low.EndsWith(".umap"))
                return "Map";
            if (low.Contains("material") || low.Contains("/mi_") || low.Contains("/m_"))
                return "Material";
            if (low.EndsWith(".png") || low.EndsWith(".tga") || low.EndsWith(".dds") || low.Contains("texture") || low.Contains("_textures"))
                return "Texture";
            if (low.EndsWith(".fbx") || low.EndsWith(".obj") || low.EndsWith(".psk") || low.EndsWith(".gltf") || low.EndsWith(".glb") || low.Contains("1meshs") || low.Contains("/mesh"))
                return "Mesh";
            if (low.Contains("locomotion") || low.Contains("anim"))
                return "Animation";
            return "Other";
        }

        static string PartOf(string ext)
        {
            switch (ext)
            {
                case ".uasset": return "Cooked header (names, exports)";
                case ".uexp": return "Cooked bulk (must stay with the .uasset)";
                case ".ubulk": return "Large bulk (must stay with the pair)";
                case ".umap": return "Cooked map";
                case ".pak": return "Package archive";
                default: return ext.TrimStart('.');
            }
        }

        static string TrioNote(string file, string ext)
        {
            if (ext != ".uasset" && ext != ".uexp" && ext != ".ubulk" && ext != ".umap") return "";
            string stem = ext == ".umap"
                ? Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileNameWithoutExtension(file))
                : Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileNameWithoutExtension(file));
            bool u = File.Exists(stem + ".uasset") || File.Exists(stem + ".umap");
            bool e = File.Exists(stem + ".uexp");
            bool b = File.Exists(stem + ".ubulk");
            if (u && e && b) return "Trio complete";
            if (u && e) return "Pair complete (no ubulk)";
            if (!e) return "Missing .uexp — do not ship this alone";
            if (!u) return "Missing .uasset — do not ship this alone";
            return "Incomplete";
        }

        public static void DetectTool(string file, string relative, string ext, out string tool, out string what)
        {
            string low = (relative ?? "").Replace('\\', '/').ToLowerInvariant();
            string folder = low;
            if (folder.Contains("/fmodel/") || folder.Contains("fmodeloutput") || folder.StartsWith("output/"))
            {
                tool = "FModel";
                what = "Exported from a pak by FModel";
                return;
            }
            if (ext == ".json")
            {
                string head = ReadHead(file, 4000);
                if (head.IndexOf("\"NameMap\"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tool = "UAssetGUI";
                    what = "Package saved as JSON (names and exports). Edit here, then write back with UAssetGUI.";
                    return;
                }
                if (head.IndexOf("\"PackageFlags\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    head.IndexOf("\"ExportType\"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tool = "FModel";
                    what = "JSON dump of one cooked export";
                    return;
                }
            }
            if (ext == ".psk" || ext == ".psa" || ext == ".pskx")
            {
                tool = "UModel";
                what = "Mesh or animation ripped to ActorX";
                return;
            }
            if (ext == ".blend")
            {
                tool = "Blender";
                what = "3D scene saved after an external edit";
                return;
            }
            if (ext == ".xcf")
            {
                tool = "GIMP";
                what = "Image project saved after an external edit";
                return;
            }
            if (ext == ".fbx" || ext == ".obj" || ext == ".gltf" || ext == ".glb")
            {
                tool = low.Contains("umodel") ? "UModel" : "3D tool";
                what = "Mesh file. The game still needs a cooked .uasset pair, not this file alone.";
                return;
            }
            if (ext == ".lua")
            {
                string head = ReadHead(file, 2000);
                if (head.IndexOf("RegisterHook", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    head.IndexOf("FindFirstOf", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tool = "UE4SS";
                    what = "Lua hook script. Drop it in a UE4SS Mods folder, not into a pak.";
                    return;
                }
                tool = "Text editor";
                what = "Lua file";
                return;
            }
            if (ext == ".ct")
            {
                tool = "Cheat Engine";
                what = "Memory table. This does not change pak files.";
                return;
            }
            if (ext == ".pak")
            {
                tool = low.Contains("_p.pak") ? "UnrealPak" : "Pak tool";
                what = low.Contains("_p.pak")
                    ? "Patch pack. The game loads z_*_P.pak over the base pak."
                    : "Pak archive. Do not replace the game's base pak.";
                return;
            }
            if (ext == ".locres")
            {
                tool = "Locres tool";
                what = "Cooked text table. A .txt beside it usually means it was already exported.";
                return;
            }
            if (ext == ".txt")
            {
                string side = Path.ChangeExtension(file, ".locres");
                if (File.Exists(side) || low.Contains("locres"))
                {
                    tool = "Locres tool";
                    what = "Plain-text export of game strings";
                    return;
                }
            }
            if (ext == ".png" || ext == ".tga" || ext == ".dds")
            {
                tool = "Image tool";
                what = "Texture ripped out of a material. Recook before it can override the game.";
                return;
            }
            if (ext == ".uasset" || ext == ".uexp" || ext == ".ubulk" || ext == ".umap")
            {
                tool = "Pak extract";
                what = "Cooked game file copied out of a pak. Safe to edit only in this working folder.";
                return;
            }
            if (ext == ".ini")
            {
                tool = "Config edit";
                what = "Engine or game ini. Ship as a config override, not by editing the install.";
                return;
            }
            tool = "Unknown";
            what = "Ripped file. Open it from this folder, not from the game install.";
        }

        public static string Peek(string file, string ext)
        {
            try
            {
                if (ext == ".uasset" || ext == ".umap") return PeekPackage(file);
                if (ext == ".locres") return PeekLocres(file);
                if (ext == ".pak") return PeekPak(file);
                if (ext == ".ini") return PeekIni(file);
                if (ext == ".png") return "PNG image";
                if (ext == ".json") return "JSON, " + (new FileInfo(file).Length / 1024) + " KB";
            }
            catch (Exception ex) { return ex.Message; }
            return "";
        }

        static string PeekPackage(string file)
        {
            byte[] b = ReadBytes(file, 16);
            if (b.Length < 8) return "Too small to be a package";
            uint magic = BitConverter.ToUInt32(b, 0);
            int legacy = BitConverter.ToInt32(b, 4);
            if (magic != 0x9E2A83C1)
                return "Not a UE4 package header (magic " + magic.ToString("X8") + ")";
            return "UE4 package, legacy version " + legacy + ". In-app reader stops at the header; full exports still need UAssetGUI.";
        }

        static string PeekLocres(string file)
        {
            byte[] b = ReadBytes(file, 8);
            if (b.Length < 4) return "Empty locres";
            uint magic = BitConverter.ToUInt32(b, 0);
            if (magic == 0x7574140E || magic == 0x0E145475)
                return "UE locres text table. Strings can be exported beside this file. Repack before shipping.";
            return "Locres magic not recognized (" + magic.ToString("X8") + ")";
        }

        static string PeekPak(string file)
        {
            const int footer = 44;
            var fi = new FileInfo(file);
            if (fi.Length < footer) return "Too small to be a pak";
            byte[] tail = new byte[footer];
            using (var fs = File.OpenRead(file))
            {
                fs.Seek(-footer, SeekOrigin.End);
                fs.Read(tail, 0, footer);
            }
            uint magic = BitConverter.ToUInt32(tail, footer - 4);
            // magic sits at the start of the footer in some versions; check both ends
            uint magic0 = BitConverter.ToUInt32(tail, 0);
            if (magic == 0x5A6F12E1 || magic0 == 0x5A6F12E1)
                return "UE pak footer found. Index is inside the archive. Mount point for this game is ../../../ . Do not recompress with anything but Oodle if you rebuild.";
            return "Pak footer magic not found. The file may be encrypted or incomplete. This game's main pak is not encrypted.";
        }

        static string PeekIni(string file)
        {
            int sections = 0;
            foreach (string line in File.ReadLines(file).Take(400))
                if (line.TrimStart().StartsWith("[")) sections++;
            return sections + " ini sections in the first 400 lines";
        }

        static string ReadHead(string file, int max)
        {
            try
            {
                byte[] b = ReadBytes(file, max);
                return Encoding.UTF8.GetString(b);
            }
            catch { return ""; }
        }

        static byte[] ReadBytes(string file, int max)
        {
            using (var fs = File.OpenRead(file))
            {
                int n = (int)Math.Min(max, fs.Length);
                byte[] b = new byte[n];
                fs.Read(b, 0, n);
                return b;
            }
        }

        public static void ImportTree(string source, string workingRoot, bool replaceChanged, out int copied, out int skipped, out string error)
        {
            copied = 0;
            skipped = 0;
            error = null;
            string why;
            if (!IsSafeWorkingFolder(workingRoot, out why)) { error = why; return; }
            if (!Directory.Exists(source)) { error = "Source folder not found."; return; }
            string src = Path.GetFullPath(source);
            string game = ConfigService.Game != null ? ConfigService.Game.InstallPath : null;
            if (!string.IsNullOrWhiteSpace(game))
            {
                string g = Path.GetFullPath(game).TrimEnd('\\', '/');
                if (src.StartsWith(g, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Import from a ripped folder (FModel output, UModel export), not from the game install.";
                    return;
                }
            }
            foreach (string file in EnumerateSafe(src))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (!Interesting(ext)) continue;
                string rel = file.Substring(src.Length).TrimStart('\\', '/');
                string dest = Path.Combine(workingRoot, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? workingRoot);
                if (File.Exists(dest))
                {
                    bool same = false;
                    try { same = new FileInfo(dest).Length == new FileInfo(file).Length; } catch { }
                    if (same || !replaceChanged) { skipped++; continue; }
                }
                File.Copy(file, dest, true);
                copied++;
            }
        }

        public static string CopyIntoOpenMod(Item item, out string error)
        {
            error = null;
            if (item == null || !File.Exists(item.FullPath)) { error = "File missing."; return null; }
            if (!CurrentModContext.IsOpen) { error = "Open a mod project first."; return null; }
            string rel = item.Relative.Replace('\\', '/');
            int cut = rel.IndexOf("SB2/Content/", StringComparison.OrdinalIgnoreCase);
            string under;
            if (cut >= 0) under = rel.Substring(cut + "SB2/Content/".Length);
            else
            {
                int c2 = rel.IndexOf("Content/", StringComparison.OrdinalIgnoreCase);
                under = c2 >= 0 ? rel.Substring(c2 + "Content/".Length) : rel;
            }
            string dest = Path.Combine(CurrentModContext.StagingContentDir, under.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(dest))
            {
                error = "ALREADY:" + dest;
                return dest;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? CurrentModContext.ModRoot);
            File.Copy(item.FullPath, dest, false);
            return dest;
        }
    }
}
