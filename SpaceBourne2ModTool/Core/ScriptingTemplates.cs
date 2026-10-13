using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// UE4SS Lua starters aligned with RE-UE4SS docs (Mods/Name/scripts/main.lua).
    /// Plain-language comments for beginners.
    /// </summary>
    public static class ScriptingTemplates
    {
        public static string Ue4ssHelloLua(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- ============================================================");
            sb.AppendLine("-- Live script for UE4SS (optional). File must be: scripts/main.lua");
            sb.AppendLine("-- Mod folder name: " + modName);
            sb.AppendLine("-- ============================================================");
            sb.AppendLine("-- This runs ONLY if UE4SS is installed and this mod is enabled");
            sb.AppendLine("-- in Mods/mods.txt as:  " + modName + " : 1");
            sb.AppendLine("-- Most SpaceBourne 2 file mods do NOT need this.");
            sb.AppendLine();
            sb.AppendLine("print(\"[specSB2modTool] " + modName + " loaded\\n\")");
            sb.AppendLine();
            sb.AppendLine("-- Safe place to register more hooks later (game has started initializing).");
            sb.AppendLine("RegisterHook(\"/Script/Engine.PlayerController:ClientRestart\", function(Context)");
            sb.AppendLine("    print(\"[specSB2modTool] PlayerController ClientRestart seen\\n\")");
            sb.AppendLine("end)");
            return sb.ToString();
        }

        public static string Ue4ssKeybindTemplate(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- Keybind example — RegisterKeyBind does NOT run on the game thread.");
            sb.AppendLine("-- Always use ExecuteInGameThread before touching Unreal objects.");
            sb.AppendLine("-- Enable in mods.txt: " + modName + " : 1");
            sb.AppendLine();
            sb.AppendLine("print(\"[specSB2modTool] " + modName + " keybind template loaded\\n\")");
            sb.AppendLine();
            sb.AppendLine("RegisterKeyBind(Key.F5, function()");
            sb.AppendLine("    ExecuteInGameThread(function()");
            sb.AppendLine("        print(\"[specSB2modTool] F5 pressed (on game thread)\\n\")");
            sb.AppendLine("        -- Put game object reads/writes only inside this block.");
            sb.AppendLine("    end)");
            sb.AppendLine("end)");
            return sb.ToString();
        }

        public static string Ue4ssHookSafeTemplate(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- Safer hook pattern: wait until a known engine path runs, then register more.");
            sb.AppendLine("-- Function paths MUST come from THIS game (UE4SS live view / dumpers).");
            sb.AppendLine("-- Wrong paths fail silently or crash. Enable: " + modName + " : 1");
            sb.AppendLine();
            sb.AppendLine("print(\"[specSB2modTool] " + modName + " hook-safe template loaded\\n\")");
            sb.AppendLine();
            sb.AppendLine("local hooksInstalled = false");
            sb.AppendLine();
            sb.AppendLine("RegisterHook(\"/Script/Engine.PlayerController:ClientRestart\", function(Context)");
            sb.AppendLine("    if hooksInstalled then return end");
            sb.AppendLine("    hooksInstalled = true");
            sb.AppendLine("    print(\"[specSB2modTool] Installing deferred hooks\\n\")");
            sb.AppendLine();
            sb.AppendLine("    -- Example only — replace with a real path discovered for SpaceBourne 2:");
            sb.AppendLine("    -- NotifyOnNewObject(\"/Script/Engine.Actor\", function(Actor)");
            sb.AppendLine("    --     print(\"Actor spawned\\n\")");
            sb.AppendLine("    -- end)");
            sb.AppendLine("end)");
            return sb.ToString();
        }

        public static string Ue4ssModsTxtLine(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            return "; Enable this live script mod (place ABOVE the Keybinds line if present)\r\n"
                 + modName + " : 1\r\n";
        }

        public static string Ue4ssPrePostHookTemplate(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- Pre + post RegisterHook for /Script/ paths (UE4SS)");
            sb.AppendLine("-- Before runs prior to the engine function; after runs when it returns.");
            sb.AppendLine("-- Enable: " + modName + " : 1");
            sb.AppendLine();
            sb.AppendLine("print(\"[specSB2modTool] " + modName + " pre/post hook template\\n\")");
            sb.AppendLine();
            sb.AppendLine("RegisterHook(\"/Script/Engine.PlayerController:ClientRestart\",");
            sb.AppendLine("    function(Context)");
            sb.AppendLine("        print(\"[specSB2modTool] BEFORE ClientRestart\\n\")");
            sb.AppendLine("    end,");
            sb.AppendLine("    function(Context)");
            sb.AppendLine("        print(\"[specSB2modTool] AFTER ClientRestart\\n\")");
            sb.AppendLine("    end");
            sb.AppendLine(")");
            return sb.ToString();
        }

        public static string Ue4ssInitGameStateTemplate(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- Install hooks once after game state initializes (good lifecycle point).");
            sb.AppendLine("-- Enable: " + modName + " : 1");
            sb.AppendLine();
            sb.AppendLine("local installed = false");
            sb.AppendLine();
            sb.AppendLine("RegisterInitGameStatePostHook(function(GameState)");
            sb.AppendLine("    if installed then return end");
            sb.AppendLine("    installed = true");
            sb.AppendLine("    print(\"[specSB2modTool] " + modName + " InitGameStatePost — installing\\n\")");
            sb.AppendLine("    NotifyOnNewObject(\"/Script/Engine.Actor\", function(Obj)");
            sb.AppendLine("        -- Example only; narrow the class path for real mods");
            sb.AppendLine("        -- print(Obj:GetFullName())");
            sb.AppendLine("    end)");
            sb.AppendLine("end)");
            return sb.ToString();
        }

        public static string Ue4ssFindObjectTemplate(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName)) modName = "MyLiveScriptMod";
            var sb = new StringBuilder();
            sb.AppendLine("-- FindFirstOf / FindAllOf / StaticFindObject (UE4SS Lua API)");
            sb.AppendLine("-- Find*Of uses short class names for live instances.");
            sb.AppendLine("-- FindAllOf returns nil when empty — always null-check. Do not poll every tick.");
            sb.AppendLine("-- StaticFindObject is best for UClass and defaults.");
            sb.AppendLine("-- Enable: " + modName + " : 1");
            sb.AppendLine();
            sb.AppendLine("print(\"[specSB2modTool] " + modName + " find-object template\\n\")");
            sb.AppendLine();
            sb.AppendLine("RegisterInitGameStatePostHook(function(GameState)");
            sb.AppendLine("    local first = FindFirstOf(\"PlayerController\")");
            sb.AppendLine("    if first and first:IsValid() then");
            sb.AppendLine("        print(\"Found: \" .. first:GetFullName() .. \"\\n\")");
            sb.AppendLine("    end");
            sb.AppendLine("    local cls = StaticFindObject(\"/Script/Engine.PlayerController\")");
            sb.AppendLine("    if cls and cls:IsValid() then print(\"StaticFindObject ok\\n\") end");
            sb.AppendLine("end)");
            return sb.ToString();
        }

        public static string Ue4ssInstallGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("UE4SS live scripting — plain English guide");
            sb.AppendLine("==========================================");
            sb.AppendLine();
            sb.AppendLine("UE4SS is a separate community program that can load Lua scripts while the");
            sb.AppendLine("game runs. It is optional. File mods from this application do not require it.");
            sb.AppendLine();
            sb.AppendLine("Correct Lua mod layout");
            sb.AppendLine("-----------------------");
            sb.AppendLine("  Mods/");
            sb.AppendLine("    YourModName/");
            sb.AppendLine("      scripts/");
            sb.AppendLine("        main.lua");
            sb.AppendLine();
            sb.AppendLine("Enable the mod by adding to Mods/mods.txt:");
            sb.AppendLine("  YourModName : 1");
            sb.AppendLine("Use 0 to disable. Keep the built-in Keybinds entry at the bottom.");
            sb.AppendLine();
            sb.AppendLine("Where to install UE4SS");
            sb.AppendLine("----------------------");
            sb.AppendLine("Usually next to the game executable under Binaries/Win64.");
            sb.AppendLine("Newer builds may place files under a ue4ss subfolder. Always follow the");
            sb.AppendLine("version of the UE4SS installation guide you download.");
            sb.AppendLine();
            sb.AppendLine("Thread safety (very important)");
            sb.AppendLine("-------------------------------");
            sb.AppendLine("• RegisterHook and NotifyOnNewObject run on the game thread.");
            sb.AppendLine("• RegisterKeyBind runs on another thread — wrap game work in");
            sb.AppendLine("  ExecuteInGameThread(...).");
            sb.AppendLine("• Each mod has its own Lua memory space.");
            sb.AppendLine();
            sb.AppendLine("Discovering real function paths");
            sb.AppendLine("-------------------------------");
            sb.AppendLine("Use UE4SS live view and dumpers for SpaceBourne 2. Paths copied from a");
            sb.AppendLine("different game will not work. Dump Lua bindings only for editor help —");
            sb.AppendLine("do not require those generated files from your main.lua.");
            sb.AppendLine();
            sb.AppendLine("Hot reload");
            sb.AppendLine("----------");
            sb.AppendLine("When supported, Ctrl+R reloads Lua mods. Register hooks only once per load.");
            return sb.ToString();
        }

        public static string PythonEditorSample()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Unreal Editor Python — NOT for the Steam game while playing");
            sb.AppendLine("# ============================================================");
            sb.AppendLine("# Requires the original Unreal project and the Python Editor plugin.");
            sb.AppendLine();
            sb.AppendLine("import unreal");
            sb.AppendLine();
            sb.AppendLine("assets = unreal.EditorUtilityLibrary.get_selected_assets()");
            sb.AppendLine("for asset in assets:");
            sb.AppendLine("    unreal.log(\"Selected: {}\".format(asset.get_name()))");
            return sb.ToString();
        }

        public static string CsharpPluginNotes()
        {
            var sb = new StringBuilder();
            sb.AppendLine("C# and Unreal — plain English");
            sb.AppendLine("=============================");
            sb.AppendLine();
            sb.AppendLine("Unreal gameplay is usually C++ or Blueprints, not C#.");
            sb.AppendLine("C# is excellent for desktop tools: this program, UAssetGUI, FModel.");
            sb.AppendLine("For live in-game scripts, UE4SS uses Lua (or C++ mods), not C#.");
            return sb.ToString();
        }

        public static string ComparisonGuide()
        {
            return BeginnerGuide.Scripting + "\r\n\r\n" + Ue4ssInstallGuide() + "\r\n\r\n" +
                   "Recommended path for beginners:\r\n" +
                   "1. Create a file mod from a template.\r\n" +
                   "2. Package with Speckage and use the Mod Loader.\r\n" +
                   "3. Only build a UE4SS Lua folder if you need live behavior.\r\n";
        }
    }
}
