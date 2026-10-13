using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SpecFramework
{
    /// <summary>
    /// Speckage packaging for SpaceBourne 2 mods.
    /// A zip with manifest.json and a content folder.
    /// </summary>
    public static class SpeckagePack
    {
        public const string FormatId = "speckage_sb2_v1";
        public const string ManifestEntry = "manifest.json";
        public const string ContentPrefix = "content/";

        public static string CreateFromFolder(string sourceFolder, string outputSpeckagePath, SpeckageManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("Source folder not found: " + sourceFolder);
            if (manifest == null) manifest = new SpeckageManifest();

            manifest.Format = FormatId;
            manifest.CreatedUtc = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(manifest.Name))
                manifest.Name = Path.GetFileName(sourceFolder.TrimEnd('\\', '/'));
            manifest.Name = PathGuard.SanitizeFileName(manifest.Name);

            string outPath = outputSpeckagePath;
            if (string.IsNullOrWhiteSpace(outPath))
                outPath = Path.Combine(Path.GetDirectoryName(sourceFolder) ?? ".", manifest.Name + ".speckage");
            if (!outPath.EndsWith(".speckage", StringComparison.OrdinalIgnoreCase) &&
                !outPath.EndsWith(".sb2mod", StringComparison.OrdinalIgnoreCase))
                outPath += ".speckage";

            string outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
            if (File.Exists(outPath)) File.Delete(outPath);

            var files = new List<string>();
            using (var fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
                {
                    string rel = file.Substring(sourceFolder.Length).TrimStart('\\', '/').Replace('\\', '/');
                    if (!PathGuard.IsAllowedModPayloadExtension(file) &&
                        !rel.EndsWith("modinfo.json", StringComparison.OrdinalIgnoreCase) &&
                        !rel.EndsWith("TARGET_PATHS.txt", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Block packaging of entire base game pak if someone staged it by mistake
                    if (PathGuard.IsVanillaEditBlocked(Path.GetFileName(file)))
                        continue;

                    string entryName = ContentPrefix + rel;
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (var es = entry.Open())
                    using (var input = File.OpenRead(file))
                        input.CopyTo(es);
                    files.Add(rel);

                    string fn = Path.GetFileName(file);
                    if (fn.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!manifest.DeployedPakFiles.Contains(fn))
                            manifest.DeployedPakFiles.Add(fn);
                        if (string.IsNullOrEmpty(manifest.PakFileName))
                            manifest.PakFileName = fn;
                    }
                }
                manifest.Files = files;

                // manifest.json
                string json = SerializeManifest(manifest);
                var ment = zip.CreateEntry(ManifestEntry, CompressionLevel.Optimal);
                using (var es = ment.Open())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    es.Write(bytes, 0, bytes.Length);
                }
            }
            return outPath;
        }

        public static SpeckageManifest ReadManifest(string speckagePath)
        {
            using (var fs = File.OpenRead(speckagePath))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                var e = zip.GetEntry(ManifestEntry);
                if (e == null)
                    throw new InvalidDataException("Not a Speckage package (missing manifest.json).");
                using (var s = e.Open())
                using (var sr = new StreamReader(s, Encoding.UTF8))
                    return DeserializeManifest(sr.ReadToEnd());
            }
        }

        public static string ExtractToFolder(string speckagePath, string destinationFolder)
        {
            if (!File.Exists(speckagePath))
                throw new FileNotFoundException("Package not found", speckagePath);
            Directory.CreateDirectory(destinationFolder);

            using (var fs = File.OpenRead(speckagePath))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith("/")) continue;
                    string name = entry.FullName.Replace('\\', '/');
                    if (name.Contains("..")) throw new InvalidDataException("Path traversal in package entry: " + name);

                    string destRel;
                    if (name.StartsWith(ContentPrefix, StringComparison.OrdinalIgnoreCase))
                        destRel = name.Substring(ContentPrefix.Length);
                    else if (name.Equals(ManifestEntry, StringComparison.OrdinalIgnoreCase))
                        destRel = ManifestEntry;
                    else
                        continue;

                    if (string.IsNullOrEmpty(destRel)) continue;
                    string destPath = Path.Combine(destinationFolder, destRel.Replace('/', Path.DirectorySeparatorChar));
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);
                    if (!PathGuard.IsAllowedModPayloadExtension(destPath) &&
                        !destRel.Equals(ManifestEntry, StringComparison.OrdinalIgnoreCase) &&
                        !destRel.EndsWith("modinfo.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    using (var es = entry.Open())
                    using (var outs = File.Create(destPath))
                        es.CopyTo(outs);
                }
            }
            return destinationFolder;
        }

        public static string SerializeManifest(SpeckageManifest m)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Format\":\"").Append(Esc(m.Format)).Append("\",");
            sb.Append("\"Name\":\"").Append(Esc(m.Name)).Append("\",");
            sb.Append("\"Author\":\"").Append(Esc(m.Author)).Append("\",");
            sb.Append("\"Version\":\"").Append(Esc(m.Version)).Append("\",");
            sb.Append("\"Description\":\"").Append(Esc(m.Description)).Append("\",");
            sb.Append("\"GameId\":\"").Append(Esc(m.GameId)).Append("\",");
            sb.Append("\"DefaultLoadOrder\":").Append(m.DefaultLoadOrder).Append(",");
            sb.Append("\"PakFileName\":\"").Append(Esc(m.PakFileName)).Append("\",");
            sb.Append("\"MountPoint\":\"").Append(Esc(m.MountPoint)).Append("\"");
            sb.Append("}");
            return sb.ToString();
        }

        public static SpeckageManifest DeserializeManifest(string json)
        {
            var man = new SpeckageManifest();
            man.Name = ExtractJsonString(json, "Name") ?? man.Name;
            man.Author = ExtractJsonString(json, "Author") ?? "";
            man.Version = ExtractJsonString(json, "Version") ?? "1.0.0";
            man.PakFileName = ExtractJsonString(json, "PakFileName") ?? "";
            man.Description = ExtractJsonString(json, "Description") ?? "";
            string lo = ExtractJsonString(json, "DefaultLoadOrder");
            int n;
            if (!string.IsNullOrEmpty(lo) && int.TryParse(lo, out n)) man.DefaultLoadOrder = n;
            // also try unquoted number
            int idx = json.IndexOf("\"DefaultLoadOrder\"");
            if (idx >= 0)
            {
                int c = json.IndexOf(':', idx);
                int end = c + 1;
                while (end < json.Length && (char.IsDigit(json[end]) || json[end] == ' ' || json[end] == '-')) end++;
                int.TryParse(json.Substring(c + 1, end - c - 1).Trim(), out n);
                if (n > 0) man.DefaultLoadOrder = n;
            }
            return man;
        }

        private static string ExtractJsonString(string json, string key)
        {
            string needle = "\"" + key + "\"";
            int i = json.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int colon = json.IndexOf(':', i);
            int q1 = json.IndexOf('"', colon + 1);
            if (q1 < 0) return null;
            int q2 = json.IndexOf('"', q1 + 1);
            if (q2 < 0) return null;
            return json.Substring(q1 + 1, q2 - q1 - 1);
        }

        private static string Esc(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }
    }
}
