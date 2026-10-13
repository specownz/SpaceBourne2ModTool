using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Conversion guidance + helper scripts for turning common authoring formats
    /// into what SpaceBourne 2 (UE 4.27 cooked assets) can load.
    ///
    /// The game does not load raw PNG/FBX at runtime. Pipeline is:
    ///   authoring format → UE 4.27 editor import → Cook → .uasset/.uexp/.ubulk → PAK
    /// Loose exceptions: some Content/Movies and Content/FMOD files on disk.
    /// </summary>
    public static class AssetConversionHelper
    {
        public class ConversionRecipe
        {
            public string SourceTypes { get; set; }
            public string TargetTypes { get; set; }
            public string Category { get; set; }
            public string Steps { get; set; }
            public bool CanLooseReplace { get; set; }
        }

        public static List<ConversionRecipe> GetRecipes()
        {
            return new List<ConversionRecipe>
            {

                new ConversionRecipe
                {
                    Category = "Game Data (1Data)",
                    SourceTypes = "DataTables / curves / JSON-like tables",
                    TargetTypes = ".uasset + .uexp under SB2/Content/1Data/",
                    CanLooseReplace = false,
                    Steps =
                        "1Data is the largest game-content folder (~7.6k assets).\n" +
                        "Export with FModel → identify DataTable / Curve assets → edit in UAssetGUI (UE 4.27)\n" +
                        "or re-cook from a matching UE project. Path must stay under SB2/Content/1Data/..."
                },
                new ConversionRecipe
                {
                    Category = "Armor",
                    SourceTypes = ".fbx, textures",
                    TargetTypes = "cooked under SB2/Content/Armors/",
                    CanLooseReplace = false,
                    Steps =
                        "Armors (~3.9k assets). Mirror FModel path under Armors/.\n" +
                        "Replace materials/textures in _Textures or Armors, keep uasset+uexp+ubulk."
                },
                new ConversionRecipe
                {
                    Category = "Meshes (1Meshs / Packes)",
                    SourceTypes = ".fbx, .obj",
                    TargetTypes = "cooked under SB2/Content/1Meshs or Packes",
                    CanLooseReplace = false,
                    Steps =
                        "1Meshs (~3.6k) and Packes (~4.2k) hold ship/prop meshes.\n" +
                        "FBX → UE 4.27 import → cook → stage at exact package path → z_*_P.pak."
                },
                new ConversionRecipe
                {
                    Category = "Texture",
                    SourceTypes = ".png, .tga, .jpg, .psd, .dds",
                    TargetTypes = ".uasset + .uexp (+ .ubulk)",
                    CanLooseReplace = false,
                    Steps =
                        "1. FModel → export existing texture raw or as image for reference path.\n" +
                        "2. Edit in Photoshop/GIMP/etc.\n" +
                        "3. UE 4.27 blank project → import image (same name if replacing).\n" +
                        "4. File → Cook Content for Windows.\n" +
                        "5. Copy cooked .uasset/.uexp/.ubulk into mod staging mirroring pak path (SB2/Content/...).\n" +
                        "6. Pack with UnrealPak -compress → z_*_P.pak into Paks folder."
                },
                new ConversionRecipe
                {
                    Category = "Material",
                    SourceTypes = "UE material asset / instance",
                    TargetTypes = ".uasset + .uexp",
                    CanLooseReplace = false,
                    Steps =
                        "Materials are cooked assets. Edit in UE 4.27 (or UAssetGUI for simple scalar/vector params),\n" +
                        "cook, then pak-override the same package path. Prefer MaterialInstance constants for simple recolors."
                },
                new ConversionRecipe
                {
                    Category = "Static / Skeletal Mesh",
                    SourceTypes = ".fbx, .obj, .gltf",
                    TargetTypes = ".uasset + .uexp (+ .ubulk)",
                    CanLooseReplace = false,
                    Steps =
                        "1. Export reference mesh from FModel if needed.\n" +
                        "2. Author in Blender/Maya → FBX.\n" +
                        "3. Import into UE 4.27 with matching skeleton if skeletal.\n" +
                        "4. Cook → place cooked files at original content path → pak."
                },
                new ConversionRecipe
                {
                    Category = "Localization",
                    SourceTypes = ".txt / spreadsheet",
                    TargetTypes = "Game.locres",
                    CanLooseReplace = true,
                    Steps =
                        "1. Extract Game.locres from pak (QuickBMS or FModel raw).\n" +
                        "2. unreal_locres_texttool → Unicode/Export.\n" +
                        "3. Edit text → Unicode/Import → Game.locres.\n" +
                        "4. Place at SB2/Content/Localization/Game/{lang}/Game.locres (loose or in _P pak).\n" +
                        "5. Prefer a language code not already shipped."
                },
                new ConversionRecipe
                {
                    Category = "Audio (FMOD)",
                    SourceTypes = ".wav, .ogg, .bank",
                    TargetTypes = ".bank / game audio assets",
                    CanLooseReplace = true,
                    Steps =
                        "Content/FMOD exists on disk outside the main pak — try replacing banks/files with same names first.\n" +
                        "In-pak audio still needs path-matched cooked or bank override via _P pak."
                },
                new ConversionRecipe
                {
                    Category = "Movies / Cinematics",
                    SourceTypes = ".mp4, .bk2, .avi",
                    TargetTypes = "same extension on disk",
                    CanLooseReplace = true,
                    Steps =
                        "Content/Movies is on disk. Match filename and container the game expects (often Bink .bk2 or mp4).\n" +
                        "Convert with ffmpeg if needed, then replace the file; no PAK required for pure disk movies."
                },
                new ConversionRecipe
                {
                    Category = "Config / INI",
                    SourceTypes = ".ini, .txt",
                    TargetTypes = ".ini in pak or Saved/Config",
                    CanLooseReplace = true,
                    Steps =
                        "Engine may read Saved/Config for user settings. Pack overrides for cooked defaults need matching path in _P pak.\n" +
                        "Always back up originals."
                },
                new ConversionRecipe
                {
                    Category = "Blueprint / Logic",
                    SourceTypes = "UE Blueprint",
                    TargetTypes = ".uasset + .uexp",
                    CanLooseReplace = false,
                    Steps =
                        "Requires UE 4.27 project aligned with game (difficult without official SDK).\n" +
                        "UAssetGUI can tweak some exported properties. Runtime logic mods more often use UE4SS or CE."
                }
            };
        }

        public static string WriteGuide(string outputDir)
        {
            FileService.EnsureDirectory(outputDir);
            string path = Path.Combine(outputDir, "SB2_Conversion_Guide.txt");
            var sb = new StringBuilder();
            sb.AppendLine("SpaceBourne 2 – Asset conversion guide");
            sb.AppendLine("Engine: UE 4.27 | Main pak: SB2-WindowsNoEditor.pak | Mount: ../../../");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: The runtime loads COOKED Unreal assets from PAK (and a few loose folders).");
            sb.AppendLine("You cannot drop a random PNG into the PAK and expect it to work.");
            sb.AppendLine();
            foreach (var r in GetRecipes())
            {
                sb.AppendLine("============================================================");
                sb.AppendLine(r.Category.ToUpperInvariant());
                sb.AppendLine("Source: " + r.SourceTypes);
                sb.AppendLine("Target: " + r.TargetTypes);
                sb.AppendLine("Loose disk replace possible: " + (r.CanLooseReplace ? "YES" : "NO — must cook + pak"));
                sb.AppendLine();
                sb.AppendLine(r.Steps);
                sb.AppendLine();
            }
            sb.AppendLine("============================================================");
            sb.AppendLine("Recommended external tools");
            sb.AppendLine("  FModel          – browse/export");
            sb.AppendLine("  UAssetGUI       – edit cooked uasset (set UE 4.27)");
            sb.AppendLine("  Unreal Engine 4.27 – import + cook");
            sb.AppendLine("  UnrealPak       – pack _P mods");
            sb.AppendLine("  ffmpeg          – movie/audio container conversion");
            sb.AppendLine("  QuickBMS + locres tool – localization");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            LoggerService.Log("Conversion guide written: " + path);
            return path;
        }

        /// <summary>
        /// Creates a starter batch that reminds the user of the cook/pack chain.
        /// Does not invoke UE (path unknown); documents the commands.
        /// </summary>
        public static string WritePipelineBatch(string outputDir, string modName)
        {
            FileService.EnsureDirectory(outputDir);
            string path = Path.Combine(outputDir, "CookAndPack_README.bat");
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("REM SpaceBourne 2 mod pipeline reminder");
            sb.AppendLine("REM 1) Cook in UE 4.27 editor: File -> Cook Content for Windows");
            sb.AppendLine("REM 2) Copy from Saved\\Cooked\\WindowsNoEditor\\... into your mod staging SB2\\Content\\...");
            sb.AppendLine("REM 3) Run CreatePak.bat from the mod Output folder (generated by this tool)");
            sb.AppendLine("REM 4) Copy z_*_P.pak into SB2\\Content\\Paks\\");
            sb.AppendLine("echo See SB2_Conversion_Guide.txt for format details.");
            sb.AppendLine("pause");
            File.WriteAllText(path, sb.ToString(), Encoding.ASCII);
            return path;
        }
    }
}
