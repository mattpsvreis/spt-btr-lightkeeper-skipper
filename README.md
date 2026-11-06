# BTR/Lightkeeper Skipper

Standalone **addon** for [Skipper](https://sp-mod.com/mod/1343/skipper).

Skipper only injects Skip buttons on trader quest screens that have a handover button. BTR Driver and Lightkeeper quests are mainly managed from **Character → Tasks**, where that button is missing — so Skipper does nothing there.

This mod adds Skipper-style **SKIP** buttons (and an optional **FINISH** button) for those quests in the Tasks menu. Finish completes the quest and delivers rewards through the normal trader **message/mail** flow.

## Requirements

- SPT **4.1.x**
- [Skipper](https://sp-mod.com/mod/1343/skipper) **1.1.5+** (`com.terkoiz.skipper`)

## Install

1. Install Skipper first.
2. Extract this zip into your SPT root (the folder with `EscapeFromTarkov.exe`).

## Usage

1. Open **Character → Tasks**.
2. Expand a **BTR Driver** or **Lightkeeper** quest.
3. Hold **Left Control** (configurable in F12) to reveal **SKIP** on each objective — same workflow as Skipper.
4. When the quest is ready to turn in, a **FINISH** button appears on the description (can be disabled in F12). Confirm it to complete the quest, refresh the Tasks list, and receive rewards in messages.

## Known limitations

- Same as Skipper: skipped objective progress may not persist across raids/restarts until you actually finish the quest. Prefer skipping when you are ready to complete.
- Only BTR Driver and Lightkeeper quests are affected.

## Credits

- Quest skip technique from **Terkoiz** / **acidphantasm** Skipper
- Addon by Dark

## License

MIT License.
