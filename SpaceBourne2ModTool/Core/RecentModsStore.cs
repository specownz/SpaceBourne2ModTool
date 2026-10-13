using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    public class RecentModEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string SavedAtUtc { get; set; }
    }

    /// <summary>
    /// Tracks recently saved/opened mod projects (simple pipe-delimited persistence).
    /// </summary>
    public static class RecentModsStore
    {
        private const int MaxEntries = 15;

        public static List<RecentModEntry> GetAll()
        {
            var list = new List<RecentModEntry>();
            try
            {
                string raw = ConfigService.RecentModsJson ?? "";
                // format: path||name||utc|path||name||utc
                if (string.IsNullOrWhiteSpace(raw) || raw == "[]")
                    return list;
                foreach (string chunk in raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = chunk.Split(new[] { "||" }, StringSplitOptions.None);
                    if (parts.Length >= 2)
                    {
                        list.Add(new RecentModEntry
                        {
                            Path = parts[0],
                            Name = parts[1],
                            SavedAtUtc = parts.Length >= 3 ? parts[2] : ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        public static void Add(string projectPath, string modName)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return;
            var list = GetAll();
            list.RemoveAll(x => string.Equals(x.Path, projectPath, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, new RecentModEntry
            {
                Path = projectPath,
                Name = modName ?? Path.GetFileName(projectPath),
                SavedAtUtc = DateTime.UtcNow.ToString("o")
            });
            if (list.Count > MaxEntries)
                list = list.Take(MaxEntries).ToList();
            Save(list);
        }

        public static void Save(List<RecentModEntry> list)
        {
            var sb = new StringBuilder();
            foreach (var e in list)
                sb.AppendLine(e.Path + "||" + e.Name + "||" + e.SavedAtUtc);
            ConfigService.RecentModsJson = sb.ToString().TrimEnd();
            ConfigService.Save();
        }

        public static void DiscoverFromDisk()
        {
            try
            {
                string root = ConfigService.ModsOutputPath;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
                var list = GetAll();
                var existing = new HashSet<string>(list.Select(x => x.Path), StringComparer.OrdinalIgnoreCase);
                foreach (string dir in Directory.GetDirectories(root))
                {
                    string info = Path.Combine(dir, "modinfo.json");
                    if (!File.Exists(info)) continue;
                    if (existing.Contains(dir)) continue;
                    list.Add(new RecentModEntry
                    {
                        Path = dir,
                        Name = Path.GetFileName(dir),
                        SavedAtUtc = File.GetLastWriteTimeUtc(info).ToString("o")
                    });
                }
                list = list.OrderByDescending(x => x.SavedAtUtc).Take(MaxEntries).ToList();
                Save(list);
            }
            catch { }
        }
    }
}
