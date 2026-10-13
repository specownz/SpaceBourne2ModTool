using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpaceBourne2ModTool.Services
{
    public static class FileService
    {
        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void CopyDirectory(string sourceDir, string destDir, bool overwrite = true)
        {
            EnsureDirectory(destDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest, overwrite);
            }
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destSub = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSub, overwrite);
            }
        }

        public static List<string> GetAllFilesRecursive(string root, string searchPattern = "*.*")
        {
            if (!Directory.Exists(root)) return new List<string>();
            return Directory.GetFiles(root, searchPattern, SearchOption.AllDirectories).ToList();
        }

        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                            .Sum(f => new FileInfo(f).Length);
        }

        public static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int i = 0;
            double size = bytes;
            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }
            return $"{size:0.##} {suffixes[i]}";
        }

        public static bool IsValidGamePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return false;
            // Look for SB2 folder or Paks
            string paks = Path.Combine(path, "SB2", "Content", "Paks");
            if (Directory.Exists(paks)) return true;
            paks = Path.Combine(path, "Content", "Paks");
            return Directory.Exists(paks);
        }
    }
}
