# specSB2modTool — Complete Features & Capabilities List

**a creation by spec** · assisted by Grok (xAI)  
Unofficial SpaceBourne 2 mod toolkit · Unreal Engine 4.27 · Windows · .NET Framework 4.8

---

## A. Interface & usability

| Feature | Description |
|--------|-------------|
| Dark modern theme | Recursive theming of forms, tabs, lists, text boxes, buttons, grids |
| Form auto-scale | Controls scale when windows are resized |
| Quickbar | Left navigation for common tools |
| MenuStrip | File, Analyze, Create, Package, View, Tools, Help |
| First-run auto-detect splash | Searches for game and external tools with progress and checkmarks |
| Beginner guide | Plain-English topics for new modders |
| Help section | In-app manual topics aligned with User Manual |
| About | Version, credits, third-party acknowledgements, tagline |
| Status strip | Live status messages |
| Dashboard snapshot | Game path, tool status, analysis summary, targets counts |

## B. Game discovery & analysis

| Feature | Description |
|--------|-------------|
| Steam auto-detect | AppID 1646850, registry, library folders, manifests |
| Game path validation | Checks install structure |
| Game File Analyzer | Long-running, hang-resistant UnrealPak -List based scan |
| Dual progress bars | Overall + current file |
| Per-file status | Frequent progress updates |
| Depth-2/3 folder maps | Characters, NPCs, quests, armor sets, cinematics, etc. |
| Category classification | Texture, Material, Mesh, Audio, Animation, Blueprint, UI, … |
| Loose Movies / FMOD inventory | On-disk replace candidates |
| ModTargets report | MAJORCHAR, QUEST, ARMORSET, CINEMATIC, LOOSE, packing notes |
| Naming patterns | Prefix mining for BP/UI/mat/texture style names |
| Gameplay buckets | Gameplay-related path signals |
| Injection risk | Notes on monolithic base, Oodle, mount |
| Deployed mods inventory | Existing z_* / patch presence |
| SpecVert coverage | Module signal counts |
| Tool cross-reference | External tool presence/capabilities |
| Asset health | Trio / format health hints |
| Game fingerprint | SHA-based snapshot of pak sizes/dates for update detection |
| Easy Mod Cookbook | Plain summary of easiest targets from each scan |
| Author feedback reports | Large path dumps for iterative tool improvement |
| CrossToolHub | Shares LATEST reports across tools |

## B2. Merge-safe scaffolding
| Feature | Description |
|--------|-------------|
| Folder create | Always additive |
| File merge | Same content skipped; different content → Yes/No warning with excerpts + .bak backup |
| List merge | TARGET_PATHS-style unique line append |
| Used by | Content Browser, Mod Templates (apply), related scaffolds |

## C. Project creation & templates

| Feature | Description |
|--------|-------------|
| New Mod wizard | Creates staged SB2/Content hierarchy |
| 20+ built-in templates | Characters, armor, quests, dialogs, movies, FMOD, UI, ships, VFX, meshes, locomotion, localization, config, SpecVert, multi-layer, Steam requests |
| User templates | Save/load `.sb2template.txt` from open mod layout |
| Apply template to open mod | Layers folders, object/script seeds, load-order hints |
| SpecVerted flag | Marks mods using modular SpecVert workspace |
| TARGET_PATHS / WORKFLOW / LOAD_ORDER files | Auto-generated project notes |
| Recent mods list | Open / refresh from Dashboard |

## D. Community request studio (Steam)

| Feature | Description |
|--------|-------------|
| Grammar cleanup | Heuristic English cleanup for dialogue/quest text |
| Save cleaned text to open mod | Script documents under Scripts/ |
| Art / planets / story exposure | Lists scan-based folders and scaffolds |
| Rebalance documents | Health/Damage/Armor/Speed/Cost object docs |
| Scaffold complete mod | One-click project from selected request |

## E. Editing & content tools

