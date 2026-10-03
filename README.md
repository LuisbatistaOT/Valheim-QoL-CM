# Valheim QoL CM

A clickable admin panel for Valheim. Press backtick, then click. No console commands.

**Version 1.5** is V1. It adds Ghost, Tame, and Kill enemies. Earlier specs stay at 0.4, 0.3, 0.2, and 0.1. BepInEx reads the plugin version with `System.Version`, so the published string stays `1.5` with no extra zero.

## Plugin information

You need Valheim, BepInEx 5.4.23.5, and Jötunn 2.30.1. This plugin does not download them.

Download the two DLLs from [release/ValheimQoLCM](release/ValheimQoLCM):

- `ValheimQoLCM.dll`
- `ValheimQoLCM.Core.dll`

Copy both into `Valheim\BepInEx\plugins\ValheimQoLCM\`.

A dedicated server uses the same two files in its own `BepInEx\plugins\ValheimQoLCM\` folder. Restart the game or the server after copying.

On a local world, the host is the first admin. On a server, an admin is a Steam ID already listed in `adminlist.txt`. The plugin does not promote anyone by itself, and it does not ban anyone.

Join as an admin and press **`** (the key left of `1`). Press it again to close the panel. Closing only hides the panel. Esc closes it too. Modes, tamed animals, killed enemies, spawned items, granted admins, the skill-loss percent, and the action log stay as you left them. A player who is not an admin gets no panel.

The header reads **Valheim QoL - CM** and **Version 1.5**. **By Alfamud** at the bottom right opens this repository.

- **Player Management.** Connected players, then bring-me, bring-player, and grant admin. Those three stay gray until another player is selected. Your own row stays highlighted and those actions stay gray.
- **Global Cheats.** God, Fly, Creative, Free cam, and Ghost. Each one toggles your own character. Ghost makes enemies ignore you. Tame and Kill enemies sit under that grid. Tame tames the animals the vanilla tame command would tame. Kill enemies removes hostile creatures within 1000 of you. Players and tamed animals stay. Both buttons tell you how many they affected.
- **Item Spawner.** Search items you can pick up, set quantity and quality, then spawn. Choosing an item sets the quantity to 1. Shared names also show the prefab. Spawn stays gray until an item is selected. With nobody else highlighted, items appear at your feet.
- **Server skill loss.** Percent of each skill removed on the next death for every player. 0 removes none. 100 clears skills. Gear still drops. The server value wins.
- **Grant admin.** Confirm to add the selected player's Steam ID. If the connection has no Steam ID, type one 17-digit ID and confirm again.
- **Console.** Action messages sit along the bottom and stay there after you close the panel.

Full placement notes are in [docs/USAGE.md](docs/USAGE.md).

## Working on the project

The repository is the source. Domain rules live in `src/ValheimQoLCM.Core` and do not reference Unity. The BepInEx plugin is `src/ValheimQoLCM`. Specs are under `specs/`.

From the repository root:

```powershell
dotnet test ValheimQoLCM.sln
```

Build output stays in this repository. The files players copy are the two DLLs in `release/ValheimQoLCM`. Do not edit the Valheim install outside `BepInEx\plugins`. Local builds read the game path from `src/ValheimQoLCM/Valheim.local.props`, which is not committed.

The current release is **1.5**, spec 005. Spec 001 is frozen and published as [0.001](https://github.com/LuisbatistaOT/Valheim-QoL-CM/releases/tag/0.001). Specs 002, 003, and 004 stay at 0.2, 0.3, and 0.4.

## Changelog

### 1.5

Spec 005. V1. Global Cheats gains Ghost, Tame, and Kill enemies.

- Ghost toggles vanilla ghost mode on your character. The other modes stay as they were.
- Tame asks the world host to tame the creatures the vanilla tame command would tame, and the console reports how many.
- Kill enemies asks the host to kill untamed enemies within 1000 of you. Players and tamed creatures stay. The console reports how many.
- A dead character cannot use the three new controls.
- The version string is `1.5`.

### 0.4

Spec 004. The item list only offers drops a player can pick up. Choosing a row sets the quantity to 1 and the console names that same row.

- Selecting an item sets quantity and quality to 1. Max Stack still fills one vanilla stack, capped at 100.
- Rows use the in-game name of the prefab Spawn creates. Shared names also show the prefab, and the console repeats that row name.
- Prefabs with an empty `m_icons` list are omitted. Valheim refuses to pick those drops up, which is why a bow search was full of copies named Bow.
- The version string is `0.4`.

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
