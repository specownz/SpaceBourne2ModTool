using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Safe scaffolding into an open mod: create folders freely, merge text lists,
    /// and prompt before overwriting existing non-empty files with different content.
    /// </summary>
    public static class ScaffoldMergeService
    {
        public class Report
        {
            public int FoldersCreated;
            public int FilesWritten;
            public int FilesSkippedSame;
            public int FilesSkippedUser;
            public int LinesMerged;
            public readonly List<string> Details = new List<string>();
            public override string ToString()
            {
                return "Folders created: " + FoldersCreated
                    + "\nFiles written: " + FilesWritten
                    + "\nUnchanged (same content): " + FilesSkippedSame
                    + "\nSkipped (you kept existing): " + FilesSkippedUser
                    + "\nList lines merged: " + LinesMerged;
            }
        }

        /// <summary>Ensure directory exists. Always safe (merge).</summary>
        public static void EnsureDir(string path, Report r)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (r != null) { r.FoldersCreated++; r.Details.Add("dir+ " + path); }
            }
        }

        /// <summary>
        /// Write text file. If missing → write. If same content → skip.
        /// If different → MessageBox Yes/No with preview of what will be replaced.
        /// </summary>
        public static bool WriteTextMerged(IWin32Window owner, string path, string newContent, Report r, string purpose)
        {
            if (string.IsNullOrEmpty(path)) return false;
            newContent = newContent ?? "";
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) EnsureDir(dir, r);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, newContent, Encoding.UTF8);
                if (r != null) { r.FilesWritten++; r.Details.Add("new  " + path); }
                return true;
            }

            string existing;
            try { existing = File.ReadAllText(path); }
            catch
            {
                if (r != null) r.Details.Add("lock " + path);
                return false;
            }

            if (Normalize(existing) == Normalize(newContent))
            {
                if (r != null) { r.FilesSkippedSame++; r.Details.Add("same " + path); }
                return false;
            }

            string previewOld = Truncate(existing, 400);
            string previewNew = Truncate(newContent, 400);
            string msg =
                "This scaffold wants to replace an existing file.\r\n\r\n"
                + "File: " + path + "\r\n"
                + (string.IsNullOrEmpty(purpose) ? "" : "Purpose: " + purpose + "\r\n")
                + "\r\n—— Current file (excerpt) ——\r\n" + previewOld
                + "\r\n\r\n—— New content (excerpt) ——\r\n" + previewNew
                + "\r\n\r\nReplace the existing file?\r\n"
                + "Yes = overwrite · No = keep your current file";

            var result = MessageBox.Show(owner, msg, "Scaffold — confirm overwrite",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                if (r != null) { r.FilesSkippedUser++; r.Details.Add("kept " + path); }
                return false;
            }

            // Backup once beside the file
            try
            {
                string bak = path + ".bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Copy(path, bak, false);
            }
            catch { }

            File.WriteAllText(path, newContent, Encoding.UTF8);
            if (r != null) { r.FilesWritten++; r.Details.Add("over " + path); }
            return true;
        }

        /// <summary>Append unique non-empty lines to a list file (TARGET_PATHS style).</summary>
        public static void MergeLines(string path, IEnumerable<string> lines, Report r)
        {
            if (string.IsNullOrEmpty(path) || lines == null) return;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) EnsureDir(dir, r);

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(path))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(path))
                        if (!string.IsNullOrWhiteSpace(line)) set.Add(line.Trim());
                }
                catch { }
            }

            var add = new List<string>();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string t = line.Trim();
                if (set.Add(t)) add.Add(t);
            }
            if (add.Count == 0) return;

            var sb = new StringBuilder();
            if (File.Exists(path))
            {
                try { sb.Append(File.ReadAllText(path)); if (!sb.ToString().EndsWith("\n") && sb.Length > 0) sb.AppendLine(); }
                catch { }
            }
            foreach (string a in add) sb.AppendLine(a);
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            if (r != null) { r.LinesMerged += add.Count; r.Details.Add("merge+" + add.Count + " " + path); }
        }

        public static void EnsureContentFolder(string modRoot, string relativeUnderContent, Report r)
        {
            if (string.IsNullOrEmpty(modRoot) || string.IsNullOrEmpty(relativeUnderContent)) return;
            string rel = relativeUnderContent.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            // movies special ../Movies handled by caller
            string full = Path.Combine(modRoot, "SB2", "Content", rel);
            EnsureDir(full, r);
        }

        static string Normalize(string s)
        {
            if (s == null) return "";
            return s.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }

        static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "(empty)";
            s = s.Replace("\r\n", "\n");
            if (s.Length <= max) return s;
            return s.Substring(0, max) + "\n…";
        }
    }
}
