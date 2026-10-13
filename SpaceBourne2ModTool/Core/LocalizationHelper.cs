using System;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Helpers for SpaceBourne 2 localization (.locres) workflow.
    /// Based on community method:
    /// 1. Extract with QuickBMS + UE4.27 script
    /// 2. Decode with unreal_locres_texttool (Unicode/Export)
    /// 3. Edit the .txt
    /// 4. Re-import and place under Content/Localization/Game/{lang}/
    /// </summary>
    public static class LocalizationHelper
    {
        public static string[] CommonLanguages = new[]
        {
            "en", "de", "fr", "es", "es-ES", "it", "pt", "pt-BR", "ru", "pl", "ja", "ko", "zh-Hans", "zh-Hant"
        };

        public static string CreateEditableTemplate(string languageCode, string outputFolder)
        {
            FileService.EnsureDirectory(outputFolder);
            string txtPath = Path.Combine(outputFolder, $"Game_{languageCode}.locres.txt");

            var sb = new StringBuilder();
            sb.AppendLine("; SpaceBourne 2 Localization Template");
            sb.AppendLine($"; Language: {languageCode}");
            sb.AppendLine("; Format is approximate – real files are binary .locres produced by unreal_locres_texttool.");
            sb.AppendLine("; Workflow:");
            sb.AppendLine(";   1. Use QuickBMS + 4.27 pak script to extract original Game.locres");
            sb.AppendLine(";   2. Run unreal_locres_texttool → Unicode/Export → obtain .txt");
            sb.AppendLine(";   3. Edit the .txt (keep key structure intact)");
            sb.AppendLine(";   4. Unicode/Import to recreate .locres");
            sb.AppendLine(";   5. Place as SB2/Content/Localization/Game/{lang}/Game.locres");
            sb.AppendLine(";   6. In-game: Interface → Language → select the new entry");
            sb.AppendLine();
            sb.AppendLine("; Example entries (replace with real exported keys):");
            sb.AppendLine("NSLOCTEXT(\"Game\", \"ExampleKey1\", \"Example English Text\")");
            sb.AppendLine("NSLOCTEXT(\"Game\", \"ExampleKey2\", \"Another string that can be translated\")");
            sb.AppendLine();
            sb.AppendLine("; Tip: Always keep a backup of the original .locres and work on a copy.");
            sb.AppendLine("; Tip: Use a language code that is NOT already shipped so the game lists it cleanly.");

            File.WriteAllText(txtPath, sb.ToString(), Encoding.UTF8);
            LoggerService.Log($"Created localization template: {txtPath}");
            return txtPath;
        }

        public static string GetInstallInstructions(string languageCode)
        {
            return
                "After you have a valid Game.locres for language \"" + languageCode + "\":\n\n" +
                "1. Go to your game folder → SB2\\Content\\\n" +
                "2. Create Localization\\Game\\" + languageCode + "\\ if it does not exist\n" +
                "3. Place Game.locres inside that folder\n" +
                "4. Start SpaceBourne 2 → Interface → Language\n" +
                "5. Your new language should appear in the list\n\n" +
                "Note: Prefer a language code that is not already present in the base game.";
        }
    }
}
