using System;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// UAssetAPI + UE4SS integration knowledge for SB2 (UE 4.27).
    /// UAssetAPI is a separate library (atenfyr); UAssetGUI is its GUI. UE4SS is runtime scripting.
    /// specSB2modTool targets .NET Framework 4.8; current UAssetAPI/UAssetGUI builds target modern .NET — full in-process embed is optional via local DLL. This bridge does not embed UAssetAPI by default — it documents workflows and discovers optional DLLs.
    /// </summary>
    public static class UAssetApiBridge
    {
        public static string FindLocalUAssetApiDll()
        {
            string[] roots =
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source"),
                @"C:\dev",
                ConfigService.ModsOutputPath
            };
            foreach (string root in roots)
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;
                try
                {
                    foreach (string f in Directory.GetFiles(root, "UAssetAPI.dll", SearchOption.AllDirectories))
                        return f;
                }
                catch { }
            }
            return null;
        }

        public static string StatusReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(DetailedGuide());
            sb.AppendLine();
            sb.AppendLine("=== Local paths ===");
            sb.AppendLine("UAssetGUI: " + (File.Exists(ConfigService.UAssetGuiPath) ? ConfigService.UAssetGuiPath : "(not set)"));
            string dll = FindLocalUAssetApiDll();
            sb.AppendLine("UAssetAPI.dll: " + (dll ?? "(not found — optional)"));
            sb.AppendLine("Vanilla core edits: " + (ConfigService.AllowVanillaCoreEdits ? "ALLOWED (dangerous)" : "blocked (safe default)"));
            return sb.ToString();
        }

        public static string DetailedGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("UAssetAPI reflection & cooked assets (UE 4.27 / SpaceBourne 2)");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("Library: atenfyr/UAssetAPI — low-level read/write of .uasset (+ paired .uexp).");
            sb.AppendLine("GUI: UAssetGUI wraps the same serializers. Always set EngineVersion VER_UE4_27 for SB2.");
            sb.AppendLine();
            sb.AppendLine("What UAssetAPI can do:");
            sb.AppendLine("  • Name map, imports, exports (NormalExport, etc.)");
            sb.AppendLine("  • 100+ property types (Float, Int, Bool, Name, Str, Object, Array, Struct, …)");
            sb.AppendLine("  • Edit CDO / export properties by name, e.g. export[\"SpeedMaximum\"]");
            sb.AppendLine("  • JSON export/import (proprietary format aiming at binary equality)");
            sb.AppendLine("  • Raw Kismet (Blueprint) bytecode read/write (advanced, fragile)");
            sb.AppendLine("  • .usmap mappings for unversioned properties (UE4SS can dump .usmap)");
            sb.AppendLine();
            sb.AppendLine("Typical C# pattern (external tool / future plugin):");
            sb.AppendLine("  var asset = new UAsset(path, EngineVersion.VER_UE4_27);");
            sb.AppendLine("  var exp = (NormalExport)asset.Exports[i];");
            sb.AppendLine("  var prop = (FloatPropertyData)exp[\"SomeStat\"]; prop.Value = 999f;");
            sb.AppendLine("  asset.Write(outPath);  // keep matching .uexp");
            sb.AppendLine();
            sb.AppendLine("Limits / risks:");
            sb.AppendLine("  • Some 4.27 assets fail binary-equality until name map / mappings are fixed");
            sb.AppendLine("  • Always keep .uasset + .uexp (+ .ubulk) together");
            sb.AppendLine("  • Prefer overriding existing package paths (AssetRegistry)");
            sb.AppendLine();
            sb.AppendLine("UE4SS scripting (runtime reflection — not file packing)");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("UE4SS injects into the game process and exposes Lua (and C++) to live UObjects.");
            sb.AppendLine("Install into the game binaries folder (version matched to the game). Capabilities:");
            sb.AppendLine("  • FindFirstOf / FindAllOf reflected classes");
            sb.AppendLine("  • Read/write properties via reflection; call UFunctions");
            sb.AppendLine("  • RegisterHook on UFunctions (pre/post) — wraps ProcessEvent on game thread");
            sb.AppendLine("  • NotifyOnNewObject / InitGameState hooks for spawn/load timing");
            sb.AppendLine("  • RegisterKeyBind is off game-thread — use ExecuteInGameThread for UObject work");
            sb.AppendLine("  • C++ mods + Blueprint mod loader optional; .usmap dump helps UAssetAPI");
            sb.AppendLine("  • Live property viewer/editor, .usmap dumper, header dumpers");
            sb.AppendLine("  • Blueprint mod loader (spawn BP mods without replacing base PAK)");
            sb.AppendLine();
            sb.AppendLine("Minimal Lua mod layout:");
            sb.AppendLine("  UE4SS/Mods/MyMod/scripts/main.lua");
            sb.AppendLine("  enable in Mods/mods.txt  →  MyMod : 1");
            sb.AppendLine();
            sb.AppendLine("SB2 workflow split:");
            sb.AppendLine("  FILE MODS  → FModel → UAssetGUI/UAssetAPI → z_*_P.pak → Mod Loader");
            sb.AppendLine("  LIVE MODS  → UE4SS Lua/C++ (stats, hooks) — optional; may break on updates");
            sb.AppendLine();
            sb.AppendLine("specSB2modTool integrates file side today; UE4SS is documented for advanced users.");
            return sb.ToString();
        }

        public static void OpenUAssetGui(string optionalAssetPath = null)
        {
            ExternalToolLauncher.Launch("uassetgui");
        }
    }
}
