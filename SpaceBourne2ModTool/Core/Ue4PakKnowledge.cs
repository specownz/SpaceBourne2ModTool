// BUILD-20261013-1331
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>UE 4.27 pak layout notes for SpaceBourne 2. Build 20261013-1319.</summary>
    public static class Ue4PakKnowledge
    {
        public static string GetGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("UE 4.27 / SpaceBourne 2 — PAK structure and content catalog");
            sb.AppendLine("1) Paks load from <Game>/SB2/Content/Paks, Engine/Content/Paks, and Saved/Content/Paks.");
            sb.AppendLine("   Names ending in _P are patches and override the same virtual path.");
            sb.AppendLine("2) Mount point is ../../../ . Stage files under {Mod}_P using the same relative paths.");
            sb.AppendLine("3) /Game/... maps to ../../../SB2/Content/... and /Engine/... maps to ../../../Engine/Content/...");
            sb.AppendLine("4) AssetRegistry.bin lists cooked assets. Prefer overriding existing paths.");
            sb.AppendLine("5) Ship .uasset with matching .uexp and .ubulk. Edit with UAssetGUI set to 4.27.");
            sb.AppendLine("6) Compression is mostly Oodle (Kraken). Pack with UnrealPak 4.27 and -compress.");
            sb.AppendLine("7) Useful roots: 1Data/NPCs, 1Data/Quests, 1Data/Weapons, 1Data/SB2Ships, Armors/Majorcharacters, Armors/1_New_sets.");
            sb.AppendLine("8) Tools: FModel, UAssetGUI, UnrealPak 4.27.");
            return sb.ToString();
        }

        public static string ToGameVirtualPath(string pakRelativePath)
        {
            if (string.IsNullOrEmpty(pakRelativePath)) return pakRelativePath;
            string p = pakRelativePath.Replace('\\', '/').TrimStart('/');
            const string prefix = "SB2/Content/";
            if (p.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                string rest = p.Substring(prefix.Length);
                int dot = rest.LastIndexOf('.');
                if (dot > 0) rest = rest.Substring(0, dot);
                return "/Game/" + rest;
            }
            if (p.StartsWith("Engine/Content/", System.StringComparison.OrdinalIgnoreCase))
            {
                string rest = p.Substring("Engine/Content/".Length);
                int dot = rest.LastIndexOf('.');
                if (dot > 0) rest = rest.Substring(0, dot);
                return "/Engine/" + rest;
            }
            return p;
        }
    }
}
