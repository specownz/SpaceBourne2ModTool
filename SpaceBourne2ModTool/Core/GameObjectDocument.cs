using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>Editable override document for a game object (JSON in mod/Objects). UE4.27 cooked assets still need FModel/UAssetGUI for binary properties.</summary>
    public class GameObjectDocument
    {
        public string ObjectType { get; set; } = "Generic";
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
        public string DisplayName { get; set; } = "New Object";
        public string GamePath { get; set; } = "";
        public string StagingRelative { get; set; } = "";
        public string Notes { get; set; } = "";
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

        public string SuggestedFileName()
        {
            string safe = DisplayName ?? Id;
            foreach (char c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
            return ObjectType + "_" + safe + ".json";
        }

        public void Save(string path)
        {
            ModifiedUtc = DateTime.UtcNow;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path, JsonLite.ObjectToJson(ObjectType, Id, DisplayName, GamePath, StagingRelative, Notes, Properties, Tags, ModifiedUtc), Encoding.UTF8);
        }

        public static GameObjectDocument Load(string path)
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            return new GameObjectDocument
            {
                ObjectType = JsonLite.GetString(json, "ObjectType") ?? "Generic",
                Id = JsonLite.GetString(json, "Id") ?? Guid.NewGuid().ToString("N").Substring(0, 8),
                DisplayName = JsonLite.GetString(json, "DisplayName") ?? Path.GetFileNameWithoutExtension(path),
                GamePath = JsonLite.GetString(json, "GamePath") ?? "",
                StagingRelative = JsonLite.GetString(json, "StagingRelative") ?? "",
                Notes = JsonLite.GetString(json, "Notes") ?? "",
                Properties = JsonLite.GetObjectMap(json, "Properties")
            };
        }
    }

    public static class ObjectTypeRegistry
    {
        public class TypeDef
        {
            public string Id, DisplayName, DefaultStaging, Description;
            public string[] DefaultPropertyKeys;
        }

        public static readonly TypeDef[] Types =
        {
            T("NPC", "NPC", "1Data/NPCs", "NpcId,DisplayName,Faction,DialogRoot,Notes", "NPC data under 1Data/NPCs — pair with Dialog assets and Majorcharacters meshes."),
            T("Dialog", "Dialog / Conversation", "1Data/NPCs/Dialogs", "Speaker,LineId,Text,NextLineId,Conditions,GamePath", "Cooked packages under Dialogs/<NPC>/ — Script Editor + UAssetGUI."),
            T("Quest", "Quest", "1Data/Quests", "QuestId,Title,Stage,Objectives,Rewards,RelatedNpc", "Quest data (e.g. Obsesif) — document overrides here."),
            T("Character", "Major Character", "Armors/Majorcharacters", "CharacterFolder,MeshNotes,MaterialSwap,ArmorSet", "Armors/Majorcharacters/<Name> reskins."),
            T("Armor", "Armor Set", "Armors/1_New_sets", "SetId,Slot,MeshPath,MaterialPath", "Armor sets M_*/F_*."),
            T("Helmet", "Helmet", "Armors", "HelmetId,MeshPath,MaterialPath", "Head gear. Kept separate from body armor so a reskin does not overwrite the suit."),
            T("Weapon", "Weapon", "1Data/Weapons", "WeaponId,Damage,FireRate,Ammo,MeshPath", "Weapon data. Cooked meshes still ship as .uasset plus .uexp."),
            T("Ship", "Ship / Vehicle", "1Data/SB2Ships", "ShipId,DisplayName,Speed,Hull,Shield,Notes", "Ship data. Visuals may also live under Packes or 1Galaxy."),
            T("Item", "Item / Inventory", "1Data", "ItemId,DisplayName,Category,Value,Weight,Description", "Item definitions plus localization."),
            T("Loot", "Loot table", "1Data", "TableId,Entries,Weights,Notes", "Drop lists. Weights are numbers, not a second copy of the item."),
            T("Effect", "Effect / sound", "BallisticsVFX", "EffectId,Kind,AssetPath,Notes", "Visual effects and sounds. Kind is VFX or Audio."),
            T("Cinematic", "Cinematic / Sequence", "111Cinematic", "SequenceName,Level,Notes", "LevelSequences / related cooked assets."),
            T("Config", "Config / INI", "../Config", "IniFile,Section,Key,Value", "SB2/Config overrides."),
            T("Generic", "Generic Asset", "", "AssetPath,Notes", "Any SB2/Content path."),
        };

        static TypeDef T(string id, string name, string staging, string keys, string desc) =>
            new TypeDef { Id = id, DisplayName = name, DefaultStaging = staging, DefaultPropertyKeys = keys.Split(','), Description = desc };

        public static TypeDef Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return Types[Types.Length - 1];
            foreach (var t in Types)
                if (string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) return t;
            return Types[Types.Length - 1];
        }
    }
}
