using System.Collections.Generic;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Plain-language help for people who have never modded a game.
    /// Explain terms in the same sentence; avoid unexplained short forms.
    /// </summary>
    public static class BeginnerGuide
    {
        public const string WhatIsAMod =
            "A mod is a set of extra or replacement files that change how the game looks or behaves.\r\n\r\n" +
            "This program does not rewrite the original SpaceBourne 2 program. Instead it builds small " +
            "collections of files the game can load on top of the original content. When something goes wrong, " +
            "you can remove those extra files and the game returns to normal.";

        public const string Dashboard =
            "This is your home screen.\r\n\r\n" +
            "Step 1 — Tell the program where SpaceBourne 2 is installed. On most computers it lives under " +
            "Steam in a folder named \"SpaceBourne 2\". Use Set / Change Game Path, or let the first-run " +
            "search find it for you.\r\n\r\n" +
            "Step 2 — Run the Game File Scan (Game Analyzer). The scan reads the game's packed content so " +
            "later tools know real folder names (characters, quests, armor, and so on).\r\n\r\n" +
            "Step 3 — Create a mod project with New Mod or Mod Templates, make your changes, then package " +
            "and load the mod with Speckage and the Mod Loader.\r\n\r\n" +
            "The list of recently saved mods and the snapshot box show progress. Green \"Valid\" means the " +
            "program found a real game install.";

        public const string Analyzer =
            "The Game File Scan (also called the Game Analyzer) reads the game's packed content files and " +
            "writes easy-to-open reports.\r\n\r\n" +
            "SpaceBourne 2 stores most of its content in large archive files (package archives). Opening every " +
            "entry can take a long time the first time. Leave the window open until both progress bars finish.\r\n\r\n" +
            "When the scan completes, reports appear in your Mods output folder under Analysis. Other parts of " +
            "this program read those reports so they can offer real character names, quest folders, and " +
            "suggested load order.\r\n\r\n" +
            "You can cancel a scan if needed. Running it again after a game update is a good idea because " +
            "new updates may change files.";

        public const string Templates =
            "A mod template is a ready-made folder layout for a common kind of change.\r\n\r\n" +
            "Examples: all major character outfits, quest text areas, movie replacements, or balance notes.\r\n\r\n" +
            "Choosing a template creates the folders, short instruction files, and optional object or script " +
            "notes for you. You still place the actual art or text files, but you do not have to invent every " +
            "path from memory.\r\n\r\n" +
            "You can also save your own template from an open mod so you can reuse your favorite layout later.";

        public const string SteamStudio =
            "Players on the Steam discussion board asked for three things this studio supports:\r\n\r\n" +
            "1) Clearer English in conversations and quests — paste text into the Dialogue Grammar tab, " +
            "press Clean Grammar, then save into your open mod.\r\n\r\n" +
            "2) Access to art, planets, and story content — the Graphics / Planets / Story tab lists real " +
            "folders from the last scan so you know where to stage replacements.\r\n\r\n" +
            "3) Rebalancing numbers such as health or damage — the Rebalance tab saves a simple value " +
            "document. Applying those numbers into the game's binary data files may still need the free " +
            "UAssetGUI program when the values live inside packed assets.\r\n\r\n" +
            "Use Scaffold Complete Mod to build a starter project for the selected request.";

        public const string Speckage =
            "Speckage is this program's shareable package format.\r\n\r\n" +
            "Think of it like a zip file with a label inside that stores the mod name, author, version, and " +
            "preferred load order. Friends can import the Speckage file into the Mod Loader without unpacking " +
            "every folder by hand.\r\n\r\n" +
            "Speckage is a zip with a label: mod name, author, version, and load order. " +
            "Someone else can import that file into the Mod Loader.";

        public const string ModLoader =
            "The Mod Loader is how you turn packaged mods on and off.\r\n\r\n" +
            "Import a Speckage package, set the load order (lower numbers load earlier; higher numbers can " +
            "override earlier ones), enable the mods you want, then choose Inject. The loader only places " +
            "managed patch files with a safe naming pattern and keeps a backup of what it changed.\r\n\r\n" +
            "If the game misbehaves, use Unload Managed to remove those injected files and restore from the " +
            "backup when available. Original large game archives are not deleted by default.";

        public const string Packaging =
            "Packaging means turning your staged folders into something the game can load.\r\n\r\n" +
            "Two common approaches:\r\n" +
            "• Speckage package — good for sharing and for the Mod Loader.\r\n" +
            "• Patch package archive (names often end with _P) — built with UnrealPak from Unreal Engine 4.27 " +
            "when you need a direct archive the game reads from its Paks folder.\r\n\r\n" +
            "Always keep pairs of cooked assets together: the .uasset file and its matching .uexp file " +
            "(and .ubulk when present). Missing one of the pair often crashes the game.";

        public const string ExternalTools =
            "Some steps need free helper programs because the data is stored in special binary formats:\r\n\r\n" +
            "• FModel — browse and export files from the game's package archives.\r\n" +
            "• UAssetGUI — view and change properties inside cooked asset files (engine version 4.27).\r\n" +
            "• UModel — preview many three-dimensional models.\r\n" +
            "• UnrealPak — create compressed package archives with the same tools the engine uses.\r\n\r\n" +
            "This application finds those tools when possible and can open them for you. It cannot fully " +
            "replace FModel or UAssetGUI inside itself on .NET Framework 4.8, but it handles structure, " +
            "text, packaging, and safe injection.";

        public const string Scripting =
            "Scripting means writing small programs that change the editor or the running game.\r\n\r\n" +
            "Important limits for SpaceBourne 2 players:\r\n\r\n" +
            "• Python in Unreal Engine only runs inside the Unreal Editor while a project is open. It does " +
            "not run inside the finished Steam game. Python is for developers who own the original project " +
            "files and want to automate art work in the editor.\r\n\r\n" +
            "• Official gameplay code in Unreal is usually C++ or Blueprints, not C#. C# is widely used in " +
            "community tools (including this program and UAssetGUI).\r\n\r\n" +
            "• For live changes while the Steam game is running, community tools such as UE4SS use Lua " +
            "(and sometimes C++). That is optional and separate from the file packages this tool builds.\r\n\r\n" +
            "Most beginners should start with Templates and the Mod Loader. Open Scripting Studio only when " +
            "you specifically want Lua starter files or editor Python samples.";

        public const string LiveUe4ss =
            "UE4SS is an optional community tool that can run small Lua programs while the game is open.\r\n\r\n" +
            "Use it only when a file package cannot do what you need. Correct layout is:\r\n" +
            "  YourModName/scripts/main.lua\r\n" +
            "and a line in Mods/mods.txt such as: YourModName : 1\r\n\r\n" +
            "This application can build that folder for you under Help → Live scripts and programming notes. " +
            "It does not install UE4SS into the game for you — follow the official UE4SS install guide.\r\n\r\n" +
            "Keyboard hooks must hand work to the game thread. Function paths must come from this game, not another title.";

        public const string Safety =
            "Safety habits that prevent long repair times:\r\n\r\n" +
            "• Keep Steam's verify integrity option in mind if something breaks badly.\r\n" +
            "• Prefer patch files that only override a few paths instead of replacing entire base archives.\r\n" +
            "• Leave \"Allow editing of vanilla core files\" turned off unless you understand the risk.\r\n" +
            "• Use the Mod Loader backup and unload features.\r\n" +
            "• After a game update, scan again and re-test your mods.";

        public const string Workflow =
            "Recommended first workflow (about thirty minutes once tools are installed):\r\n\r\n" +
            "1. Confirm the game path is Valid on the Dashboard.\r\n" +
            "2. Run the Game File Scan and wait until it finishes.\r\n" +
            "3. Open Mod Templates and create a mod from \"Blank patch\" or a Steam template.\r\n" +
            "4. Place replacement files into the staged folders (or use Dialogue Grammar for text notes).\r\n" +
            "5. Open Speckage packager, build a package, then open Mod Loader, import, enable, inject.\r\n" +
            "6. Start the game and check that your change appears.\r\n\r\n" +
            "If nothing changes, check load order, confirm file names match the game exactly, and confirm " +
            "you included both .uasset and .uexp for any cooked asset.";

        public static Dictionary<string, string> All()
        {
            return new Dictionary<string, string>
            {
                { "What is a mod?", WhatIsAMod },
                { "Home screen (Dashboard)", Dashboard },
                { "Game File Scan", Analyzer },
                { "Mod templates", Templates },
                { "Steam community requests", SteamStudio },
                { "Speckage packages", Speckage },
                { "Mod Loader", ModLoader },
                { "Packaging explained", Packaging },
                { "Helper programs", ExternalTools },
                { "Scripting (Python, C#, Lua)", Scripting },
                { "Live scripts with UE4SS (optional)", LiveUe4ss },
                { "Safety and backups", Safety },
                { "Recommended first workflow", Workflow },
            };
        }

        /// <summary>Short panel intro used on individual forms.</summary>
        public static string PanelIntro(string formKey)
        {
            switch ((formKey ?? "").ToLowerInvariant())
            {
                case "analyzer": return Analyzer;
                case "templates": return Templates;
                case "steam": return SteamStudio;
                case "loader": return ModLoader;
                case "speckage": return Speckage;
                case "scripting": return Scripting;
                case "dashboard": return Dashboard;
                default: return WhatIsAMod;
            }
        }
    }
}
