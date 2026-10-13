using System.Collections.Generic;
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Hard-coded insights from real Game Analyzer runs (2026-09-26 / 2026-09-27).
    /// Keep in sync when user returns newer SB2_GameAnalysis_LATEST.json.
    /// </summary>
    public static class GameInsights
    {
        public const string KnownMainPakName = "SB2-WindowsNoEditor.pak";
        public const string KnownMountPoint = "../../../";
        public const string KnownCompression = "Oodle";
        public const string KnownBinaryRelative = "SB2/Binaries/Win64/SB2-Win64-Shipping.exe";
        public const string KnownPaksRelative = "SB2/Content/Paks";
        public const string SteamAppId = "1646850";
        public const string EngineHint = "UE4.27";

        /// <summary>
        /// Top SB2/Content folders from deep list (~86k lines, 2026-09-27).
        /// Counts are approximate from that sample; used for scaffolding presets.
        /// </summary>
        public static readonly (string Folder, int ApproxCount, string Role)[] ContentFolders =
        {
            ("1Data", 7649, "Game data tables / definitions — high value for balance mods"),
            ("Packes_L_D", 4816, "Packaged LODs / detail packs"),
            ("Packes", 4203, "Packaged asset groups"),
            ("Armors", 3907, "Armor meshes/materials — strong visual mod target"),
            ("1Meshs", 3658, "Meshes (ships, props) — mesh/texture overrides"),
            ("Locomotion", 2624, "Character movement / animation"),
            ("11Misc", 2459, "Miscellaneous content"),
            ("StarterBundle", 2288, "Starter content bundle"),
            ("1Galaxy", 1671, "Galaxy / space content"),
            ("Indus", 1655, "Industrial / environment kit"),
            ("KB3D", 1526, "KB3D marketplace kit"),
            ("_Textures", 1511, "Shared textures"),
            ("BallisticsVFX", 1331, "Weapon / ballistic VFX"),
            ("CyberPunkMegapack", 1221, "Cyberpunk environment pack"),
            ("CyberpunkCity", 1217, "Cyberpunk city pack"),
            ("1Widgets", 1183, "UMG widgets / UI"),
            ("FMOD", 1076, "FMOD audio (also on disk under Content/FMOD)"),
            ("ScienceLab", 1061, "Science lab environment"),
            ("SpaceStation", 976, "Space station environment"),
            ("Brushify", 896, "Landscape / brushify"),
            ("Recife_2050", 845, "City environment pack"),
            ("THEARCHIVE", 650, "Archive content pack"),
            ("Chameleon", 643, "Chameleon system/assets"),
            ("ParagonProps", 609, "Paragon props"),
            ("ModSciInteriors", 591, "Sci-fi interiors"),
            ("JUPITER_SciFi_Kit", 572, "Jupiter sci-fi kit"),
            ("Vx", 502, "VX assets"),
            ("DesertDome", 484, "Desert dome environment"),
            ("111Cinematic", 0, "Cinematics / face anims / levels"),
            ("Localization", 0, "Localization (locres mostly in pak)"),
            ("Movies", 0, "Movies — also loose on disk"),
        };

        public static readonly string[] KnownConfigPaths =
        {
            "SB2/Config/DefaultDeviceProfiles.ini",
            "SB2/Config/DefaultEngine.ini",
            "SB2/Config/DefaultEvents.ini",
            "SB2/Config/DefaultGame.ini",
            "SB2/Config/DefaultGameplayTags.ini",
            "SB2/Config/DefaultInput.ini",
            "SB2/Config/LoggingOverride.ini",
        };

        public static string RecommendModPakFileName(string modName, int chunkId)
        {
            string safe = string.IsNullOrWhiteSpace(modName) ? "Mod" : modName;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                safe = safe.Replace(c, '_');
            safe = safe.Replace(' ', '_');
            return $"z_pakchunk{chunkId}-{safe}_P.pak";
        }

        public static IEnumerable<string> GetScaffoldFoldersForPreset(string preset)
        {
            switch ((preset ?? "").ToLowerInvariant())
            {
                case "armor":
                    yield return "Armors";
                    yield return "Armors/Majorcharacters";
                    yield return "Armors/1_New_sets";
                    yield return "_Textures";
                    break;
                case "npcs":
                    yield return "1Data/NPCs";
                    yield return "1Data/NPCs/Dialogs";
                    break;
                case "major_armor":
                    yield return "Armors/Majorcharacters";
                    break;
                case "quests":
                    yield return "1Data/Quests";
                    break;
                case "weapons":
                    yield return "1Data/Weapons";
                    break;
                case "ships":
                    yield return "1Data/SB2Ships";
                    break;
                case "mesh":
                case "ship":
                    yield return "1Meshs";
                    yield return "_Textures";
                    yield return "Packes";
                    break;
                case "data":
                case "balance":
                    yield return "1Data";
                    break;
                case "ui":
                    yield return "1Widgets";
                    break;
                case "vfx":
                    yield return "BallisticsVFX";
                    break;
                case "audio":
                    yield return "FMOD";
                    break;
                case "galaxy":
                    yield return "1Galaxy";
                    break;
                case "localization":
                    yield return "Localization/Game";
                    break;
                case "cinematic":
                    yield return "111Cinematic";
                    break;
                case "full":
                    foreach (var f in ContentFolders)
                        yield return f.Folder;
                    break;
                default:
                    yield return "1Data";
                    yield return "Armors";
                    yield return "1Meshs";
                    yield return "_Textures";
                    yield return "1Widgets";
                    break;
            }
        }

        public static string GetLayoutSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Real install layout (analyzer 2026-09-27, ~86k list lines):");
            sb.AppendLine("  Mount:    ../../../");
            sb.AppendLine("  Compress: Oodle dominant (+ None, some Zlib)");
            sb.AppendLine("  Main pak: SB2-WindowsNoEditor.pak (~11 GB, monolithic)");
            sb.AppendLine("  Content:  SB2/Content/ dominates (~78k sampled paths)");
            sb.AppendLine();
            sb.AppendLine("High-value SB2/Content folders:");
            foreach (var f in ContentFolders)
            {
                if (f.ApproxCount > 0)
                    sb.AppendLine($"  • {f.Folder}/ (~{f.ApproxCount}) — {f.Role}");
            }
            sb.AppendLine();
            sb.AppendLine("Mod packaging:");
            sb.AppendLine("  • Stage: {Mod}_P/SB2/Content/<folder>/...");
            sb.AppendLine("  • Pak:   z_pakchunkN-Name_P.pak → SB2/Content/Paks/");
            sb.AppendLine("  • Keep .uasset + .uexp (+ .ubulk) together");
            sb.AppendLine();
            sb.AppendLine("See Content Browser for live folder samples (Armors/1_New_sets, 1Data/11_AI, etc.).");
            return sb.ToString();
        }
    }
}
