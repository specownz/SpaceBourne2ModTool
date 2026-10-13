# specSB2modTool user manual

Unofficial tool for SpaceBourne 2 (Unreal Engine 4.27). It does not replace the game and it is not made by the game's developer. A creation by spec, assisted by Grok (xAI).

The program opens maximized and fills the screen. The left side is the Quickbar. The rest is a tab workspace. Home holds the dashboard and the built-in pages. Every other tool opens as its own tab. You can switch tabs without losing your place. Window → Close this tool tab closes the tool you are looking at. It does not close Home.

## 0. What is on the screen

| Piece | What it does |
|---|---|
| Quickbar | Same-size buttons. Scroll if the list is taller than the window. White text on dark buttons. |
| Home tab | Dashboard, New Mod, PAK tools, localization, trainer notes, and Settings. The Quickbar switches these. |
| Tool tabs | Game Analyzer, Working Folder, viewers, editors, GitHub, Mod Loader, and the rest. One copy of each stays open. Opening it again shows the tab you already have. |
| Menu | File, Analyze, Create, Package, View, Tools, Window, Help. About is under Help. |
| Status line | The last thing the program did. |

Dialog boxes are only for picking a folder or a file, for the first-run search, and for yes/no warnings.

## 1. First run

1. Start the program. If it has never searched, it looks for the Steam install and for external tools. The splash list shows each search and a check when one is found.
2. On the home screen, confirm the game folder. A valid install contains the shipping game program and `SB2\Content\Paks`.
3. Run **Game Analyzer** once and let both progress bars finish. This first scan is the baseline.
4. Create a mod from **New Mod** or **Mod Templates**.
5. Edit with the object editors, the content browser, or the working folder.
6. Package with Speckage, then use the Mod Loader to inject a copy and start the game. The loader does not rewrite the game's base pak.

## 2. Home screen

The Quickbar opens the tools you use most. Home shows the dashboard first: the game folder, whether it looks valid, recent mods, and links to download FModel, UAssetGUI, UModel, and the other external programs.

Settings stores those paths. Paid tools are preferred when that option is on. Free fallbacks are Notepad, GIMP, and Blender.

**Allow editing of vanilla core files** is off. Leave it off. Turning it on can break the game.

## 2b. Working Project Folder

This is the copy of the game you mod from. It is not the Steam folder.

- **Scan (skip known files)** uses a ledger in `.specsb2`. Unchanged files are not read again.
- **Hard rip** reads every file. It asks first.
- The storage line shows gigabytes used and free space on that drive. A full rip can be larger than the game pak.
- The list is on the left. The preview on the right shows pictures, and a simple outline for OBJ and STL. Other types say to use **Open this file**.
- **Open this file** opens the image viewer, the 3D viewer, the EvE voice form, UAssetGUI, or your code editor, as a tab.
- **Copy selected** and **Copy shown list** put files into the open mod. Files already there are skipped.
- **Import** skips files that were already copied. **Re-import changed files** replaces a file only when the size changed.

## 2c. EvE voice

EvE is the ship's voice. The game plays it from FMOD sound banks. This program can save a WAV with the installed Windows voices, the same idea as specVoice. specVoice is not built in. If you have specVoice.exe you can open it from the EvE tab. The game will not speak the WAV until you rebuild the matching bank and copy that bank into the mod.

## 3. Game File Scan

The scan reads the install, lists pak files, and, when UnrealPak is available, reads the pak index. It does not modify the game.

Reports land in the folder you choose. Keep using the same folder. The important files are:

| File | What to do with it |
|---|---|
| SB2_GameAnalysis_LATEST.md | Overview: compression, extensions, content folders |
| SB2_GameAnalysis_LATEST.json | Full machine-readable scan |
| SB2_ModTargets_LATEST.txt | Easiest override targets |
| SB2_ContentMap_LATEST.txt | Folder counts, including characters and armor sets |
| SB2_UpdateDiff_LATEST.txt | What changed since the previous scan |
| SB2_AuthorFeedback_LATEST.txt | Packet to send back when improving the tool |
| SB2_GameFingerprint_LATEST.txt | Stamp used by Convert mod to current game files |

