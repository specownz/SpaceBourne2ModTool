using System.Collections.Generic;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// In-app Help Section topics — keep aligned with the User Manual on every update.
    /// </summary>
    public static class HelpContent
    {
        public static IEnumerable<KeyValuePair<string, string>> Topics()
        {
            yield return Kv("1. Introduction",
                "specSB2modTool is an unofficial Windows program for creating mods for SpaceBourne 2.\r\n\r\n" +
                "A mod is a set of extra or replacement files the game can load on top of the original content. " +
                "This program does not rewrite the original game program. It is not affiliated with the game developer.\r\n\r\n" +
                "Tagline: a creation by spec · assisted by Grok (xAI).");

            yield return Kv("2. Quick start",
                "1. Set the SpaceBourne 2 install folder on the home screen (or use first-run search).\r\n" +
                "2. Confirm it shows Valid.\r\n" +
                "3. Run the Game File Scan and wait until both progress bars finish.\r\n" +
                "4. Create a mod from a ready-made template or the community request studio.\r\n" +
                "5. Place your files or use the dialogue cleaner / balance notes.\r\n" +
                "6. Build a Speckage package, then use the Mod Loader to inject and start the game.");

            yield return Kv("3. Home screen and menus",
                "The program opens maximized and uses the whole screen.\r\n\r\n" +
                "The left Quickbar is a scrollable list of the tools you use most. Its title is Quickbar. Buttons are the same height and the text is white on a dark button.\r\n\r\n" +
                "The large area is a tab workspace. Home is the dashboard, new mod, pak tools, localization, trainer notes, and settings. Every other tool opens as its own tab so you can switch without closing your place. Window, Close this tool tab, closes the tool you are looking at and leaves Home alone.\r\n\r\n" +
                "The top menu is File, Analyze, Create, Package, View, Tools, Window, and Help. About is under Help.\r\n\r\n" +
                "The status line along the bottom tells you what the last action did.");

            yield return Kv("4c. Working Project Folder",
                "The working folder is your copy of ripped game files. It is not the Steam install.\r\n\r\n" +
                "Scan (skip known files) uses the rip ledger. A file that has not changed is not read again. Hard rip reads every file and asks first.\r\n\r\n" +
                "The status line shows how many files were reused and how many are new. The storage line shows gigabytes used and free space on that drive.\r\n\r\n" +
                "The list is on the left. A preview is on the right: pictures, and a simple outline for OBJ and STL models. Cooked packages do not draw a picture. Use Open this file for the full image viewer, 3D viewer, EvE voice form, UAssetGUI, or your code editor.\r\n\r\n" +
                "Copy selected or copy the shown list into the open mod. Files already in the mod are skipped. Import leaves files that were already ripped unless you choose Re-import changed files.");

            yield return Kv("4. Game File Scan",
                BeginnerGuide.Analyzer + "\r\n\r\nAfter a scan, open SB2_EasyModCookbook_LATEST.txt and SB2_ModTargets_LATEST.txt for the easiest override paths. SB2_GameFingerprint_LATEST.txt powers Convert mod to current game files.");

            yield return Kv("4d. After a game update",
                "When SpaceBourne 2 updates, run the Game File Scan again into the same reports folder.\r\n\r\n" +
                "The scan writes SB2_UpdateDiff_LATEST.txt. That file lists pak files that were added, removed, resized, or rewritten, plus content folders and file types whose counts changed.\r\n\r\n" +
                "The first scan in a folder is only a baseline. The second scan is the one that shows the update. Send back SB2_UpdateDiff_LATEST.txt and SB2_GameAnalysis_LATEST.md.\r\n\r\n" +
                "Weapons, helmets, loot tables, and effects are edited in Object Editors. They are not a second set of tools.");

            yield return Kv("4b. Merge-safe scaffolding",
                "Whenever the app scaffolds into an open mod it creates missing folders, merges unique lines into list files (such as TARGET_PATHS), and only overwrites existing text files after a warning that shows excerpts of old and new content. You can scaffold multiple times without wiping your work.");

            yield return Kv("5. Mod templates",
                BeginnerGuide.Templates);

            yield return Kv("6. Community request studio",
                BeginnerGuide.SteamStudio);

            yield return Kv("7. Speckage packages",
                BeginnerGuide.Speckage);

            yield return Kv("8. Mod Loader",
                BeginnerGuide.ModLoader);

            yield return Kv("9. Packaging",
                BeginnerGuide.Packaging);

            yield return Kv("10. Helper programs",
                BeginnerGuide.ExternalTools);

            yield return Kv("11. Scripting and live scripts",
                BeginnerGuide.Scripting + "\r\n\r\n" + BeginnerGuide.LiveUe4ss);

            yield return Kv("12. UE4SS hook patterns (advanced)",
                "RegisterHook runs on the game thread when a game function runs.\r\n" +
                "For engine paths that start with /Script/, you can pass a before-callback and an after-callback.\r\n" +
                "NotifyOnNewObject runs when a class is constructed (including child classes).\r\n" +
                "RegisterInitGameStatePostHook runs after the game mode initializes state — a good place to install more hooks once.\r\n" +
                "RegisterKeyBind does NOT run on the game thread — wrap game work in ExecuteInGameThread.\r\n" +
                "Wrap callback bodies carefully; wrong function paths can crash. Discover paths with UE4SS tools for THIS game.\r\n" +
                "Use Help → Live scripts to export a correct scripts/main.lua folder.");

            yield return Kv("13. Convert mod to current game files",
                "After a game update, package archive sizes or dates change. This program stores a game fingerprint with your mod.\r\n\r\n" +
                "If the fingerprint still matches, conversion does nothing.\r\n" +
                "If it differs (or was never stamped), conversion refreshes TARGET_PATHS from the latest scan, " +
                "writes a CONVERT_GUIDE, links analysis reports, and stamps the new fingerprint.\r\n\r\n" +
                "It does not automatically rewrite binary .uasset data. You may still need to re-export or edit cooked pairs if the update renamed assets.");

            yield return Kv("14. SpecFramework (offline packaging)",
                "PathGuard keeps writes under safe roots and blocks original core archives by default.\r\n" +
                "Speckage builds shareable packages. Mod Loader injects only managed patch files and keeps backups.\r\n" +
                "SpecPipeline shows the checklist from scaffold to inject.");

            yield return Kv("14b. GitHub publish and import",
                "Package or Tools → GitHub. Save a Personal Access Token (repo scope), test connection, then:\r\n" +
                "• Publish open mod: creates a GitHub repository and pushes with Git\r\n" +
                "• Commit + push / Pull for updates\r\n" +
                "• Import: clone URL or owner/repo, or Download ZIP without Git\r\n" +
                "• Browse/search repositories and list releases\r\n" +
                "Imports land under Mods output / GitHubImports. Token stays in local settings only.");

            yield return Kv("15. Live session observer",
                "Optional tool under Tools. Records notes while you play and can read the end of a UE4SS log file if that separate program is installed.\r\n\r\n" +
                "It does not inject into the game. Export a merged report to combine notes with the latest Game File Scan targets for better path planning.");

            yield return Kv("16. Safety",
                BeginnerGuide.Safety);

            yield return Kv("17. Form scaling",
                "Windows can be resized. Controls scale using the shared form scale helper when the form was opened through the normal branding path.");

            yield return Kv("18. Credits",
                "Created by spec. Assisted by Grok (xAI).\r\n" +
                "FModel / CUE4Parse, UAssetAPI / UAssetGUI (atenfyr), UModel, Epic UnrealPak, RE-UE4SS documentation patterns.");
        }

        static KeyValuePair<string, string> Kv(string k, string v)
        {
            return new KeyValuePair<string, string>(k, v);
        }
    }
}
