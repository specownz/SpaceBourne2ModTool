using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Manages the lifecycle of a single mod project: folder structure, metadata,
    /// file staging, and packaging preparation for SpaceBourne 2 (UE 4.27).
    ///
    /// Standard mod layout for override / patch-pack style:
    ///   {ModName}_P\
    ///       SB2\
    ///           Content\
    ///               ... (mirrors game Content paths)
    /// </summary>
    public class ModProject
    {
        public ModInfo Info { get; private set; }
        public string ProjectRoot { get; private set; }
        public string StagingRoot { get; private set; }  // the {Name}_P folder

        public ModProject(ModInfo info, string baseOutputPath)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            ProjectRoot = Path.Combine(baseOutputPath, Info.Name);
            StagingRoot = Path.Combine(ProjectRoot, Info.GetModFolderName());
        }

        public void CreateStructure()
        {
            FileService.EnsureDirectory(ProjectRoot);
            FileService.EnsureDirectory(StagingRoot);

            // Core content hierarchy from real analyzer map (SB2/Content/...)
            string content = Path.Combine(StagingRoot, "SB2", "Content");
            FileService.EnsureDirectory(content);
            FileService.EnsureDirectory(Path.Combine(content, "Localization", "Game"));

            // Preset folders based on mod type / name keywords
            string preset = InferPreset(Info);
            foreach (string folder in GameInsights.GetScaffoldFoldersForPreset(preset))
            {
                FileService.EnsureDirectory(Path.Combine(content, folder.Replace('/', Path.DirectorySeparatorChar)));
            }

            // Config override staging (optional)
            FileService.EnsureDirectory(Path.Combine(StagingRoot, "SB2", "Config"));

            // Placeholder README inside each high-value folder
            WriteFolderPlaceholders(content, preset);

            WriteModInfoJson();
            WriteInstallReadme();

            File.WriteAllText(
                Path.Combine(ProjectRoot, "UE427_Modding_Practices.txt"),
                ModdingPractices.GetFullGuide(),
                Encoding.UTF8);

            File.WriteAllText(
                Path.Combine(ProjectRoot, "Packaging_Checklist.txt"),
                PakHelper.GetPackagingChecklist(),
                Encoding.UTF8);

            File.WriteAllText(
                Path.Combine(ProjectRoot, "SB2_Content_Folders_Guide.txt"),
                GameInsights.GetLayoutSummary(),
                Encoding.UTF8);

            LoggerService.Log($"Created mod project structure at {ProjectRoot} (preset={preset})");
        }

        private static string InferPreset(ModInfo info)
        {
            string n = ((info.Name ?? "") + " " + (info.Description ?? "")).ToLowerInvariant();
            if (info.Type == ModType.Localization) return "localization";
            if (n.Contains("npc")) return "npcs";
            if (n.Contains("quest")) return "quests";
            if (n.Contains("armor") || n.Contains("major")) return "armor";
            if (n.Contains("ship") || n.Contains("mesh")) return "mesh";
            if (n.Contains("data") || n.Contains("balance") || n.Contains("table")) return "data";
            if (n.Contains("ui") || n.Contains("widget") || n.Contains("hud")) return "ui";
            if (n.Contains("vfx") || n.Contains("fx") || n.Contains("effect")) return "vfx";
            if (n.Contains("audio") || n.Contains("sound") || n.Contains("fmod")) return "audio";
            if (n.Contains("galaxy") || n.Contains("space")) return "galaxy";
            if (n.Contains("cinematic") || n.Contains("cutscene")) return "cinematic";
            if (info.Type == ModType.AssetReplacement) return "mesh";
            return "default";
        }

        private void WriteFolderPlaceholders(string contentRoot, string preset)
        {
            foreach (string folder in GameInsights.GetScaffoldFoldersForPreset(preset))
            {
                string dir = Path.Combine(contentRoot, folder.Replace('/', Path.DirectorySeparatorChar));
                string readme = Path.Combine(dir, "PLACE_COOKED_ASSETS_HERE.txt");
                if (File.Exists(readme)) continue;
                var role = "";
                foreach (var f in GameInsights.ContentFolders)
                    if (f.Folder.Equals(folder.Split('/')[0], StringComparison.OrdinalIgnoreCase))
                        role = f.Role;
                File.WriteAllText(readme,
                    "Drop cooked UE 4.27 assets here (.uasset + .uexp + optional .ubulk).\n" +
                    "Path must mirror the game: SB2/Content/" + folder + "/...\n" +
                    (string.IsNullOrEmpty(role) ? "" : "Role: " + role + "\n") +
                    "Then pack the parent {Mod}_P folder with UnrealPak -compress.\n",
                    Encoding.UTF8);
            }
        }

        public void AddFile(string relativeGamePath, string sourceFilePath)
        {
            // relativeGamePath should start after Content/, e.g. "Textures/Ships/MyShip.uasset"
            // or full "SB2/Content/..."
            string cleanRel = relativeGamePath.Replace('\\', '/').TrimStart('/');
            if (cleanRel.StartsWith("SB2/", StringComparison.OrdinalIgnoreCase))
                cleanRel = cleanRel.Substring(4);
            if (cleanRel.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
                cleanRel = cleanRel.Substring(8);

            string dest = Path.Combine(StagingRoot, "SB2", "Content", cleanRel.Replace('/', Path.DirectorySeparatorChar));
            FileService.EnsureDirectory(Path.GetDirectoryName(dest));
            File.Copy(sourceFilePath, dest, true);
            Info.Files.Add(cleanRel);
            Info.Modified = DateTime.Now;
            LoggerService.Log($"Added file to mod: {cleanRel}");
        }

        public void AddLocalization(string languageCode, string locresFilePath)
        {
            // Language folders: en, de, fr, es-ES, etc.
            string destDir = Path.Combine(StagingRoot, "SB2", "Content", "Localization", "Game", languageCode);
            FileService.EnsureDirectory(destDir);
            string dest = Path.Combine(destDir, "Game.locres");
            File.Copy(locresFilePath, dest, true);
            Info.Files.Add($"Localization/Game/{languageCode}/Game.locres");
            Info.Type = ModType.Localization;
            Info.Modified = DateTime.Now;
        }

        public string GetStagingContentPath()
        {
            return Path.Combine(StagingRoot, "SB2", "Content");
        }

        private void WriteModInfoJson()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"name\": \"{Escape(Info.Name)}\",");
            sb.AppendLine($"  \"author\": \"{Escape(Info.Author)}\",");
            sb.AppendLine($"  \"version\": \"{Escape(Info.Version)}\",");
            sb.AppendLine($"  \"description\": \"{Escape(Info.Description)}\",");
            sb.AppendLine($"  \"game\": \"SpaceBourne 2\",");
            sb.AppendLine($"  \"engine\": \"UE4.27\",");
            sb.AppendLine($"  \"chunkId\": {Info.ChunkId},");
            sb.AppendLine($"  \"type\": \"{Info.Type}\",");
            sb.AppendLine($"  \"created\": \"{Info.Created:o}\"");
            sb.AppendLine("}");
            File.WriteAllText(Path.Combine(ProjectRoot, "modinfo.json"), sb.ToString(), Encoding.UTF8);
        }

        private void WriteInstallReadme()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {Info.Name} - SpaceBourne 2 Mod");
            sb.AppendLine();
            sb.AppendLine($"Author: {Info.Author}");
            sb.AppendLine($"Version: {Info.Version}");
            sb.AppendLine($"Target: SpaceBourne 2 (UE 4.27)");
            sb.AppendLine();
            sb.AppendLine("## Installation");
            sb.AppendLine();
            sb.AppendLine("1. Locate your game install folder (usually `...\\steamapps\\common\\SpaceBourne 2\\`).");
            sb.AppendLine("2. Copy the generated `.pak` file into:");
            sb.AppendLine("   `SB2\\Content\\Paks\\`");
            sb.AppendLine("3. Make sure the filename ends with `_P.pak` (priority override).");
            sb.AppendLine("4. Launch the game. Higher chunk numbers / alphabetical order after vanilla paks load later.");
            sb.AppendLine();
            sb.AppendLine("## Alternative: Loose Files");
            sb.AppendLine();
            sb.AppendLine("You can also copy the contents of the staging folder directly into the game's `SB2` folder");
            sb.AppendLine("(the engine supports loose file overrides when no AES encryption is present).");
            sb.AppendLine();
            sb.AppendLine("## Notes");
            sb.AppendLine();
            sb.AppendLine("- This is an unofficial community mod.");
            sb.AppendLine("- Game updates may break file-based mods.");
            sb.AppendLine("- Always back up your save games before testing mods.");
            sb.AppendLine();
            sb.AppendLine("Generated by SpaceBourne 2 Mod Creation Tool");
            File.WriteAllText(Path.Combine(ProjectRoot, "README.md"), sb.ToString(), Encoding.UTF8);
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }
    }
}
