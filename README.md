# Valheim QoL CM

Clickable admin panel for Valheim. Admins press backtick, then click. The current release is **0.3** (spec 003). This is not V1.

Versions use `0.N` for spec N. BepInEx reads the plugin version with `System.Version`, which turns a padded string such as `0.003` into `0.3`. The panel, this README, and the BepInEx plugin list all say `0.3`.

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

Join a world as an admin and press **`** (the backtick key, left of `1`). Press it again to close the panel. Closing only hides the panel. Modes, spawned items, granted admins, the skill-loss percent, and the action log stay as you set them. Esc also closes the panel. A player who is not an admin gets no panel. The header reads **Valheim QoL - CM** and **Version 0.3**. **By Alfamud** at the bottom right opens this repository.

- **Player Management.** Connected players, then bring-me, bring-player, and grant admin. Those three stay gray until another player is selected. Your own row stays highlighted and those actions stay gray.
- **Global Cheats.** God, Fly, Creative, and Free cam in a two-by-two grid. They toggle your own character and stay clickable.
- **Item Spawner.** Filter on top of the item list. Rows use the in-game names. Quantity, x10, and Max Stack sit under the list, with quality on the next row. Choosing an item sets the quantity to that item's stack size. Spawn stays gray until an item is selected. With nobody else highlighted, items appear at your feet.
- **Server skill loss.** Percent of each current skill level removed on the next death for every player. 0 removes none. 100 clears skills. Gear still drops. The server value wins.
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

Spec 001 is frozen and published as [0.001](https://github.com/LuisbatistaOT/Valheim-QoL-CM/releases/tag/0.001). Spec 003 is version 0.3. Gameplay rules from spec 001 are unchanged.

## Changelog

### 0.3

Spec 003. The panel is four wood modules. Spawn and the item list were corrected after the first layout pass. Gameplay rules from spec 001 are otherwise unchanged.

- Player actions sit under the connected-player list and stay gray until another player is selected. Your own row stays in the list, and those actions stay gray on that row.
- God, Fly, Creative, and Free cam sit in a two-by-two Global Cheats grid and stay clickable.
- The item list shows in-game names from `ObjectDB` and Valheim's localization. The filter matches those names, including Stone. The list scrolls.
- Quantity, `x10`, and Max Stack sit under the list. Quality is on the next row. Choosing an item sets the quantity to that item's stack size, capped at 100.
- With nobody else highlighted, Spawn drops items at your feet. Highlight another player and they drop at that player's feet.
- Skill loss is labeled as the server-wide death penalty, with the percent in a fixed-width box.
- The header reads `Valheim QoL - CM` and `Version 0.3`. By Alfamud opens this repository.
- The version string is `0.3`.

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
