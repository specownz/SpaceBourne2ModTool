# specSB2modTool

**a creation by spec** · assisted by Grok (xAI)

Unofficial **SpaceBourne 2** mod creation toolkit for Windows (Unreal Engine **4.27**). Dark modern UI. Built around real file-mod workflows: unencrypted package archives, small `z_*_P` overrides, mount `../../../`, Oodle compression via Engine 4.27 UnrealPak, and cooked asset pairs (`.uasset` + `.uexp` + optional `.ubulk`).

> Not affiliated with the game developer. No Steam Workshop required.

---

## Pitch

Scan your install, scaffold mods from complete templates, clean dialogue text, plan art and balance changes, package Speckage shares, inject ordered patches safely, and keep mods aligned after game updates. Optional live-session notes and UE4SS Lua folder builders for advanced users.

---

## Feature summary

- **Game File Scan** — hang-resistant, dual progress, deep reports (targets, fingerprint, cookbook, risk, coverage)
- **Merge-safe scaffolding** — multi-scaffold without wiping work; overwrite confirms with preview
- **Templates & Community request studio** — grammar, world exposure, rebalance scaffolds
- **GitHub** — publish open mods, push/pull, clone or ZIP-import community mods
- **Speckage + Mod Loader** — offline packages, ordered inject, PathGuard, backups
- **Convert mod to current game files** — fingerprint; does nothing if unchanged
- **Live session observer** — play notes + optional UE4SS log tail (no process injection)
- **UE4SS Lua project export** — correct `scripts/main.lua` layout and hook templates
- **Content Browser, SpecVert, Object/Script editors, viewers**
- **Help section + Beginner guide** in plain English
- **Auto-detect** Steam install and helper programs

---

## Typical workflow

1. Set game path → **Game File Scan**  
2. **Mod Templates** or **Community request studio**  
3. Stage files (or loose Movies/FMOD)  
4. **Speckage** → **Mod Loader** inject → play  
5. After updates: scan again → **Convert mod to current game files** → re-package  

---

## Requirements

Windows · .NET Framework 4.8 · optional UnrealPak 4.27, FModel, UAssetGUI 4.27, UModel · legitimate SpaceBourne 2 (Steam AppID 1646850)

## Credits

spec · Grok (xAI) · FModel/CUE4Parse · UAssetAPI/UAssetGUI (atenfyr) · UModel · Epic UnrealPak · RE-UE4SS documentation patterns

## Tags

`SpaceBourne2` `UE4` `4.27` `modding` `PAK` `Speckage` `modloader` `FModel` `UAssetGUI` `UE4SS`
