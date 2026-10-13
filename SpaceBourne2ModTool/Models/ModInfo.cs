using System;
using System.Collections.Generic;

namespace SpaceBourne2ModTool.Models
{
    /// <summary>
    /// Represents metadata and configuration for a SpaceBourne 2 mod project.
    /// </summary>
    public class ModInfo
    {
        public string Name { get; set; } = "NewMod";
        public string Author { get; set; } = Environment.UserName;
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = "";
        public string GameVersionTarget { get; set; } = "1.0+";
        public int ChunkId { get; set; } = 1000;
        public string MountPoint { get; set; } = "../../../";  // Standard UE mount
        public ModType Type { get; set; } = ModType.AssetReplacement;
        public List<string> Files { get; set; } = new List<string>();
        public Dictionary<string, string> ExtraMetadata { get; set; } = new Dictionary<string, string>();
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;

        public string GetPakFileName()
        {
            // Real game ships a single SB2-WindowsNoEditor.pak (~11GB).
            // Mods: z_ prefix sorts after "SB2...", _P enables patch override priority.
            return Core.GameInsights.RecommendModPakFileName(Name, ChunkId);
        }

        public string GetModFolderName()
        {
            return $"{SanitizeName(Name)}_P";
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Mod";
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(" ", "_");
        }
    }

    public enum ModType
    {
        AssetReplacement,   // Textures, meshes, materials (most common)
        Localization,       // .locres language packs
        ConfigOverride,     // INI / config tweaks
        LooseFiles,         // Drop-in Content folder files
        TrainerTemplate,    // Cheat Engine / WeMod style helpers
        Hybrid              // Combination of the above
    }
}
