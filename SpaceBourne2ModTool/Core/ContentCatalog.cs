using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Embedded snapshot of real SpaceBourne 2 content layout from Game Analyzer
    /// (v1.4, 2026-09-27, 85960 list lines). Used when the user has not just run
    /// a fresh analysis, and for scaffolding / Content Browser presets.
    /// </summary>
    public static class ContentCatalog
    {
        public class FolderInfo
        {
            public string Name { get; set; }
            public int ApproxCount { get; set; }
            public string Role { get; set; }
            public string[] SamplePaths { get; set; }
            public string[] SubHints { get; set; }
        }

        /// <summary>
        /// Named scaffold presets for high-value subfolders (from analyzer depth-2).
        /// StagingPath is relative to SB2/Content/.
        /// </summary>
        public class NamedPreset
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string StagingPath { get; set; }  // e.g. 1Data/NPCs
            public int ApproxCount { get; set; }
            public string Role { get; set; }
            public string[] SamplePaths { get; set; }
            public string[] ExtraSubdirs { get; set; }
        }

        public static readonly NamedPreset[] Presets =
        {
            // --- High-level targets ---
            new NamedPreset
            {
                Id = "npcs",
                DisplayName = "1Data / NPCs (dialogs & NPC data)",
                StagingPath = "1Data/NPCs",
                ApproxCount = 2832,
                Role = "NPC dialogs and data — largest 1Data subfolder",
                ExtraSubdirs = new[] { "Dialogs", "Dialogs/Angelas" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/NPCs/Dialogs/Angelas/1_PlayerLookAtTalker.uasset",
                    "SB2/Content/1Data/NPCs/Dialogs/Angelas/AddRemove_Money.uasset",
                }
            },
            new NamedPreset
            {
                Id = "npc_dialogs",
                DisplayName = "1Data / NPCs / Dialogs",
                StagingPath = "1Data/NPCs/Dialogs",
                ApproxCount = 2832,
                Role = "All NPC dialog trees (Angelas and others)",
                ExtraSubdirs = new[] { "Angelas", "Angelas/Alaysia" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/NPCs/Dialogs/Angelas/1_PlayerLookAtTalker.uasset",
                    "SB2/Content/1Data/NPCs/Dialogs/Angelas/Alaysia/A_Drane_2.uasset",
                }
            },
            new NamedPreset
            {
                Id = "ccg",
                DisplayName = "1CCG card game (new update)",
                StagingPath = "1CCG",
                ApproxCount = 346,
                Role = "Collectible card content added in the latest update: cards, frames, parallax",
                ExtraSubdirs = new[] { "Cards", "Cards/Materials", "Cards/Textures", "Frames", "Parallax" },
                SamplePaths = new[] { "SB2/Content/1CCG/Cards/", "SB2/Content/1CCG/Frames/", "SB2/Content/1CCG/Parallax/" }
            },
            new NamedPreset
            {
                Id = "dlc_quest",
                DisplayName = "DLC quests and voices",
                StagingPath = "DLCQuest",
                ApproxCount = 254,
                Role = "DLC quest chain 0Begin / 0Find1 / 0Find2 / GetShip, including new voice and sound folders",
                ExtraSubdirs = new[] { "0Begin", "0Begin/Sounds", "0Find1", "0Find1/Voices", "0Find2", "0Find2/Voices", "GetShip" },
                SamplePaths = new[] { "SB2/Content/DLCQuest/0Begin/", "SB2/Content/DLCQuest/GetShip/" }
            },
            new NamedPreset
            {
                Id = "news_widgets",
                DisplayName = "News widgets",
                StagingPath = "1Widgets/News",
                ApproxCount = 389,
                Role = "In-game news UI expanded in the update (operations, solar systems, textures)",
                ExtraSubdirs = new[] { "Operations", "Textures", "SolarSystems" },
                SamplePaths = new[] { "SB2/Content/1Widgets/News/" }
            },
            new NamedPreset
            {
                Id = "main_menu_kit",
                DisplayName = "Main menu kit",
                StagingPath = "Packes/ProMainMenuKit",
                ApproxCount = 79,
                Role = "ProMainMenuKit textures added with the update",
                ExtraSubdirs = new[] { "Textures" },
                SamplePaths = new[] { "SB2/Content/Packes/ProMainMenuKit/Textures/" }
            },
            new NamedPreset
            {
                Id = "rpg_curves",
                DisplayName = "RPG curves",
                StagingPath = "1Data/1RPG/Curves",
                ApproxCount = 60,
                Role = "New RPG curve assets under 1Data/1RPG — balance and progression numbers",
                SamplePaths = new[] { "SB2/Content/1Data/1RPG/Curves/" }
            },
            new NamedPreset
            {
                Id = "diplomacy",
                DisplayName = "Player faction diplomacy",
                StagingPath = "1Data/1_PlayerFaction/PF_Diplomacy",
                ApproxCount = 100,
                Role = "Diplomacy data grew in the update",
                SamplePaths = new[] { "SB2/Content/1Data/1_PlayerFaction/PF_Diplomacy/" }
            },
            new NamedPreset
            {
                Id = "major_armor",
                DisplayName = "Armors / Majorcharacters (all)",
                StagingPath = "Armors/Majorcharacters",
                ApproxCount = 1637,
                Role = "All named major character folders combined",
                SamplePaths = new[] { "SB2/Content/Armors/Majorcharacters/" }
            },
            new NamedPreset
            {
                Id = "char_B_Old_Lady",
                DisplayName = "Majorcharacter / B_Old_Lady",
                StagingPath = "Armors/Majorcharacters/B_Old_Lady",
                ApproxCount = 212,
                Role = "Named major character visual (~212 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "Materials/Old_Lady_head" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/B_Old_Lady/Materials/Old_Lady_head/High_polytail_Inst.uasset",
                    "SB2/Content/Armors/Majorcharacters/B_Old_Lady/Materials/Old_Lady_head/Std_Cornea_L_LWHQ_Inst.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_N_GuzelWoman",
                DisplayName = "Majorcharacter / N_GuzelWoman",
                StagingPath = "Armors/Majorcharacters/N_GuzelWoman",
                ApproxCount = 209,
                Role = "Named major character visual (~209 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "Guzel1_fbm" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_GuzelWoman/Guzel1.uasset",
                    "SB2/Content/Armors/Majorcharacters/N_GuzelWoman/Guzel1_fbm/Fade_Mohawk_Hair_Bump.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_N_OldEvil",
                DisplayName = "Majorcharacter / N_OldEvil",
                StagingPath = "Armors/Majorcharacters/N_OldEvil",
                ApproxCount = 183,
                Role = "Named major character visual (~183 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "Materials/OldEvilhead" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_OldEvil/Materials/OldEvilhead/Std_Cornea_L_LWHQ_Inst.uasset",
                    "SB2/Content/Armors/Majorcharacters/N_OldEvil/Materials/OldEvilhead/Std_Cornea_R_LWHQ_Inst.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_N_Good_Boy",
                DisplayName = "Majorcharacter / N_Good_Boy",
                StagingPath = "Armors/Majorcharacters/N_Good_Boy",
                ApproxCount = 176,
                Role = "Named major character visual (~176 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "GmaleHead_fbm" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_Good_Boy/GmaleHead.uasset",
                    "SB2/Content/Armors/Majorcharacters/N_Good_Boy/GmaleHead_fbm/Std_Cornea_L_Diffuse.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_N_Cinlii",
                DisplayName = "Majorcharacter / N_Cinlii",
                StagingPath = "Armors/Majorcharacters/N_Cinlii",
                ApproxCount = 173,
                Role = "Named major character visual (~173 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "Cinli_Head_fbm" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_Cinlii/Cinli_Head.uasset",
                    "SB2/Content/Armors/Majorcharacters/N_Cinlii/Cinli_Head_fbm/Std_Cornea_L_Diffuse.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_N_GWomen",
                DisplayName = "Majorcharacter / N_GWomen",
                StagingPath = "Armors/Majorcharacters/N_GWomen",
                ApproxCount = 173,
                Role = "Named major character visual (~173 assets) — head/materials/anims",
                ExtraSubdirs = null,
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_GWomen/"
                }
            },
            new NamedPreset
            {
                Id = "char_N_Zayif",
                DisplayName = "Majorcharacter / N_Zayif",
                StagingPath = "Armors/Majorcharacters/N_Zayif",
                ApproxCount = 173,
                Role = "Named major character visual (~173 assets) — head/materials/anims",
                ExtraSubdirs = null,
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_Zayif/"
                }
            },
            new NamedPreset
            {
                Id = "char_N_RoboGirl",
                DisplayName = "Majorcharacter / N_RoboGirl",
                StagingPath = "Armors/Majorcharacters/N_RoboGirl",
                ApproxCount = 165,
                Role = "Named major character visual (~165 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "FaceAnimBP" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/N_RoboGirl/DraneCinamatic.uasset",
                    "SB2/Content/Armors/Majorcharacters/N_RoboGirl/FaceAnimBP/Drane_Idle.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_Good_OldMan",
                DisplayName = "Majorcharacter / Good_OldMan",
                StagingPath = "Armors/Majorcharacters/Good_OldMan",
                ApproxCount = 158,
                Role = "Named major character visual (~158 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "GoodOldManHead_fbm" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/Good_OldMan/GoodOldManHead.uasset",
                    "SB2/Content/Armors/Majorcharacters/Good_OldMan/GoodOldManHead_fbm/Std_Cornea_L_Diffuse.uasset"
                }
            },
            new NamedPreset
            {
                Id = "char_Sister",
                DisplayName = "Majorcharacter / Sister",
                StagingPath = "Armors/Majorcharacters/Sister",
                ApproxCount = 15,
                Role = "Named major character visual (~15 assets) — head/materials/anims",
                ExtraSubdirs = new[] { "1" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/Majorcharacters/Sister/1/BaseSister1.uasset",
                    "SB2/Content/Armors/Majorcharacters/Sister/1/BaseSister_PhysicsAsset.uasset"
                }
            },
            new NamedPreset
            {
                Id = "newset_M_1",
                DisplayName = "Armor set 1_New_sets / M_1",
                StagingPath = "Armors/1_New_sets/M_1",
                ApproxCount = 45,
                Role = "Modular armor set M_1 (~45 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_1/" }
            },
            new NamedPreset
            {
                Id = "newset_M_2",
                DisplayName = "Armor set 1_New_sets / M_2",
                StagingPath = "Armors/1_New_sets/M_2",
                ApproxCount = 47,
                Role = "Modular armor set M_2 (~47 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_2/" }
            },
            new NamedPreset
            {
                Id = "newset_M_3",
                DisplayName = "Armor set 1_New_sets / M_3",
                StagingPath = "Armors/1_New_sets/M_3",
                ApproxCount = 38,
                Role = "Modular armor set M_3 (~38 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_3/" }
            },
            new NamedPreset
            {
                Id = "newset_M_4",
                DisplayName = "Armor set 1_New_sets / M_4",
                StagingPath = "Armors/1_New_sets/M_4",
                ApproxCount = 36,
                Role = "Modular armor set M_4 (~36 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_4/" }
            },
            new NamedPreset
            {
                Id = "newset_M_5",
                DisplayName = "Armor set 1_New_sets / M_5",
                StagingPath = "Armors/1_New_sets/M_5",
                ApproxCount = 39,
                Role = "Modular armor set M_5 (~39 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_5/" }
            },
            new NamedPreset
            {
                Id = "newset_M_6",
                DisplayName = "Armor set 1_New_sets / M_6",
                StagingPath = "Armors/1_New_sets/M_6",
                ApproxCount = 34,
                Role = "Modular armor set M_6 (~34 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/M_6/" }
            },
            new NamedPreset
            {
                Id = "newset_F_1",
                DisplayName = "Armor set 1_New_sets / F_1",
                StagingPath = "Armors/1_New_sets/F_1",
                ApproxCount = 37,
                Role = "Modular armor set F_1 (~37 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/F_1/" }
            },
            new NamedPreset
            {
                Id = "newset_F_2",
                DisplayName = "Armor set 1_New_sets / F_2",
                StagingPath = "Armors/1_New_sets/F_2",
                ApproxCount = 37,
                Role = "Modular armor set F_2 (~37 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/F_2/" }
            },
            new NamedPreset
            {
                Id = "newset_F_3",
                DisplayName = "Armor set 1_New_sets / F_3",
                StagingPath = "Armors/1_New_sets/F_3",
                ApproxCount = 41,
                Role = "Modular armor set F_3 (~41 assets)",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/F_3/" }
            },
            new NamedPreset
            {
                Id = "armor_sets_root",
                DisplayName = "Armors / 1_New_sets (all)",
                StagingPath = "Armors/1_New_sets",
                ApproxCount = 356,
                Role = "All modular armor sets F_* / M_*",
                SamplePaths = new[] { "SB2/Content/Armors/1_New_sets/" }
            },
            new NamedPreset
            {
                Id = "armor_base_male_head",
                DisplayName = "Armors / Base_Male_Human_Head",
                StagingPath = "Armors/Base_Male_Human_Head",
                ApproxCount = 448,
                Role = "Base male human head",
                SamplePaths = new[] { "SB2/Content/Armors/Base_Male_Human_Head/" }
            },
            new NamedPreset
            {
                Id = "armor_set_3",
                DisplayName = "Armors / Set_3",
                StagingPath = "Armors/Set_3",
                ApproxCount = 139,
                Role = "Armor set 3 (textures/materials)",
                ExtraSubdirs = new[] { "Textures", "Materials" },
                SamplePaths = new[] { "SB2/Content/Armors/Set_3/" }
            },
            new NamedPreset
            {
                Id = "armor_set_5",
                DisplayName = "Armors / Set_5",
                StagingPath = "Armors/Set_5",
                ApproxCount = 137,
                Role = "Armor set 5",
                ExtraSubdirs = new[] { "Textures", "Materials" },
                SamplePaths = new[] { "SB2/Content/Armors/Set_5/" }
            },
            new NamedPreset
            {
                Id = "armor_set_female",
                DisplayName = "Armors / Set_Female",
                StagingPath = "Armors/Set_Female",
                ApproxCount = 95,
                Role = "Female armor set",
                ExtraSubdirs = new[] { "Textures", "Materials" },
                SamplePaths = new[] { "SB2/Content/Armors/Set_Female/" }
            },
            new NamedPreset
            {
                Id = "quests",
                DisplayName = "1Data / Quests",
                StagingPath = "1Data/Quests",
                ApproxCount = 331,
                Role = "Quest definitions",
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/Quests/00_NewFirstThing.uasset",
                    "SB2/Content/1Data/Quests/10_FreeLancersGuild.uasset",
                }
            },
            new NamedPreset
            {
                Id = "weapons_data",
                DisplayName = "1Data / Weapons",
                StagingPath = "1Data/Weapons",
                ApproxCount = 284,
                Role = "Weapon data and damage types",
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/Weapons/Damage_Types.uasset",
                    "SB2/Content/1Data/Weapons/Back_Shooter_Anka.uasset",
                }
            },
            new NamedPreset
            {
                Id = "ships_data",
                DisplayName = "1Data / SB2Ships",
                StagingPath = "1Data/SB2Ships",
                ApproxCount = 227,
                Role = "Ship/cockpit data",
                ExtraSubdirs = new[] { "Cockpit" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/SB2Ships/Cockpit/Bonder_Cockpit.uasset",
                    "SB2/Content/1Data/SB2Ships/Cockpit/CapitalCockpit.uasset",
                }
            },
            new NamedPreset
            {
                Id = "ai_data",
                DisplayName = "1Data / 11_AI",
                StagingPath = "1Data/11_AI",
                ApproxCount = 722,
                Role = "Behavior trees, blackboards, TeamBattle",
                ExtraSubdirs = new[] { "Ammo", "TeamBattle" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/11_AI/1_VAL_New_BB.uasset",
                    "SB2/Content/1Data/11_AI/1_VAL_New_BT.uasset",
                }
            },
            new NamedPreset
            {
                Id = "building_parts",
                DisplayName = "1Data / BuildingSystem / Parts",
                StagingPath = "1Data/BuildingSystem/Parts",
                ApproxCount = 640,
                Role = "Building system parts (largest BuildingSystem child)",
                SamplePaths = new[] { "SB2/Content/1Data/BuildingSystem/Parts/" }
            },
            new NamedPreset
            {
                Id = "enemy_data",
                DisplayName = "1Data / Enemy",
                StagingPath = "1Data/Enemy",
                ApproxCount = 512,
                Role = "Enemy data assets",
                SamplePaths = new[] { "SB2/Content/1Data/Enemy/" }
            },
            new NamedPreset
            {
                Id = "warp_quest_spawners",
                DisplayName = "1Galaxy / Warp / QuestSpawners",
                StagingPath = "1Galaxy/Warp/QuestSpawners",
                ApproxCount = 516,
                Role = "Warp quest spawners",
                SamplePaths = new[] { "SB2/Content/1Galaxy/Warp/QuestSpawners/" }
            },
            new NamedPreset
            {
                Id = "locomotion_mainchar",
                DisplayName = "Locomotion / MainChar",
                StagingPath = "Locomotion/MainChar",
                ApproxCount = 862,
                Role = "Player locomotion textures/anims",
                ExtraSubdirs = new[] { "Textures", "Anims", "F_Anims" },
                SamplePaths = new[] { "SB2/Content/Locomotion/MainChar/" }
            },
            new NamedPreset
            {
                Id = "talent_tree",
                DisplayName = "1Data / Talent_Tree",
                StagingPath = "1Data/Talent_Tree",
                ApproxCount = 158,
                Role = "Talent / perk tree data",
                SamplePaths = new[] { "SB2/Content/1Data/Talent_Tree/" }
            },
            new NamedPreset
            {
                Id = "keybinds_ship",
                DisplayName = "1Data / 1_Keybings / Ship",
                StagingPath = "1Data/1_Keybings/Ship",
                ApproxCount = 204,
                Role = "Ship keybind data",
                SamplePaths = new[] { "SB2/Content/1Data/1_Keybings/Ship/" }
            },

            new NamedPreset
            {
                Id = "quests_obsesif",
                DisplayName = "1Data / Quests / Obsesif",
                StagingPath = "1Data/Quests/Obsesif",
                ApproxCount = 181,
                Role = "Largest quest subfolder (~181) from depth-3 analysis",
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/Quests/Obsesif/"
                }
            },
            new NamedPreset
            {
                Id = "main_quests",
                DisplayName = "1Data / Main_Quests",
                StagingPath = "1Data/Main_Quests",
                ApproxCount = 2,
                Role = "Main quest branch data (S_Branches)",
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/Main_Quests/S_Branches.uasset"
                }
            },
            new NamedPreset
            {
                Id = "cinematic_111",
                DisplayName = "111Cinematic (animations & dialog cams)",
                StagingPath = "111Cinematic",
                ApproxCount = 0,
                Role = "Cinematic anims, montages, DialogCam presets — high-impact visual/story mods",
                ExtraSubdirs = new[] { "Animation", "DialogCam" },
                SamplePaths = new[]
                {
                    "SB2/Content/111Cinematic/DialogCam/Preset_DialogArea.uasset",
                    "SB2/Content/111Cinematic/Animation/JackTest2.uasset"
                }
            },
            new NamedPreset
            {
                Id = "config_sb2",
                DisplayName = "SB2 Config (INI overrides)",
                StagingPath = "Config",
                ApproxCount = 7,
                Role = "DefaultGame.ini and related — stage under SB2/Config for _P.pak overrides",
                SamplePaths = new[]
                {
                    "SB2/Config/DefaultGame.ini"
                }
            },
            new NamedPreset
            {
                Id = "locres_game",
                DisplayName = "Localization (.locres)",
                StagingPath = "Localization",
                ApproxCount = 51,
                Role = "~51 locres in pak — dump/rebuild with community locres tools",
                SamplePaths = new[]
                {
                    "SB2/Content/Localization/"
                }
            },
            new NamedPreset
            {
                Id = "packes",
                DisplayName = "Packes / Packes_L_D (mesh packs)",
                StagingPath = "Packes",
                ApproxCount = 4203,
                Role = "Large mesh/material packs (~4k + Packes_L_D ~4.8k)",
                SamplePaths = new[]
                {
                    "SB2/Content/Packes/"
                }
            },
            new NamedPreset
            {
                Id = "ammo_data",
                DisplayName = "1Data / Ammo_Data",
                StagingPath = "1Data/Ammo_Data",
                ApproxCount = 0,
                Role = "Bullet types / easy-add ammo data",
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/Ammo_Data/Bullet_Types_For_Easy_Add.uasset"
                }
            },
            new NamedPreset
            {
                Id = "wex_dialogs",
                DisplayName = "THEARCHIVE / WexDialogs",
                StagingPath = "THEARCHIVE/WexDialogs",
                ApproxCount = 60,
                Role = "Archive dialog content (~60)",
                SamplePaths = new[]
                {
                    "SB2/Content/THEARCHIVE/WexDialogs/"
                }
            },

        };

        public static System.Collections.Generic.List<NamedPreset> BuildDepth3Presets(
            System.Collections.Generic.IDictionary<string, string> extraMetadata)
        {
            var list = new System.Collections.Generic.List<NamedPreset>();
            if (extraMetadata == null) return list;
            foreach (var kv in extraMetadata)
            {
                if (!kv.Key.StartsWith("folder:d3:", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                string path = kv.Key.Substring("folder:d3:".Length); // Armors/Majorcharacters/B_Old_Lady
                int count = 0;
                int.TryParse(kv.Value, out count);
                if (count <= 0) continue;
                string id = "d3_" + path.Replace('/', '_').Replace('\\', '_');
                // Skip if already a static preset with same staging path
                if (System.Linq.Enumerable.Any(Presets, p => p.StagingPath.Equals(path, System.StringComparison.OrdinalIgnoreCase)))
                    continue;
                list.Add(new NamedPreset
                {
                    Id = id,
                    DisplayName = path.Replace("/", " / ") + "  (from analysis)",
                    StagingPath = path,
                    ApproxCount = count,
                    Role = "Discovered depth-3 folder from Game Analyzer",
                    SamplePaths = new[] { "SB2/Content/" + path + "/" }
                });
            }
            return list;
        }

        public static NamedPreset FindPreset(string idOrName)
        {
            if (string.IsNullOrEmpty(idOrName)) return null;
            return System.Linq.Enumerable.FirstOrDefault(Presets, p =>
                p.Id.Equals(idOrName, System.StringComparison.OrdinalIgnoreCase)
                || p.DisplayName.Equals(idOrName, System.StringComparison.OrdinalIgnoreCase)
                || p.StagingPath.Equals(idOrName, System.StringComparison.OrdinalIgnoreCase));
        }


        public static readonly FolderInfo[] Folders =
        {
            new FolderInfo
            {
                Name = "1Data", ApproxCount = 7649, Role = "AI, NPCs (~2832), BuildingSystem, Enemy, keybinds, BB/BT — top balance/logic target",
                SubHints = new[] { "11_AI", "11_AI/Ammo", "NPCs", "BuildingSystem", "Enemy", "1_Keybings" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Data/11_AI/1_VAL_New_BB.uasset",
                    "SB2/Content/1Data/11_AI/1_VAL_New_BT.uasset",
                    "SB2/Content/1Data/11_AI/1_VAL_ShootCover.uasset",
                    "SB2/Content/1Data/11_AI/Ammo/FakeProjectile_Target_Muted.uasset",
                }
            },
            new FolderInfo
            {
                Name = "Packes_L_D", ApproxCount = 4816, Role = "Packaged LODs / detail packs",
                SamplePaths = new[] { "SB2/Content/Packes_L_D/" }
            },
            new FolderInfo
            {
                Name = "Packes", ApproxCount = 4203, Role = "Packaged asset groups",
                SamplePaths = new[] { "SB2/Content/Packes/" }
            },
            new FolderInfo
            {
                Name = "Armors", ApproxCount = 3907, Role = "Armor sets (Majorcharacters ~1637, 1_New_sets/F_1) — helmets, textures, physics",
                SubHints = new[] { "1_New_sets", "1_New_sets/F_1", "Majorcharacters" },
                SamplePaths = new[]
                {
                    "SB2/Content/Armors/1_New_sets/F_1/Armor8_Helmet.uasset",
                    "SB2/Content/Armors/1_New_sets/F_1/Armor8_New_CPP.uasset",
                    "SB2/Content/Armors/1_New_sets/F_1/Armor8_New_PhysicsAsset.uasset",
                    "SB2/Content/Armors/1_New_sets/F_1/Armor8_Normal.uasset",
                    "SB2/Content/Armors/1_New_sets/F_1/Armor8_WhitePurple_BaseColor.uasset",
                }
            },
            new FolderInfo
            {
                Name = "1Meshs", ApproxCount = 3658, Role = "Ships, pods, props, meshes",
                SubHints = new[] { "1Miscs", "1Miscs/Drop_Pod" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Meshs/1Miscs/Aseroid_VoxelConvert.uasset",
                    "SB2/Content/1Meshs/1Miscs/Drop_Pod/1_albedo.uasset",
                    "SB2/Content/1Meshs/1Miscs/Drop_Pod/Pod.uasset",
                    "SB2/Content/1Meshs/1Miscs/Hammer.uasset",
                }
            },
            new FolderInfo
            {
                Name = "Locomotion", ApproxCount = 2624, Role = "Character movement / animation systems",
                SamplePaths = new[] { "SB2/Content/Locomotion/" }
            },
            new FolderInfo
            {
                Name = "11Misc", ApproxCount = 2459, Role = "Baked skeletal, creatures BPs, misc systems",
                SubHints = new[] { "1BakedSkeletal", "1_1_1_HalilAICreature/BP_Creatures" },
                SamplePaths = new[]
                {
                    "SB2/Content/11Misc/1BakedSkeletal/3/M_SK_Shoes_05_MI.uasset",
                    "SB2/Content/11Misc/1_1_1_HalilAICreature/BP_Creatures/BP_Crab1.uasset",
                    "SB2/Content/11Misc/1_1_1_HalilAICreature/BP_Creatures/BP_Monster1.uasset",
                    "SB2/Content/11Misc/1_1_1_HalilAICreature/BP_Creatures/BP_Raptor1.uasset",
                }
            },
            new FolderInfo
            {
                Name = "1Galaxy", ApproxCount = 1671, Role = "Galaxy, dungeons, bosses, space progression",
                SubHints = new[] { "1_CPP_Dungeons", "Warp" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Galaxy/1_CPP_Dungeons/BossNames.uasset",
                    "SB2/Content/1Galaxy/1_CPP_Dungeons/BP_CaveInsidGate.uasset",
                }
            },
            new FolderInfo
            {
                Name = "1Widgets", ApproxCount = 1183, Role = "UMG UI templates and widgets",
                SubHints = new[] { "1Templetes" },
                SamplePaths = new[]
                {
                    "SB2/Content/1Widgets/1Templetes/H_Button.uasset",
                    "SB2/Content/1Widgets/1Templetes/H_Button_Icon.uasset",
                    "SB2/Content/1Widgets/1Templetes/H_Button_Small.uasset",
                    "SB2/Content/1Widgets/1Templetes/H_Button_Type2.uasset",
                }
            },
            new FolderInfo
            {
                Name = "_Textures", ApproxCount = 1511, Role = "Shared textures",
                SamplePaths = new[] { "SB2/Content/_Textures/" }
            },
            new FolderInfo
            {
                Name = "BallisticsVFX", ApproxCount = 1331, Role = "Weapon / ballistic VFX",
                SamplePaths = new[] { "SB2/Content/BallisticsVFX/" }
            },
            new FolderInfo
            {
                Name = "FMOD", ApproxCount = 1076, Role = "In-pak FMOD; also Master.bank on disk under Content/FMOD/Desktop",
                SamplePaths = new[] { "SB2/Content/FMOD/" }
            },
            new FolderInfo
            {
                Name = "111Cinematic", ApproxCount = 294, Role = "Cinematics, face anims, levels",
                SamplePaths = new[]
                {
                    "SB2/Content/111Cinematic/AI.uasset",
                    "SB2/Content/111Cinematic/Amara.uasset",
                    "SB2/Content/111Cinematic/AsteroidStationLevelT.umap",
                }
            },
            new FolderInfo
            {
                Name = "StarterBundle", ApproxCount = 2288, Role = "Starter content bundle",
                SamplePaths = new[] { "SB2/Content/StarterBundle/" }
            },
            new FolderInfo
            {
                Name = "Indus", ApproxCount = 1655, Role = "Industrial environment kit",
                SamplePaths = new[] { "SB2/Content/Indus/" }
            },
            new FolderInfo
            {
                Name = "AdvancedMaleCustomization", ApproxCount = 201, Role = "Character customization",
                SamplePaths = new[] { "SB2/Content/AdvancedMaleCustomization/" }
            },
            new FolderInfo
            {
                Name = "Scifi_Arsenal_Vol1", ApproxCount = 455, Role = "Sci-fi weapons arsenal",
                SamplePaths = new[] { "SB2/Content/Scifi_Arsenal_Vol1/" }
            },
            new FolderInfo
            {
                Name = "G2_CyborgCharacters", ApproxCount = 339, Role = "Cyborg characters",
                SamplePaths = new[] { "SB2/Content/G2_CyborgCharacters/" }
            },
        };

        /// <summary>Movies found on disk (Content/Movies) — replace by filename, no PAK.</summary>
        public static readonly string[] KnownMovies =
        {
            "Activeperks.mp4", "ArchiveCinematic_Final.mp4", "BattleTutorial.mp4",
            "Buildstructure.mp4", "BuildStructures.mp4", "Capitalshipspart1.mp4",
            "Capitalshipspart2.mp4", "Capital_Tutorial_mp4.mp4", "Castle Siege Final_mp4.mp4",
            "Castle Siege UI Tutorial_mp4.mp4", "Castle Siege_During Battle.mp4",
            "Castlesiegepart1.mp4", "Castlesiegepart2.mp4", "Castlesiegepart3.mp4",
            "CharacterPerks.mp4", "CombatBasics.mp4", "Crew.mp4", "Crewmanagement.mp4",
            "Defendsystems.mp4", "Diplomacy.mp4", "Diplomacynew.mp4", "DNA.mp4",
            "Drifting.mp4", "Enteringplanet.mp4", "Factionbasics.mp4",
            "Factionbattlespart1.mp4", "Factionbattlespart2.mp4", "Factionbattlespart3.mp4",
            "FullHDLongStartup.mp4", "GravityBoots.mp4", "HotasTut.mp4", "Hoverbike.mp4",
            "Insystemtravelpart1.mp4", "Insystemtravelpart2.mp4", "Jumpingsectorsystem.mp4",
            "Landinganddocking.mp4", "Leveling.mp4", "Loots.mp4", "Managingcomponents.mp4",
            "Menu_Background.mp4",
        };

        public static readonly string[] KnownFmodBanks =
        {
            "FMOD/Desktop/Master.bank",
            "FMOD/Desktop/Master.strings.bank",
        };

        public static readonly string[] KnownConfigs =
        {
            "SB2/Config/DefaultDeviceProfiles.ini",
            "SB2/Config/DefaultEngine.ini",
            "SB2/Config/DefaultEvents.ini",
            "SB2/Config/DefaultGame.ini",
            "SB2/Config/DefaultGameplayTags.ini",
            "SB2/Config/DefaultInput.ini",
            "SB2/Config/LoggingOverride.ini",
        };

        public static FolderInfo FindFolder(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Folders.FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetCatalogText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SpaceBourne 2 Content Catalog (embedded from analyzer 2026-09-27)");
            sb.AppendLine("Mount: ../../../ | Oodle | Monolithic SB2-WindowsNoEditor.pak");
            sb.AppendLine();
            foreach (var f in Folders.OrderByDescending(x => x.ApproxCount))
            {
                sb.AppendLine($"[{f.Name}] ~{f.ApproxCount} — {f.Role}");
                if (f.SubHints != null)
                    sb.AppendLine("  sub: " + string.Join(", ", f.SubHints));
                if (f.SamplePaths != null)
                    foreach (var p in f.SamplePaths.Take(5))
                        sb.AppendLine("  " + p);
            }
            sb.AppendLine();
            sb.AppendLine("Named presets:");
            foreach (var pr in Presets)
                sb.AppendLine($"  [{pr.Id}] {pr.StagingPath} (~{pr.ApproxCount}) — {pr.Role}");
            sb.AppendLine();
            sb.AppendLine("LOOSE Movies (disk, no pak):");
            foreach (var m in KnownMovies.Take(25))
                sb.AppendLine("  Movies/" + m);
            sb.AppendLine();
            sb.AppendLine("LOOSE FMOD:");
            foreach (var b in KnownFmodBanks)
                sb.AppendLine("  " + b);
            return sb.ToString();
        }
    }
}
