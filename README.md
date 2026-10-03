# Valheim QoL CM

Clickable admin panel for Valheim. Admins press backtick, then click. The current release is **0.2** (spec 002). This is not V1.

Versions use `0.N` for spec N. BepInEx reads the plugin version with `System.Version`, which turns a padded string such as `0.002` into `0.2`. The panel, this README, and the BepInEx plugin list all say `0.2`.

## Players who only want the plugin

Download the two DLLs from [release/ValheimQoLCM](release/ValheimQoLCM):

- `ValheimQoLCM.dll`
- `ValheimQoLCM.Core.dll`

Copy both into:

`Valheim\BepInEx\plugins\ValheimQoLCM\`

On this machine that folder is `F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\ValheimQoLCM\`. A dedicated server uses the same two files under its own `BepInEx\plugins\ValheimQoLCM\` directory. Restart the game or the server after copying.

You need:

- Valheim with BepInEx 5.4.23.5
- Jötunn 2.30.1 already installed

The host of a local world is the first admin. On a server, an admin is a Steam ID already listed in `adminlist.txt`. The plugin does not promote anyone by itself and does not ban anyone.

Join a world as an admin and press **`** (the backtick key, left of `1`). Press it again to close the panel. Closing only hides the panel. Modes, spawned items, granted admins, the skill-loss percent, and the action log stay as you set them. Esc also closes the panel. A player who is not an admin gets no panel.

- **Players.** Left table of who is connected. Click a row. The selected row stays highlighted.
- **Items.** Filter under the player table. Matching items are rows under the filter. Click one to select it.
- **God, Fly, Creative, Free cam.** Each button toggles that mode on your character. Turning one off leaves the others alone.
- **Bring me to player / Bring player to me.** Moves between you and the highlighted player.
- **Spawn.** Quantity and quality are on the right. Quantity starts at 1 and cannot be above 100.
- **Skill loss.** Percent of each current skill level removed on the next death. 0 removes none. 100 clears skills. Gear still drops. The server value wins.
- **Grant admin.** Confirm to add the selected player's Steam ID. If the connection has no Steam ID, type one 17-digit ID and confirm again.
- **Console.** Action messages append along the bottom and stay there after you close the panel.

Full placement notes are in [docs/USAGE.md](docs/USAGE.md).

## Working on the project

The repository is the source. Domain rules live in `src/ValheimQoLCM.Core` and do not reference Unity. The BepInEx plugin is `src/ValheimQoLCM`. Specs are under `specs/`.

From the repository root:

```powershell
dotnet test ValheimQoLCM.sln
```

Build output stays in this repository. The files players copy are the two DLLs in `release/ValheimQoLCM`. Do not edit the Valheim install outside `BepInEx\plugins`.

Spec 001 is frozen and published as [0.001](https://github.com/LuisbatistaOT/Valheim-QoL-CM/releases/tag/0.001). Spec 002 is frozen at 0.2. Gameplay rules from spec 001 are unchanged.

## Changelog

### 0.2

Spec 002. Layout only.

- Connected players are a clickable table on the left. The selected row stays highlighted.
- The item filter sits under that table, and matching items are rows under the filter. Quantity and quality no longer cover the list.
- Action messages append in a console along the bottom. Closing the panel keeps those lines and does not undo gameplay changes.
- The version string is `0.2`. The padded string `0.002` was dropped because BepInEx showed it as `0.2`.

### 0.1

Published on GitHub as tag `0.001`. BepInEx listed that build as `0.1` because of the extra zero. Spec 001 MVP.

- Clickable admin panel for god, fly, creative, free cam, teleport, item spawn, skill-loss percent, and grant admin.
- Backtick opens and closes the panel. The first build listened for Ctrl+Tab, which did not appear in game.
- Closing the panel does not undo modes or other changes.
- A skill-loss percent of 0 removes no skill level. 100 clears skills. Gear still drops.
- Spawn quantity and quality are checked before anything is created.
- Grant admin uses the Steam ID from the live connection, with a typed fallback when that ID is missing.
