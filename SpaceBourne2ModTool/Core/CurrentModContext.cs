using System;
using System.IO;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    public static class CurrentModContext
    {
        public static string ModRoot { get; private set; }
        public static ModInfo Info { get; private set; }
        public static bool IsOpen { get { return !string.IsNullOrEmpty(ModRoot) && Directory.Exists(ModRoot); } }
        public static event Action Changed;

        public static void Open(string modRoot)
        {
            if (string.IsNullOrWhiteSpace(modRoot) || !Directory.Exists(modRoot))
                throw new DirectoryNotFoundException("Mod folder not found: " + modRoot);
            ModRoot = Path.GetFullPath(modRoot);
            Info = new ModInfo { Name = Path.GetFileName(ModRoot) };
            string mi = Path.Combine(ModRoot, "ModInfo.json");
            if (File.Exists(mi))
            {
                try
                {
                    string json = File.ReadAllText(mi);
                    string name = JsonLite.GetString(json, "Name");
                    if (!string.IsNullOrEmpty(name)) Info.Name = name;
                    string author = JsonLite.GetString(json, "Author");
                    if (!string.IsNullOrEmpty(author)) Info.Author = author;
                    string ver = JsonLite.GetString(json, "Version");
                    if (!string.IsNullOrEmpty(ver)) Info.Version = ver;
                }
                catch { }
            }
            try { RecentModsStore.Add(ModRoot, Info.Name); } catch { }
            if (Changed != null) Changed.Invoke();
        }

        public static void Close()
        {
            ModRoot = null;
            Info = null;
            if (Changed != null) Changed.Invoke();
        }

        public static string ObjectsDir { get { return Dir("Objects"); } }
        public static string ScriptsDir { get { return Dir("Scripts"); } }
        public static string StagingContentDir { get { return Dir(Path.Combine("SB2", "Content")); } }

        public static string DisplayName
        {
            get { return !IsOpen ? "(no mod open)" : (Info != null ? Info.Name : Path.GetFileName(ModRoot)) + " — " + ModRoot; }
        }

        static string Dir(string relative)
        {
            if (!IsOpen) return null;
            string d = Path.Combine(ModRoot, relative);
            Directory.CreateDirectory(d);
            return d;
        }
    }
}
