using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Helpers for the easiest SB2 mods: replace files that already live on disk
    /// outside the monolithic pak (Content/Movies, Content/FMOD).
    /// Analyzer found ~82 .mp4 under Movies and FMOD banks on a real install.
    /// </summary>
    public static class LooseModHelper
    {
        public static string GetMoviesPath(string installPath)
        {
            if (string.IsNullOrEmpty(installPath)) return null;
            string p = Path.Combine(installPath, "SB2", "Content", "Movies");
            return Directory.Exists(p) ? p : null;
        }

        public static string GetFmodPath(string installPath)
        {
            if (string.IsNullOrEmpty(installPath)) return null;
            string p = Path.Combine(installPath, "SB2", "Content", "FMOD");
            return Directory.Exists(p) ? p : null;
        }

        public static List<FileInfo> ListFiles(string folder, string searchPattern = "*.*")
        {
            var list = new List<FileInfo>();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return list;
            try
            {
                foreach (string f in Directory.GetFiles(folder, searchPattern, SearchOption.AllDirectories))
                    list.Add(new FileInfo(f));
            }
            catch (Exception ex)
            {
                LoggerService.LogError("LooseModHelper.ListFiles", ex);
            }
            return list.OrderBy(f => f.FullName).ToList();
        }

        /// <summary>
        /// Backup original then copy replacement into place. Returns backup path.
        /// </summary>
        public static string ReplaceWithBackup(string targetFile, string sourceFile)
        {
            if (!File.Exists(targetFile))
                throw new FileNotFoundException("Target not found", targetFile);
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Source not found", sourceFile);

            string backupDir = Path.Combine(Path.GetDirectoryName(targetFile) ?? "", "_SB2ModBackup");
            FileService.EnsureDirectory(backupDir);
            string backupName = Path.GetFileName(targetFile) + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
            string backupPath = Path.Combine(backupDir, backupName);
            if (!File.Exists(backupPath))
                File.Copy(targetFile, backupPath, false);

            File.Copy(sourceFile, targetFile, true);
            LoggerService.Log("Loose replace: " + targetFile + " <= " + sourceFile + " (backup " + backupPath + ")");
            return backupPath;
        }

        public static int RestoreBackups(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return 0;
            string backupDir = Path.Combine(folder, "_SB2ModBackup");
            if (!Directory.Exists(backupDir)) return 0;
            int n = 0;
            foreach (string bak in Directory.GetFiles(backupDir, "*.bak"))
            {
                // name form: originalname.ext.yyyyMMdd_HHmmss.bak
                string name = Path.GetFileName(bak);
                int idx = name.IndexOf(".20"); // start of timestamp-ish
                // simpler: strip last two extensions .timestamp.bak
                string withoutBak = name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)
                    ? name.Substring(0, name.Length - 4) : name;
                // remove .yyyyMMdd_HHmmss
                int dot = withoutBak.LastIndexOf('.');
                if (dot > 0 && withoutBak.Length - dot == 16) // .yyyyMMdd_HHmmss approx
                    withoutBak = withoutBak.Substring(0, dot);
                // actually our format is file.ext.yyyyMMdd_HHmmss.bak
                // so remove .\d{8}_\d{6}
                var m = System.Text.RegularExpressions.Regex.Match(name, @"^(.+\.[^.]+)\.\d{8}_\d{6}\.bak$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!m.Success) continue;
                string originalName = m.Groups[1].Value;
                string dest = Path.Combine(folder, originalName);
                // if nested, search
                var matches = Directory.GetFiles(folder, originalName, SearchOption.AllDirectories);
                dest = matches.Length > 0 ? matches[0] : Path.Combine(folder, originalName);
                File.Copy(bak, dest, true);
                n++;
            }
            return n;
        }
    }
}
