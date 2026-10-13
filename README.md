# specSB2modTool

**a creation by spec**

Unofficial SpaceBourne 2 (Unreal Engine 4.27) mod creation toolkit for Windows.

- **Store / community pitch:** see [STORE_DESCRIPTION.md](STORE_DESCRIPTION.md)
- **User Manual:** [SpaceBourne2ModTool_User_Manual.docx](SpaceBourne2ModTool_User_Manual.docx)

## Highlights

- Game File Analyzer (hang-proof PAK inventory + deep reports)
- Content Browser presets, Object Editors, Script Editor
- Speckage packaging + ordered Mod Loader (safe `z_*_P.pak` inject)
- Auto-detect tools (UnrealPak 4.27, FModel, UAssetGUI, authoring apps)
- Speckage packaging and ordered Mod Loader (safe `z_*_P.pak` inject)

## Build

Open `SpaceBourne2ModTool.sln` in Visual Studio and build the WinForms project.

## Packaging

- **PathGuard** keeps writes inside the mod folder or the game pak folder
- **Speckage** is a zip with `manifest.json` and a `content` folder
- **Mod Loader** imports, orders, enables, and injects only managed patch paks

Menu: **Package → Speckage Packager** and **Mod Loader**
