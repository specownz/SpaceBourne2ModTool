using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Trainer / Cheat Engine documentation and stubs for SpaceBourne 2.
    ///
    /// Based on:
    /// - Existing public trainers (WeMod 26+ options, FearlessRevolution CE tables, etc.)
    /// - Current CE pointer-scan workflow for Unreal Engine 4 games
    /// - Community UE4 pointer guides (GWorld, persistent level offset 0x30, AOB patterns)
    ///
    /// Process name: SB2-Win64-Shipping.exe
    /// Engine:       UE 4.27
    /// </summary>
    public static class TrainerTemplateGenerator
    {
        public static List<TrainerOption> GetDefaultOptions()
        {
            return new List<TrainerOption>
            {
                // Player
                new TrainerOption { Id = "inf_health", DisplayName = "Unlimited Player Health", Category = "Player", IsToggle = true, ValueType = TrainerValueType.BooleanToggle, Description = "Freeze health at MaxHealth each tick." },
                new TrainerOption { Id = "inf_armor", DisplayName = "Unlimited Player Armor", Category = "Player", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "inf_energy", DisplayName = "Unlimited Player Energy", Category = "Player", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "inf_stamina", DisplayName = "Unlimited Player Stamina", Category = "Player", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "add_health", DisplayName = "Add 100 Health", Category = "Player", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 100 },
                new TrainerOption { Id = "add_maxhealth", DisplayName = "Add 100 Max Health", Category = "Player", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 100 },
                new TrainerOption { Id = "add_armor", DisplayName = "Add 100 Armor", Category = "Player", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 100 },
                new TrainerOption { Id = "health_regen", DisplayName = "Edit Health Regen", Category = "Player", ValueType = TrainerValueType.IntegerSet },
                new TrainerOption { Id = "damage_resist", DisplayName = "Edit Damage Resistance", Category = "Player", ValueType = TrainerValueType.IntegerSet },

                // Ship
                new TrainerOption { Id = "inf_ship_shield", DisplayName = "Unlimited Ship Shield", Category = "Ship", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "inf_ship_armor", DisplayName = "Unlimited Ship Armor / Hull", Category = "Ship", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "inf_ship_energy", DisplayName = "Unlimited Ship Energy", Category = "Ship", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "inf_thruster", DisplayName = "Unlimited Thruster Fuel / Boost", Category = "Ship", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },
                new TrainerOption { Id = "ship_shield_max", DisplayName = "Edit Max Ship Shield", Category = "Ship", ValueType = TrainerValueType.IntegerSet },

                // Weapons
                new TrainerOption { Id = "no_overheat", DisplayName = "No Weapons Overheat", Category = "Weapons", IsToggle = true, ValueType = TrainerValueType.BooleanToggle, Description = "Usually a float or bool on the weapon component; freeze or NOP the overheat add." },
                new TrainerOption { Id = "no_reload", DisplayName = "No Reload", Category = "Weapons", IsToggle = true, ValueType = TrainerValueType.BooleanToggle },

                // Stats / Economy
                new TrainerOption { Id = "add_money", DisplayName = "Add 100K Money", Category = "Stats", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 100000 },
                new TrainerOption { Id = "sub_money", DisplayName = "Subtract 100K Money", Category = "Stats", ValueType = TrainerValueType.IntegerAdd, DefaultValue = -100000 },
                new TrainerOption { Id = "add_pilot", DisplayName = "Add 10 Pilot Attribute Points", Category = "Stats", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 10 },
                new TrainerOption { Id = "add_soldier", DisplayName = "Add 10 Soldier Attribute Points", Category = "Stats", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 10 },
                new TrainerOption { Id = "add_adventurer", DisplayName = "Add 10 Adventurer / Explorer Points", Category = "Stats", ValueType = TrainerValueType.IntegerAdd, DefaultValue = 10 },
                new TrainerOption { Id = "edit_xp", DisplayName = "Edit XP / Level", Category = "Stats", ValueType = TrainerValueType.IntegerSet },

                // Game
                new TrainerOption { Id = "game_speed", DisplayName = "Game Speed Multiplier", Category = "Game", ValueType = TrainerValueType.FloatMultiplier, DefaultValue = 1.0f, Description = "1.0 = normal. Often found near world time dilation or custom speed vars." },
                new TrainerOption { Id = "player_speed", DisplayName = "Player Move Speed Multiplier", Category = "Game", ValueType = TrainerValueType.FloatMultiplier, DefaultValue = 1.0f },
                new TrainerOption { Id = "jump_height", DisplayName = "Jump Height Multiplier", Category = "Game", ValueType = TrainerValueType.FloatMultiplier, DefaultValue = 1.0f }
            };
        }

        public static string GenerateCheatEngineTableStub(List<TrainerOption> options, string outputPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<!--");
            sb.AppendLine("  SpaceBourne 2 – Cheat Engine Table STUB (UE 4.27)");
            sb.AppendLine("  Generated by SpaceBourne 2 Mod Creation Tool");
            sb.AppendLine();
            sb.AppendLine("  Process: SB2-Win64-Shipping.exe");
            sb.AppendLine();
            sb.AppendLine("  THIS IS NOT A WORKING TABLE.");
            sb.AppendLine("  You must perform live value scans + pointer scans for the current build.");
            sb.AppendLine("  Community tables (FearlessRevolution, WeMod, etc.) are the best live reference.");
            sb.AppendLine();
            sb.AppendLine("  Recommended CE workflow for UE4 games:");
            sb.AppendLine("  1. Attach to SB2-Win64-Shipping.exe while in a stable state (planet / ship).");
            sb.AppendLine("  2. Scan for known values (health, money, …) → Next Scan after change.");
            sb.AppendLine("  3. When you have a candidate address, right-click → Pointer scan for this address.");
            sb.AppendLine("  4. Defaults that work well for UE4:");
            sb.AppendLine("       Max level = 5–7");
            sb.AppendLine("       Max offset = 2048 (or higher if needed)");
            sb.AppendLine("       Addresses must be 32-bit aligned = ON");
            sb.AppendLine("  5. Rescan after level change / restart to filter invalid paths.");
            sb.AppendLine("  6. Prefer short paths that start from a static module address.");
            sb.AppendLine("  7. Optional: use UE4SS / UE structure dump tables to locate GWorld,");
            sb.AppendLine("     then walk PersistentLevel (often +0x30) → Actors → your pawn.");
            sb.AppendLine("  8. For \"unlimited\" cheats, write an AA script that freezes the value");
            sb.AppendLine("     or restores Max every frame instead of a one-shot write.");
            sb.AppendLine("-->");
            sb.AppendLine("<CheatTable>");
            sb.AppendLine("  <CheatEntries>");

            int id = 1;
            foreach (var opt in options)
            {
                sb.AppendLine($"    <!-- [{opt.Category}] {opt.DisplayName} -->");
                sb.AppendLine("    <CheatEntry>");
                sb.AppendLine($"      <ID>{id++}</ID>");
                sb.AppendLine($"      <Description>\"{EscapeXml(opt.DisplayName)}\"</Description>");
                sb.AppendLine("      <LastState Value=\"0\" RealAddress=\"00000000\"/>");
                sb.AppendLine("      <VariableType>4 Bytes</VariableType>");
                sb.AppendLine("      <Address>SB2-Win64-Shipping.exe+OFFSET_HERE</Address>");
                sb.AppendLine($"      <!-- Type hint: {opt.ValueType} | Toggle={opt.IsToggle} -->");
                if (!string.IsNullOrEmpty(opt.Description))
                    sb.AppendLine($"      <!-- {EscapeXml(opt.Description)} -->");
                sb.AppendLine("    </CheatEntry>");
            }

            sb.AppendLine("  </CheatEntries>");
            sb.AppendLine("</CheatTable>");

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            LoggerService.Log("Generated CE table stub: " + outputPath);
            return outputPath;
        }

        public static string GenerateMarkdownGuide(List<TrainerOption> options, string outputPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# SpaceBourne 2 Trainer & Pointer-Scan Guide");
            sb.AppendLine();
            sb.AppendLine("Generated by the SpaceBourne 2 Mod Creation Tool.");
            sb.AppendLine();
            sb.AppendLine("## Process & Engine");
            sb.AppendLine("- Executable: `SB2-Win64-Shipping.exe`");
            sb.AppendLine("- Engine: Unreal Engine **4.27**");
            sb.AppendLine("- PAK encryption: **none** (file mods are straightforward)");
            sb.AppendLine();
            sb.AppendLine("## Cheat Engine Pointer Scan – Practical Workflow (UE4)");
            sb.AppendLine();
            sb.AppendLine("1. **Find a dynamic address**");
            sb.AppendLine("   - Start the game, reach a stable state (on foot or in ship).");
            sb.AppendLine("   - Attach CE → scan for the current value (Exact Value, 4 Bytes / Float).");
            sb.AppendLine("   - Change the value in-game → Next Scan → repeat until few results.");
            sb.AppendLine("   - Add the correct address to the list.");
            sb.AppendLine();
            sb.AppendLine("2. **Pointer scan**");
            sb.AppendLine("   - Right-click the address → *Pointer scan for this address*.");
            sb.AppendLine("   - Recommended starting options for UE4:");
            sb.AppendLine("     - Max level: **5–7**");
            sb.AppendLine("     - Max offset value: **2048** (raise if needed)");
            sb.AppendLine("     - Addresses must be 32-bit aligned: **ON**");
            sb.AppendLine("     - Only loop the static addresses in the module: optional but faster");
            sb.AppendLine("   - Save the `.ptr` results.");
            sb.AppendLine();
            sb.AppendLine("3. **Rescan / filter**");
            sb.AppendLine("   - Change level or restart the game (forces new dynamic addresses).");
            sb.AppendLine("   - Re-attach, find the value again, then *Rescan memory* on the pointer list.");
            sb.AppendLine("   - Prefer the **shortest** path that still resolves after restart.");
            sb.AppendLine();
            sb.AppendLine("4. **UE-specific tips**");
            sb.AppendLine("   - Many UE4 games expose `GWorld` / `UWorld` via AOB or known patterns.");
            sb.AppendLine("   - From UWorld you can often reach `PersistentLevel` (community notes frequently cite offset **+0x30** on older builds – always verify).");
            sb.AppendLine("   - From the level you walk the actor array to the player pawn / ship.");
            sb.AppendLine("   - Tools that help: UE4SS (object dump), UE structure dump CE tables, ReClass.NET.");
            sb.AppendLine("   - AOB (Array of Bytes) scans are more stable across patches than hard-coded module offsets.");
            sb.AppendLine();
            sb.AppendLine("5. **Unlimited / freeze style**");
            sb.AppendLine("   - Prefer an Auto Assembler script that writes MaxHealth (or whatever) every frame,");
            sb.AppendLine("     or that NOPs the instruction that subtracts damage.");
            sb.AppendLine("   - One-shot \"Add 100\" is simpler but less robust.");
            sb.AppendLine();
            sb.AppendLine("## Options Catalogue (from public trainers)");
            sb.AppendLine();

            string currentCat = "";
            foreach (var opt in options)
            {
                if (opt.Category != currentCat)
                {
                    currentCat = opt.Category;
                    sb.AppendLine($"### {currentCat}");
                    sb.AppendLine();
                }
                sb.AppendLine($"- **{opt.DisplayName}**");
                sb.AppendLine($"  - Type: `{opt.ValueType}`");
                if (opt.IsToggle) sb.AppendLine("  - Style: Toggle / freeze");
                if (opt.DefaultValue != null) sb.AppendLine($"  - Typical value: `{opt.DefaultValue}`");
                if (!string.IsNullOrEmpty(opt.Description)) sb.AppendLine($"  - Notes: {opt.Description}");
                sb.AppendLine();
            }

            sb.AppendLine("## Safety");
            sb.AppendLine("- Single-player only.");
            sb.AppendLine("- Back up save files before experimenting.");
            sb.AppendLine("- Offsets break on almost every game update – expect to re-scan.");
            sb.AppendLine("- Do not distribute working offset tables that contain live addresses without a clear version note.");

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            return outputPath;
        }

        public static string GeneratePointerScanChecklist(string outputPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SpaceBourne 2 – Cheat Engine Pointer Scan Checklist");
            sb.AppendLine("==================================================");
            sb.AppendLine();
            sb.AppendLine("[ ] Game running, CE attached to SB2-Win64-Shipping.exe");
            sb.AppendLine("[ ] Stable location (not during cutscene / loading)");
            sb.AppendLine("[ ] Value scan complete → candidate address in address list");
            sb.AppendLine("[ ] Pointer scan started (Max level 5–7, Max offset ≥ 2048)");
            sb.AppendLine("[ ] .ptr file saved");
            sb.AppendLine("[ ] Level change OR full restart performed");
            sb.AppendLine("[ ] Value found again after restart");
            sb.AppendLine("[ ] Rescan pointer list → only valid paths remain");
            sb.AppendLine("[ ] Shortest reliable path selected");
            sb.AppendLine("[ ] Optional: verify path still works after a second restart");
            sb.AppendLine("[ ] For freeze cheats: AA script written & tested");
            sb.AppendLine("[ ] Version number of the game recorded next to the table");
            sb.AppendLine();
            sb.AppendLine("UE4 extras (optional but powerful):");
            sb.AppendLine("[ ] Locate GWorld via AOB or structure dump table");
            sb.AppendLine("[ ] Walk PersistentLevel → Actors → Player / Ship");
            sb.AppendLine("[ ] Document offsets for health, money, etc. relative to pawn");
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            return outputPath;
        }

        private static string EscapeXml(string s)
        {
            if (s == null) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