### After today's update, or any later update

1. Point the analyzer at the same reports folder as the baseline scan.
2. Run it again after the game has finished updating.
3. Open `SB2_UpdateDiff_LATEST.txt`.
4. Send that file and `SB2_GameAnalysis_LATEST.md`.

The diff lists:

- Pak files added, removed, resized, or rewritten with the same size
- Content folders whose file counts moved (new characters, weapons, ships, and so on show up here)
- File types whose counts moved (`.uasset`, `.locres`, `.ini`)
- The game program version, when Windows reports one

The first scan in an empty reports folder is only a baseline. If that folder already contains `SB2_GameAnalysis_LATEST.json` from before the update, the new scan compares against it and writes the diff immediately. Send `SB2_UpdateDiff_LATEST.txt` and `SB2_GameAnalysis_LATEST.md`.

Cooked items are three files when the game cooked them that way: `.uasset`, `.uexp`, and sometimes `.ubulk`. Ship them together. The game's main pak is not encrypted. Most entries use Oodle compression, so a patch pak must be built with Unreal Engine 4.27 UnrealPak and `-compress`.

## 4. Working Project Folder

This folder is for files already ripped out of a pak (FModel, UModel, or a similar export). It is not the Steam folder. The scan refuses to run if you point it at the game.

Import copies an export in and leaves files that are already there. Each file is labeled weapon, ship, armor, helmet, character, dialogue, mesh, texture, or another part. The tool column names what wrote it. Copy into an open mod places the file under that mod's `SB2\Content` tree and asks before replacing anything.

## 5. Creating a mod

**New Mod** starts an empty project. **Mod Templates** applies a built-in or saved template. **Steam Requests** holds the community jobs: dialogue grammar, exposed graphics and missions, and rebalancing notes.

Scaffolding merges into the open mod. New folders are created. List files gain new lines only. If a text file would be overwritten, a warning shows the old text and the new text and asks first.

**specVert** is optional. It marks a mod whose notes follow a modular layout. The game itself is not recompiled.

## 6. Object editors

One editor covers every object type. Open **Object Editors**, pick the type, fill the fields, save, and import into the open mod.

| Type | Where it lives in the game |
|---|---|
| NPC | 1Data/NPCs |
| Dialog | 1Data/NPCs/Dialogs |
| Quest | 1Data/Quests |
| Character | Armors/Majorcharacters |
| Armor | Armors/1_New_sets |
| Helmet | Armors (head slot only) |
| Weapon | 1Data/Weapons |
| Ship | 1Data/SB2Ships (visuals may also be under Packes or 1Galaxy) |
| Item | 1Data |
| Loot | 1Data drop lists |
| Effect | BallisticsVFX or FMOD |
| Config | SB2/Config |

These are notes and staging paths. A cooked mesh or material still has to be edited in UAssetGUI or recooked. The editor does not pretend to rewrite binary packages by itself.

**Script Editor** is for dialogue and quest text that has been extracted to text. **Content Browser** opens named presets for NPCs and major characters and can scaffold them into the open mod.

## 7. Code, images, and models

**Code Editor** is for ini, json, lua, and notes. **Image Viewer** opens png and similar files. **3D Model Viewer** opens obj and fbx-style mesh exports. The game will not load those mesh files directly. They are for looking and for sending to a cook step.

## 8. Packaging and loading

**PAK Tools** builds a response list for UnrealPak. Patch files should be named `z_YourMod_P.pak`. The mount point is `../../../`.

**Speckage** packs the mod folder into one archive you can hand to someone else. **Mod Loader** orders those packages, copies the patch paks into the game's pak folder under its own names, and starts the game. It does not delete the original pak. Remove a mod by removing the loader's patch file, not by editing the base archive.

