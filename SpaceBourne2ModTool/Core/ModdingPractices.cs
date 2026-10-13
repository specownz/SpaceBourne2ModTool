using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Current (2024–2026) best practices for modding Unreal Engine 4.27 games,
    /// specialised for SpaceBourne 2.
    /// Sources: community guides (Dmgvol, Buckminsterfullerene, FModel docs),
    /// Epic 4.27 patching docs, and observed trainer / CE workflows.
    /// </summary>
    public static class ModdingPractices
    {
        public static string GetFullGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("══════════════════════════════════════════════════════════════");
            sb.AppendLine("  SpaceBourne 2 / UE 4.27 – Current Modding Practices");
            sb.AppendLine("══════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("1. TOOLCHAIN (recommended order)");
            sb.AppendLine("   • FModel          – browse & export raw .uasset/.uexp/.ubulk");
            sb.AppendLine("   • UAssetGUI       – edit cooked assets (set UE version 4.27)");
            sb.AppendLine("   • UnrealPak 4.27  – pack (this tool automates the response file)");
            sb.AppendLine("   • Optional: repak – faster alternative packer with version control");
            sb.AppendLine("   • QuickBMS + locres tool – localization only");
            sb.AppendLine("   • Cheat Engine 7.x + optional UE4SS – runtime trainers");
            sb.AppendLine();
            sb.AppendLine("2. FOLDER STRUCTURE (critical)");
            sb.AppendLine("   Mod folder MUST end with _P and mirror the game:");
            sb.AppendLine();
            sb.AppendLine("     MyMod_P\\");
            sb.AppendLine("       SB2\\");
            sb.AppendLine("         Content\\");
            sb.AppendLine("           <same paths you see in FModel>");
            sb.AppendLine();
            sb.AppendLine("   Inside the .pak the paths become:");
            sb.AppendLine("     ../../../SB2/Content/...");
            sb.AppendLine("   (mount point ../../../ relative to Binaries/Win64)");
            sb.AppendLine("   High-value SB2/Content folders (analyzer 2026-09-27):");
            sb.AppendLine("     1Data, Packes, Packes_L_D, Armors, 1Meshs, Locomotion,");
            sb.AppendLine("     1Galaxy, 1Widgets, _Textures, BallisticsVFX, FMOD, 111Cinematic");
            sb.AppendLine();
            sb.AppendLine("3. PACKING RULES (UE 4.27)");
            sb.AppendLine("   • Filename MUST end with _P.pak  → engine treats it as a patch");
            sb.AppendLine("     and gives it higher priority so it overrides vanilla assets.");
            sb.AppendLine("   • Use a response file (not just -Create=folder) so mount paths");
            sb.AppendLine("     are explicit and correct.");
            sb.AppendLine("   • Command:");
            sb.AppendLine("       UnrealPak.exe out.pak -Create=paklist.txt -compress");
            sb.AppendLine("   • Chunk / sort order: higher numbers and later alphabet load later.");
            sb.AppendLine("     This tool defaults to pakchunk1000+ so it sits after vanilla.");
            sb.AppendLine("   • No AES key is present in SpaceBourne 2 – do not encrypt.");
            sb.AppendLine();
            sb.AppendLine("4. ASSET EDITING WORKFLOW");
            sb.AppendLine("   a. FModel → Export Raw Data (.uasset)  (keeps .uexp / .ubulk)");
            sb.AppendLine("   b. Open the .uasset in UAssetGUI (UE 4.27)");
            sb.AppendLine("   c. Edit → Save (keep companion .uexp/.ubulk next to it)");
            sb.AppendLine("   d. Drop the trio into the matching path under MyMod_P\\SB2\\Content\\");
            sb.AppendLine("   e. Pack with this tool");
            sb.AppendLine();
            sb.AppendLine("5. LOCALIZATION");
            sb.AppendLine("   • Extract Game.locres with QuickBMS (4.27 script)");
            sb.AppendLine("   • Decode with unreal_locres_texttool (Unicode/Export)");
            sb.AppendLine("   • Edit .txt → Import → place at");
            sb.AppendLine("       SB2\\Content\\Localization\\Game\\{lang}\\Game.locres");
            sb.AppendLine("   • Prefer a language code the base game does not already ship");
            sb.AppendLine();
            sb.AppendLine("6. RUNTIME TRAINERS (Cheat Engine)");
            sb.AppendLine("   • Process: SB2-Win64-Shipping.exe");
            sb.AppendLine("   • Classic value scan → pointer scan (max level 5–7)");
            sb.AppendLine("   • Rescan after restart / level change");
            sb.AppendLine("   • UE helpers: GWorld AOB, PersistentLevel often +0x30 (verify!),");
            sb.AppendLine("     UE4SS object dump, community structure tables");
            sb.AppendLine("   • Prefer AA freeze scripts over one-shot writes for \"unlimited\"");
            sb.AppendLine();
            sb.AppendLine("7. IoStore NOTE");
            sb.AppendLine("   Some 4.26/4.27 titles also ship .utoc/.ucas. SpaceBourne 2 uses");
            sb.AppendLine("   classic .pak files, so the workflow above is sufficient. If a");
            sb.AppendLine("   future update moves to IoStore, tools such as retoc / UnrealReZen");
            sb.AppendLine("   would be required.");
            sb.AppendLine();
            sb.AppendLine("8. TESTING");
            sb.AppendLine("   • Remove other mods while testing");
            sb.AppendLine("   • Verify with UnrealPak.exe your.pak -List  (check mount point)");
            sb.AppendLine("   • Back up saves – broken assets can crash or corrupt progress");
            sb.AppendLine();
            sb.AppendLine("This guide is embedded in the Mod Creation Tool and written to the");
            sb.AppendLine("project folder when you create a new mod.");
            return sb.ToString();
        }
    }
}
