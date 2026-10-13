using System;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Builds a correct UE4SS Lua mod folder layout from official RE-UE4SS guidance:
    /// Mods/MyMod/scripts/main.lua and a mods.txt enable line.
    /// Does not install UE4SS into the game — that remains a manual, optional step.
    /// </summary>
    public static class Ue4ssProjectService
    {
        public static string DefaultExportRoot
        {
            get
            {
                string d = Path.Combine(ConfigService.ModsOutputPath ?? "", "UE4SS_Lua_Projects");
                Directory.CreateDirectory(d);
                return d;
            }
        }

        /// <summary>Guess SpaceBourne 2 Win64 binary folder from a valid install.</summary>
        public static string TryFindGameWin64()
        {
            try
            {
                string root = ConfigService.Game != null ? ConfigService.Game.InstallPath : null;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return null;
                // Common UE shipping layout: Game/Binaries/Win64 or SB2/Binaries/Win64
                foreach (string cand in new[]
                {
                    Path.Combine(root, "SB2", "Binaries", "Win64"),
                    Path.Combine(root, "Binaries", "Win64"),
                    Path.Combine(root, "SpaceBourne2", "Binaries", "Win64"),
                })
                    if (Directory.Exists(cand)) return cand;
                // Search one level
                foreach (string dir in Directory.GetDirectories(root))
                {
                    string w = Path.Combine(dir, "Binaries", "Win64");
                    if (Directory.Exists(w)) return w;
                }
            }
            catch { }
            return null;
        }

        public static string CreateLuaModProject(string modName, int templateKind)
        {
            modName = Sanitize(modName);
            if (string.IsNullOrEmpty(modName)) modName = "MyLiveScriptMod";

            string root = Path.Combine(DefaultExportRoot, modName);
            string scripts = Path.Combine(root, "scripts");
            Directory.CreateDirectory(scripts);

            string mainLua = ScriptingTemplates.Ue4ssHelloLua(modName);
            if (templateKind == 1) mainLua = ScriptingTemplates.Ue4ssHookSafeTemplate(modName);
            else if (templateKind == 2) mainLua = ScriptingTemplates.Ue4ssKeybindTemplate(modName);
            else if (templateKind == 3) mainLua = ScriptingTemplates.Ue4ssPrePostHookTemplate(modName);
            else if (templateKind == 4) mainLua = ScriptingTemplates.Ue4ssInitGameStateTemplate(modName);
            else if (templateKind == 5) mainLua = ScriptingTemplates.Ue4ssFindObjectTemplate(modName);

            File.WriteAllText(Path.Combine(scripts, "main.lua"), mainLua, Encoding.UTF8);
            File.WriteAllText(Path.Combine(root, "mods_txt_line.txt"),
                ScriptingTemplates.Ue4ssModsTxtLine(modName), Encoding.UTF8);
            File.WriteAllText(Path.Combine(root, "enabled.txt"), "", Encoding.UTF8);
            File.WriteAllText(Path.Combine(root, "README_PLAIN_ENGLISH.txt"),
                BuildReadme(modName), Encoding.UTF8);

            string win64 = TryFindGameWin64();
            if (!string.IsNullOrEmpty(win64))
            {
                File.WriteAllText(Path.Combine(root, "SUGGESTED_GAME_WIN64_PATH.txt"),
                    win64 + "\r\n\r\nCopy the folder \"" + modName + "\" into the Mods folder next to UE4SS after you install UE4SS.\r\n" +
                    "Newer UE4SS builds may use: " + Path.Combine(win64, "ue4ss", "Mods") + "\r\n" +
                    "Older layout often uses: " + Path.Combine(win64, "Mods") + "\r\n",
                    Encoding.UTF8);
            }

            return root;
        }

        static string BuildReadme(string modName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Live script mod folder (UE4SS Lua) — plain English");
            sb.AppendLine("=================================================");
            sb.AppendLine();
            sb.AppendLine("What this is");
            sb.AppendLine("------------");
            sb.AppendLine("This is an OPTIONAL advanced project. It does not replace file mods made with");
            sb.AppendLine("templates and the Mod Loader. Most art, text, and balance changes should stay");
            sb.AppendLine("as file packages. Live scripts only help when you need something to run while");
            sb.AppendLine("the game is already open.");
            sb.AppendLine();
            sb.AppendLine("Folder layout (required by UE4SS)");
            sb.AppendLine("---------------------------------");
            sb.AppendLine("  " + modName + "/");
            sb.AppendLine("    scripts/");
            sb.AppendLine("      main.lua          ← the script that runs");
            sb.AppendLine("    mods_txt_line.txt   ← copy this line into Mods/mods.txt");
            sb.AppendLine("    enabled.txt         ← empty file; some setups use this instead of mods.txt");
            sb.AppendLine();
            sb.AppendLine("Install outline");
            sb.AppendLine("---------------");
            sb.AppendLine("1. Install UE4SS for Unreal Engine 4 into the game Win64 folder (read the");
            sb.AppendLine("   official UE4SS installation guide — versions differ).");
            sb.AppendLine("2. Copy this entire \"" + modName + "\" folder into the UE4SS Mods folder.");
            sb.AppendLine("3. Edit Mods/mods.txt and add a line (above the Keybinds line if present):");
            sb.AppendLine("     " + modName + " : 1");
            sb.AppendLine("   The number 1 means enabled. Use 0 to disable without deleting the folder.");
            sb.AppendLine("4. Start the game. Check UE4SS.log if nothing prints.");
            sb.AppendLine();
            sb.AppendLine("Important safety notes from UE4SS community practice");
            sb.AppendLine("----------------------------------------------------");
            sb.AppendLine("• RegisterHook runs on the game thread (safer for touching game objects).");
            sb.AppendLine("• RegisterKeyBind runs on a different thread — hand work to the game thread");
            sb.AppendLine("  with ExecuteInGameThread before touching Unreal objects.");
            sb.AppendLine("• Each mod has its own Lua state; do not rely on sharing globals between mods.");
            sb.AppendLine("• Wrong function paths do nothing or can crash. Discover real paths with the");
            sb.AppendLine("  UE4SS live view / dump tools for THIS game, not another title.");
            sb.AppendLine("• Hot reload is often Ctrl+R in the UE4SS console (when available).");
            sb.AppendLine();
            sb.AppendLine("Credits: structure follows RE-UE4SS / UE4SS documentation (community project).");
            return sb.ToString();
        }

        static string Sanitize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Trim();
        }
    }
}