**Convert mod to current game files** compares the fingerprint stored on the mod with the latest scan. If they match, it does nothing. If the game changed, it refreshes the mod's path notes. Run a scan first.

**GitHub** publishes the open mod or clones someone else's.

## 9. External tools

Use them when the in-app reader is not enough.

| Job | Tool |
|---|---|
| Browse and export a pak | FModel |
| Edit a cooked package | UAssetGUI |
| Preview a mesh | UModel |
| Build the patch pak | UnrealPak 4.27 |
| Scripts while the game runs | UE4SS |
| Images | GIMP, or the paid editor you set |
| Meshes | Blender, or the paid editor you set |

In-app header checks only identify a UE4 package, a locres table, a pak footer, or an ini. They follow the public Unreal Engine 4.27 layout. FModel and UAssetGUI are not built into this program.

## 10. What not to do

- Do not point the working folder at the Steam install.
- Do not replace `SB2-WindowsNoEditor.pak`.
- Do not ship a `.uasset` without its `.uexp`.
- Do not turn on vanilla core edits unless you have a backup and you mean it.

## 11. Every form, and how they connect

They share one open mod, one game folder, and one working folder. A change in Settings is what the Analyzer, the viewers, and the packager use next.

| Form | What you do there | What it talks to |
|---|---|---|
| Dashboard (Home) | Confirm the game folder and open a recent mod | Settings, Mod Loader, Analyzer |
| New Mod | Name a mod and mark it SpecVerted if you want the Modules tree | Templates, Object Editors, Packager |
| Game Analyzer | Read the install and write reports. Two progress bars. No line cap on the pak list. | Update diff, Content Browser, Convert mod |
| Working Folder | Ledger of ripped files, preview, copy into the mod | Viewers, EvE, UAssetGUI, open mod |
| Content Browser | Named scaffolds such as NPCs, armor, 1CCG, DLC quests | Open mod, merge warning |
| Object Editors | Edit one weapon, ship, armor, helmet, item, or effect and import it into the mod | Open mod |
| Script Editor | Dialogue and quest text when a text export exists | Analyzer paths, open mod |
| Mod Templates | Built-in and your own templates | New mod staging |
| Steam request studio | Grammar, graphics exposure, and rebalance starters | Templates |
| Localization | Edit locres text exports | PAK tools |
| Loose file mods | Replace movies or FMOD banks with a backup | Game folder, only those loose files |
| Conversion tools | Turn a known export into a note for the cook step | Working folder |
| Trainer templates | Cheat Engine notes. Not live offsets | Your own scans |
| PAK tools | Build the UnrealPak response list | Open mod, UE 4.27 UnrealPak |
| Mod Packager | Walk the pack steps for the open project | PAK tools |
| Speckage | One archive to give to someone else | Mod Loader |
| Mod Loader | Order packages, copy patch paks, start the game | Game pak folder. Does not edit the base pak |
| GitHub | Publish or download a mod | Open mod |
| specVert | Optional modular folder layout. Does not recompile the game | New mod flag |
| Image viewer | Fit, zoom, and greyscale for a ripped picture | Working folder preview |
| 3D model viewer | Wireframe, grey, or shaded for OBJ and similar | Working folder |
| EvE voice | Save a WAV line. Ship a rebuilt FMOD bank | Working folder EvE\\Generated |
| Live session | Watch a log you choose while you play | Analyzer notes |
| Code editor | Edit text that belongs to the mod | Open mod files |
| Help | This manual, inside the program | — |
| About | Version, credits, specVoice is not included | — |

## 12. A full pass

1. First-run search finds the game and the tools.
2. Analyzer writes the reports. After a game update, run it again and read the update diff.
3. Import the FModel export into the working folder. Scan. Preview the file. Copy what you need into the open mod.
4. Use a template or an object editor for the numbers, and UAssetGUI for cooked packages.
5. Pack a `z_YourMod_P.pak` or a Speckage. Load it with the Mod Loader.
6. If the game updates, Convert mod to current game files after a new scan. Do not rip files that the ledger already has.