| Feature | Description |
|--------|-------------|
| Content Browser | Named presets from catalog + analysis import |
| Object Editor Hub | Modular object types (NPC, Character, Quest, Item, Ship, …) |
| Object Editor | Save/load JSON object documents |
| Script Editor | Dialog/quest notes organized by category |
| Localization helpers | Locres staging guidance and templates |
| Code Editor | In-app editor with modding-oriented snippets |
| Conversion tools | Guidance for texture/audio/movie pipelines |
| Loose File Mods | Movies/FMOD replace with backups |
| Trainer templates | Documentation for CE/WeMod style options (not cheats bundled) |

## E2. GitHub integration
| Feature | Description |
|--------|-------------|
| PAT auth | Save token, test `/user`, local settings only |
| Publish new repo | API create + git init/add/commit/push |
| Commit + push / Pull | Existing git remotes |
| Clone import | Git clone into GitHubImports |
| ZIP import | API zipball without Git |
| Search / list repos | Search API + user repos |
| Releases list | Per selected repository |
| .gitignore / README | SB2-oriented defaults |
| Open on web | Browser to repository |

## F. Packaging, injection, updates

| Feature | Description |
|--------|-------------|
| Speckage packages | Offline shareable packages with manifest |
| Speckage packager form | UI for packaging |
| Mod Loader | Import, order, enable, inject managed patches, backup, unload, launch game |
| PathGuard | Safe path resolution; vanilla cores blocked by default |
| Allow vanilla core edits | Settings gate with warning |
| Injection plan | Suggested load-order bands from analysis |
| SpecPipeline checklist | End-to-end UE pipeline guide |
| Convert mod to current game files | Fingerprint compare; refresh targets if game updated; no-op if same |
| PAK helpers | UnrealPak response-file oriented packaging guidance |
| Pak browser | Explore package listing helpers |

## G. SpecVert & modular workspace

| Feature | Description |
|--------|-------------|
| SpecVert service | Modular tree from analysis (Armor, Characters, Dialogs, Quests, Ships, Widgets, …) |
| SpecVert form | UI to build/view modular workspace |
| Values overrides area | Document intended numeric changes |

## H. Media viewers

| Feature | Description |
|--------|-------------|
| 3D Model Viewer | OBJ/STL in-app (wireframe / unlit / shaded); UModel for other formats |
| Image Viewer | Fit, zoom, greyscale, alpha checkerboard; compact embeds where applicable |

## I. Live & scripting (optional advanced)

| Feature | Description |
|--------|-------------|
| Live session observer | Notes while playing; optional UE4SS.log tail; merged export with ModTargets |
| Scripting studio | UE4SS Lua templates, Editor Python sample, C# tool notes |
| UE4SS project export | Correct `YourMod/scripts/main.lua` + mods.txt line + plain README |
| Hook templates | Hello, delayed hook, keybind+ExecuteInGameThread, pre/post RegisterHook, InitGameStatePost |

## J. External tools integration

| Feature | Description |
|--------|-------------|
| Auto-detect | UnrealPak, FModel, UModel, UAssetGUI, Cheat Engine, authoring apps |
| Prefer paid / free fallbacks | Code editor, image editor, 3D modeling preferences |
| Launch helpers | Open detected tools from Settings/Tools menu |
| Download links | Dashboard links for obtaining helpers |
| UAssetAPI bridge status | Documents optional DLL / UAssetGUI workflow (.NET 4.8 limits) |

## K. Safety & logging

| Feature | Description |
|--------|-------------|
| Backups on inject / loose replace | Managed restore paths |
| Open log file | Diagnostics |
| Settings persistence | Paths and preferences |

## L. Documentation artifacts

| Artifact | Purpose |
|----------|---------|
| User Manual (.docx) | Full how-to and reference |
| FEATURES_AND_CAPABILITIES.md | This list |
| STORE_DESCRIPTION.md | Public pitch / feature summary |
| In-app Help + Beginner guide | Same knowledge inside the app |
| Analysis LATEST reports | Machine-readable feedback loop for tool improvement |

---

*Keep this file updated when features are added. Mirror major changes in HelpContent.cs and the User Manual.*
