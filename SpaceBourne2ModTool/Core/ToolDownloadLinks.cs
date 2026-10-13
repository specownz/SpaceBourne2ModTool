using System.Collections.Generic;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Official / common download pages for external tools used with SB2 modding.
    /// </summary>
    public static class ToolDownloadLinks
    {
        public class ToolLink
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Notes { get; set; }
        }

        public static readonly ToolLink[] All =
        {
            new ToolLink
            {
                Name = "Unreal Engine 4.27 (UnrealPak)",
                Url = "https://www.unrealengine.com/en-US/download",
                Notes = "Install UE 4.27 → Engine\\Binaries\\Win64\\UnrealPak.exe"
            },
            new ToolLink
            {
                Name = "FModel",
                Url = "https://fmodel.app/",
                Notes = "Browse and export cooked UE assets from PAKs"
            },
            new ToolLink
            {
                Name = "UModel (UE Viewer)",
                Url = "https://www.gildor.org/en/projects/umodel",
                Notes = "View/export meshes and textures"
            },
            new ToolLink
            {
                Name = "UAssetGUI",
                Url = "https://github.com/atenfyr/UAssetGUI/releases",
                Notes = "Edit cooked .uasset (use UE 4.27 version)"
            },
            new ToolLink
            {
                Name = "Cheat Engine",
                Url = "https://www.cheatengine.org/",
                Notes = "Runtime trainers / pointer scans"
            },
            new ToolLink
            {
                Name = "CUE4Parse / FModel GitHub",
                Url = "https://github.com/4sval/FModel",
                Notes = "Source and related parsing libraries"
            },
            new ToolLink
            {
                Name = "SpaceBourne 2 on Steam",
                Url = "https://store.steampowered.com/app/1646850/SpaceBourne_2/",
                Notes = "Game store page (AppID 1646850)"
            },
            new ToolLink
            {
                Name = "Sublime Text",
                Url = "https://www.sublimetext.com/",
                Notes = "Paid code editor (preferred when Prefer Paid is on)"
            },
            new ToolLink
            {
                Name = "Visual Studio Code",
                Url = "https://code.visualstudio.com/",
                Notes = "Free code editor"
            },
            new ToolLink
            {
                Name = "GIMP",
                Url = "https://www.gimp.org/",
                Notes = "Free image editor (default fallback)"
            },
            new ToolLink
            {
                Name = "Blender",
                Url = "https://www.blender.org/",
                Notes = "Free 3D modeling (default fallback)"
            },
            new ToolLink
            {
                Name = "FMOD",
                Url = "https://www.fmod.com/",
                Notes = "Audio middleware — SB2 uses FMOD banks on disk"
            },
        };
    }
}
