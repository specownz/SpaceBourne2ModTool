using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>Dialog/quest script text. Binary conversation graphs stay in .uasset; this holds editable text + game paths.</summary>
    public class ScriptDocument
    {
        public string Kind { get; set; } = "Dialog";
        public string Category { get; set; } = "Uncategorized";
        public string Title { get; set; } = "Untitled";
        public string GamePath { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

        public string FileName
        {
            get
            {
                string safe = Category + "_" + Title;
                foreach (char c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
                return Kind + "__" + safe + ".script.txt";
            }
        }

        public void Save(string path)
        {
            ModifiedUtc = DateTime.UtcNow;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path,
                "## kind: " + Kind + "\n## category: " + Category + "\n## title: " + Title +
                "\n## gamepath: " + GamePath + "\n## modified: " + ModifiedUtc.ToString("o") + "\n## ---\n" + (Body ?? ""),
                Encoding.UTF8);
        }

        public static ScriptDocument Load(string path)
        {
            var doc = new ScriptDocument();
            var body = new StringBuilder();
            bool inBody = false;
            foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
            {
                if (!inBody)
                {
                    if (line.StartsWith("## ---")) { inBody = true; continue; }
                    if (line.StartsWith("## kind:", StringComparison.OrdinalIgnoreCase)) doc.Kind = line.Substring(8).Trim();
                    else if (line.StartsWith("## category:", StringComparison.OrdinalIgnoreCase)) doc.Category = line.Substring(12).Trim();
                    else if (line.StartsWith("## title:", StringComparison.OrdinalIgnoreCase)) doc.Title = line.Substring(9).Trim();
                    else if (line.StartsWith("## gamepath:", StringComparison.OrdinalIgnoreCase)) doc.GamePath = line.Substring(12).Trim();
                    else if (!line.StartsWith("##")) { inBody = true; body.AppendLine(line); }
                }
                else body.AppendLine(line);
            }
            doc.Body = body.ToString().TrimEnd();
            if (string.IsNullOrEmpty(doc.Title)) doc.Title = Path.GetFileNameWithoutExtension(path);
            return doc;
        }

        public static List<ScriptDocument> DiscoverFromAnalysisReports(string reportsRoot)
        {
            var list = new List<ScriptDocument>();
            if (string.IsNullOrEmpty(reportsRoot) || !Directory.Exists(reportsRoot)) return list;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in new[] { "SB2_ContentMap_LATEST.txt", "SB2_AuthorFeedback_LATEST.txt", "SB2_ModTargets_LATEST.txt" })
            {
                string f = Path.Combine(reportsRoot, name);
                if (!File.Exists(f)) continue;
                int n = 0;
                try
                {
                    foreach (string raw in File.ReadLines(f))
                    {
                        if (++n > 200000) break;
                        string line = raw.Trim();
                        if (line.IndexOf("Dialogs", StringComparison.OrdinalIgnoreCase) < 0 &&
                            line.IndexOf("Quests", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        string path = ExtractPath(line);
                        if (string.IsNullOrEmpty(path) || !seen.Add(path)) continue;
                        string kind = path.IndexOf("Quest", StringComparison.OrdinalIgnoreCase) >= 0 ? "Quest" : "Dialog";
                        string category = "General", title = Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));
                        int di = path.IndexOf("Dialogs/", StringComparison.OrdinalIgnoreCase);
                        if (di >= 0)
                        {
                            string rest = path.Substring(di + 8);
                            int slash = rest.IndexOf('/');
                            if (slash > 0) { category = rest.Substring(0, slash); title = Path.GetFileNameWithoutExtension(rest); }
                            else category = rest;
                        }
                        int qi = path.IndexOf("Quests/", StringComparison.OrdinalIgnoreCase);
                        if (qi >= 0)
                        {
                            kind = "Quest";
                            string rest = path.Substring(qi + 7);
                            int slash = rest.IndexOf('/');
                            category = slash > 0 ? rest.Substring(0, slash) : rest;
                        }
                        list.Add(new ScriptDocument
                        {
                            Kind = kind, Category = category, Title = title,
                            GamePath = path.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase) ? path : "SB2/Content/" + path.TrimStart('/'),
                            Body = "; Stub from analysis — export with FModel/UAssetGUI\r\n; GamePath: " + path + "\r\n\r\n[lines]\r\n"
                        });
                        if (list.Count >= 500) return list;
                    }
                }
                catch { }
            }
            return list;
        }

        static string ExtractPath(string line)
        {
            int i = line.IndexOf("SB2/Content/", StringComparison.OrdinalIgnoreCase);
            if (i < 0) i = line.IndexOf("1Data/", StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int end = i;
            while (end < line.Length)
            {
                char c = line[end];
                if (char.IsLetterOrDigit(c) || c == '/' || c == '_' || c == '-' || c == '.' || c == ' ') end++;
                else break;
            }
            string p = line.Substring(i, end - i).Trim();
            return (p.EndsWith(".uexp") || p.EndsWith(".ubulk")) ? null : p;
        }
    }
}
