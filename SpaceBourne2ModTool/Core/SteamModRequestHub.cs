using System.Collections.Generic;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Maps Steam discussion mod requests to in-app workflows (grammar, graphics/planets/story exposure, rebalance).
    /// </summary>
    public static class SteamModRequestHub
    {
        public class Request
        {
            public string Id, Title, Summary, InAppPath, TemplateId;
            public string[] StagingFolders;
            public override string ToString() { return Title; }
        }

        public static readonly Request[] Requests =
        {
            new Request
            {
                Id = "grammar",
                Title = "Fix English grammar in dialogues/quests",
                Summary = "Clean dialogue/quest text with in-app grammar helpers, Script Editor, and localization staging. Export cooked strings via UAssetGUI when binary packages must change.",
                InAppPath = "Create → Dialogue & Grammar Studio · Script Editor · Localization",
                TemplateId = "steam_grammar",
                StagingFolders = new[] { "1Data/NPCs/Dialogs", "1Data/Quests", "Localization/Game" }
            },
            new Request
            {
                Id = "expose_world",
                Title = "Expose graphics, planets, mission/storyline",
                Summary = "Catalog and scaffold 1Galaxy, _Textures, 1Meshs, Armors, 1Data/Quests, 111Cinematic from Analyzer reports so modders can update art and story as hardware allows.",
                InAppPath = "Analyze → Content Browser · SpecVert · Mod Opportunity Studio",
                TemplateId = "steam_world_expose",
                StagingFolders = new[] { "1Galaxy", "_Textures", "1Meshs", "Armors", "1Data/Quests", "111Cinematic", "1Widgets" }
            },
            new Request
            {
                Id = "rebalance",
                Title = "Rebalancing of game systems",
                Summary = "Value override documents for damage/health/economy-style fields, balance DataTable notes under 1Data, load-order band 30, Speckage inject.",
                InAppPath = "Create → Balance & Rebalance Studio · Object Editors (Item/Ship)",
                TemplateId = "steam_rebalance",
                StagingFolders = new[] { "1Data", "1Galaxy" }
            }
        };
    }
}
