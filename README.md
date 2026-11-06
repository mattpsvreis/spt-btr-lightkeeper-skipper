# BTR/Lightkeeper Skipper

Standalone **addon** for [Skipper](https://forge.sp-tarkov.com/mod/1343/skipper).  
Skipper only injects Skip buttons on trader quest screens that have a handover button. BTR Driver and Lightkeeper quests are mainly managed from **Character → Tasks**, where that button is missing — so Skipper does nothing there.

This addon adds Skipper-style **SKIP** buttons (and an optional **FINISH** button) for those quests in the Tasks menu.

## Requirements

- SPT **4.1.x**
- [Skipper](https://forge.sp-tarkov.com/mod/1343/skipper) **1.1.5+** (`com.terkoiz.skipper`)

## Install

1. Install Skipper first.
2. Extract this zip into your SPT root (the folder with `EscapeFromTarkov.exe`).
3. Confirm you have:
   - `BepInEx/plugins/Terkoiz.Skipper/` (or Skipper’s dll)
   - `BepInEx/plugins/Dark-BTRLightkeeperSkipper/Dark-BTRLightkeeperSkipper.dll`

## Usage

1. Open **Character → Tasks**.
2. Expand a **BTR Driver** or **Lightkeeper** quest.
3. Hold **Left Control** (configurable in F12) to reveal **SKIP** on each objective — same workflow as Skipper.
4. When the quest is ready to turn in, a **FINISH** button appears on the description (can be disabled in F12).

### F12 settings

| Setting | Default | Description |
| --- | --- | --- |
| Enabled | on | Master toggle |
| Always display buttons | off | Show Skip/Finish without holding the hotkey |
| Display hotkey | Left Control | Hold to reveal buttons |
| Show Finish button | on | Allow finishing BTR/LK quests from the Tasks menu |

## Known limitations

- Same as Skipper: skipped objective progress may not persist across raids/restarts until you actually finish the quest. Prefer skipping when you are ready to complete.
- Only BTR Driver (`656f0f98d80a697f855d34b1`) and Lightkeeper (`638f541a29ffd1183d187f57`) quests are affected.

## Build from source

1. Set `TarkovDir` to your SPT install (defaults to `D:\SPT\`), or pass `-p:TarkovDir=...` to MSBuild.
2. `dotnet build BTRLightkeeperSkipper.csproj -c Release`
3. Release zip is written to `release/Dark-BTRLightkeeperSkipper-1.0.0.zip`

## Plugin info (Forge)

- **GUID:** `com.dark.btrlightkeeperskipper`
- **Name:** `Dark-BTRLightkeeperSkipper`
- **Version:** `1.0.0`

## Credits

- Quest skip technique from **Terkoiz** / **acidphantasm** Skipper
- Addon by Dark

## License

NCSA Open Source License (same family as Skipper).
