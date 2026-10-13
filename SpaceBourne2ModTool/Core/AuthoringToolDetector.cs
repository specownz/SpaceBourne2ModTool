using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Detects popular code / image / 3D authoring tools.
    /// Preference: Paid software ranked above Free; configurable via PreferPaidTools.
    /// Fallbacks: Notepad, GIMP, Blender.
    /// </summary>
    public static class AuthoringToolDetector
    {
        public enum ToolKind { CodeEditor, ImageEditor, Modeling3d }

        public class Candidate
        {
            public string Name { get; set; }
            public string ExeName { get; set; }
            public bool IsPaid { get; set; }
            public int Rank { get; set; } // lower = higher priority within paid/free band
            public string[] ExtraPathHints { get; set; }
        }

        // Paid preferred when PreferPaidTools is true (default).
        public static readonly Candidate[] CodeEditors =
        {
            new Candidate { Name = "Sublime Text", ExeName = "sublime_text.exe", IsPaid = true, Rank = 1,
                ExtraPathHints = new[] { @"Sublime Text", @"Sublime Text 4", @"Sublime Text 3" } },
            new Candidate { Name = "Visual Studio", ExeName = "devenv.exe", IsPaid = true, Rank = 2,
                ExtraPathHints = new[] { @"Microsoft Visual Studio" } },
            new Candidate { Name = "JetBrains Rider", ExeName = "rider64.exe", IsPaid = true, Rank = 3,
                ExtraPathHints = new[] { @"JetBrains" } },
            new Candidate { Name = "VS Code", ExeName = "Code.exe", IsPaid = false, Rank = 10,
                ExtraPathHints = new[] { @"Microsoft VS Code", @"Microsoft\VS Code" } },
            new Candidate { Name = "Notepad++", ExeName = "notepad++.exe", IsPaid = false, Rank = 11,
                ExtraPathHints = new[] { @"Notepad++" } },
            new Candidate { Name = "Notepad", ExeName = "notepad.exe", IsPaid = false, Rank = 100,
                ExtraPathHints = new string[0] },
        };

        public static readonly Candidate[] ImageEditors =
        {
            new Candidate { Name = "Adobe Photoshop", ExeName = "Photoshop.exe", IsPaid = true, Rank = 1,
                ExtraPathHints = new[] { @"Adobe", @"Adobe Photoshop" } },
            new Candidate { Name = "Affinity Photo", ExeName = "Photo.exe", IsPaid = true, Rank = 2,
                ExtraPathHints = new[] { @"Affinity", @"Affinity Photo" } },
            new Candidate { Name = "Paint.NET", ExeName = "paintdotnet.exe", IsPaid = false, Rank = 10,
                ExtraPathHints = new[] { @"paint.net" } },
            new Candidate { Name = "GIMP", ExeName = "gimp-2.10.exe", IsPaid = false, Rank = 11,
                ExtraPathHints = new[] { @"GIMP 2", @"GIMP" } },
            new Candidate { Name = "GIMP (bin)", ExeName = "gimp.exe", IsPaid = false, Rank = 12,
                ExtraPathHints = new[] { @"GIMP 2", @"GIMP" } },
        };

        public static readonly Candidate[] ModelingTools =
        {
            new Candidate { Name = "Autodesk Maya", ExeName = "maya.exe", IsPaid = true, Rank = 1,
                ExtraPathHints = new[] { @"Autodesk", @"Maya" } },
            new Candidate { Name = "3ds Max", ExeName = "3dsmax.exe", IsPaid = true, Rank = 2,
                ExtraPathHints = new[] { @"Autodesk", @"3ds Max" } },
            new Candidate { Name = "Cinema 4D", ExeName = "Cinema 4D.exe", IsPaid = true, Rank = 3,
                ExtraPathHints = new[] { @"Maxon", @"Cinema 4D" } },
            new Candidate { Name = "Houdini", ExeName = "houdini.exe", IsPaid = true, Rank = 4,
                ExtraPathHints = new[] { @"Side Effects Software", @"Houdini" } },
            new Candidate { Name = "Blender", ExeName = "blender.exe", IsPaid = false, Rank = 10,
                ExtraPathHints = new[] { @"Blender Foundation", @"Blender" } },
        };

        public class DetectResult
        {
            public string CodeEditorPath { get; set; }
            public string CodeEditorName { get; set; }
            public string ImageEditorPath { get; set; }
            public string ImageEditorName { get; set; }
            public string ModelingPath { get; set; }
            public string ModelingName { get; set; }
            public List<string> Log { get; set; } = new List<string>();
        }

        public static DetectResult Detect(Action<string> progress = null, Action<string, bool, string> itemProgress = null)
        {
            var r = new DetectResult();
            void P(string m) { r.Log.Add(m); progress?.Invoke(m); }

            bool preferPaid = ConfigService.PreferPaidTools;
            P("Authoring tools — prefer paid: " + preferPaid);

            itemProgress?.Invoke("CodeEditor", false, "Searching…");
            var code = FindBest(CodeEditors, preferPaid, P);
            if (code.Item1 != null)
            {
                r.CodeEditorPath = code.Item1;
                r.CodeEditorName = code.Item2;
                itemProgress?.Invoke("CodeEditor", true, code.Item1);
            }
            else
            {
                r.CodeEditorPath = ResolveNotepad();
                r.CodeEditorName = "Notepad";
                itemProgress?.Invoke("CodeEditor", !string.IsNullOrEmpty(r.CodeEditorPath), r.CodeEditorPath ?? "not found");
            }

            itemProgress?.Invoke("ImageEditor", false, "Searching…");
            var img = FindBest(ImageEditors, preferPaid, P);
            if (img.Item1 != null)
            {
                r.ImageEditorPath = img.Item1;
                r.ImageEditorName = img.Item2;
                itemProgress?.Invoke("ImageEditor", true, img.Item1);
            }
            else
            {
                // GIMP fallback name search already in list; leave empty if none
                itemProgress?.Invoke("ImageEditor", false, "not found (install GIMP)");
            }

            itemProgress?.Invoke("Modeling3d", false, "Searching…");
            var mdl = FindBest(ModelingTools, preferPaid, P);
            if (mdl.Item1 != null)
            {
                r.ModelingPath = mdl.Item1;
                r.ModelingName = mdl.Item2;
                itemProgress?.Invoke("Modeling3d", true, mdl.Item1);
            }
            else
            {
                itemProgress?.Invoke("Modeling3d", false, "not found (install Blender)");
            }

            return r;
        }

        public static void ApplyToConfig(DetectResult r)
        {
            if (!string.IsNullOrEmpty(r.CodeEditorPath) && File.Exists(r.CodeEditorPath))
                ConfigService.CodeEditorPath = r.CodeEditorPath;
            if (!string.IsNullOrEmpty(r.ImageEditorPath) && File.Exists(r.ImageEditorPath))
                ConfigService.ImageEditorPath = r.ImageEditorPath;
            if (!string.IsNullOrEmpty(r.ModelingPath) && File.Exists(r.ModelingPath))
                ConfigService.ModelingToolPath = r.ModelingPath;
            ConfigService.Save();
        }

        private static Tuple<string, string> FindBest(Candidate[] list, bool preferPaid, Action<string> log)
        {
            IEnumerable<Candidate> ordered = preferPaid
                ? list.OrderByDescending(c => c.IsPaid).ThenBy(c => c.Rank)
                : list.OrderBy(c => c.Rank);

            var roots = BuildSearchRoots();
            foreach (var c in ordered)
            {
                // Special-case notepad in System32
                if (c.ExeName.Equals("notepad.exe", StringComparison.OrdinalIgnoreCase))
                {
                    string np = ResolveNotepad();
                    if (!string.IsNullOrEmpty(np))
                    {
                        log("  Found " + c.Name + ": " + np);
                        return Tuple.Create(np, c.Name);
                    }
                    continue;
                }

                // Hint folders first
                foreach (string root in roots)
                {
                    foreach (string hint in c.ExtraPathHints ?? new string[0])
                    {
                        string dir = Path.Combine(root, hint);
                        if (!Directory.Exists(dir)) continue;
                        string hit = FindFileRecursive(dir, c.ExeName, maxDepth: 5);
                        if (hit != null)
                        {
                            log("  Found " + c.Name + " (" + (c.IsPaid ? "paid" : "free") + "): " + hit);
                            return Tuple.Create(hit, c.Name);
                        }
                    }
                }

                // Broader name search
                foreach (string root in roots.Take(12))
                {
                    string hit = FindFileRecursive(root, c.ExeName, maxDepth: 4);
                    if (hit != null)
                    {
                        log("  Found " + c.Name + ": " + hit);
                        return Tuple.Create(hit, c.Name);
                    }
                }
            }
            return Tuple.Create<string, string>(null, null);
        }

        private static string ResolveNotepad()
        {
            string sys = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "notepad.exe");
            if (File.Exists(sys)) return sys;
            string w = Path.Combine(Environment.SystemDirectory, "notepad.exe");
            return File.Exists(w) ? w : null;
        }

        private static List<string> BuildSearchRoots()
        {
            var roots = new List<string>();
            void Add(string p)
            {
                if (!string.IsNullOrEmpty(p) && Directory.Exists(p) && !roots.Contains(p, StringComparer.OrdinalIgnoreCase))
                    roots.Add(p);
            }
            Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Add(Path.Combine(local, "Programs"));
            Add(@"C:\Program Files");
            Add(@"C:\Program Files (x86)");
            Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Programs"));
            // Common portable / Tools
            Add(@"C:\Tools");
            Add(@"C:\dev");
            Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Tools"));
            return roots;
        }

        private static string FindFileRecursive(string root, string fileName, int maxDepth)
        {
            try
            {
                return FindRecursive(root, fileName, 0, maxDepth);
            }
            catch { return null; }
        }

        private static string FindRecursive(string dir, string fileName, int depth, int maxDepth)
        {
            if (depth > maxDepth || string.IsNullOrEmpty(dir)) return null;
            try
            {
                foreach (string f in SafeDirectory.GetFiles(dir, fileName))
                    return f;
            }
            catch { }
            if (depth == maxDepth) return null;
            try
            {
                foreach (string sub in SafeDirectory.GetDirectories(dir))
                {
                    string name = Path.GetFileName(sub);
                    if (name.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("WinSxS", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("$", StringComparison.Ordinal))
                        continue;
                    string hit = FindRecursive(sub, fileName, depth + 1, maxDepth);
                    if (hit != null) return hit;
                }
            }
            catch { }
            return null;
        }
    }
}
