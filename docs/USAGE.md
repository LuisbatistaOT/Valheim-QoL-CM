# Valheim QoL CM 0.2

Layout update for spec 002. Spec 001 gameplay is unchanged. This is not V1.

## Requirements

- Valheim with BepInEx 5.4.23.5 (BepInExPack Valheim).
- Jötunn 2.30.1 already loaded. This plugin does not download it.
- The same `ValheimQoLCM.dll` on every machine that should share the skill-loss percent. A player without the plugin keeps vanilla skill loss.

## Where to place the plugin

Copy this folder into the game:

`F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\ValheimQoLCM\`

The folder must contain both `ValheimQoLCM.dll` and `ValheimQoLCM.Core.dll`. Leave the Valheim install otherwise unchanged. Restart the game, or the dedicated server, after copying.

The dedicated server needs the same two DLL files under that server's `BepInEx\plugins\ValheimQoLCM\` directory.

## First admin

On a local world, the host is the admin. On a server, an admin is a Steam ID already listed in that world's `adminlist.txt`. The plugin does not promote anyone by itself. Admins can add another connected player from the panel.

## How to open the panel

Join a world as an admin and press **`** (the backtick key). Press **`** again to close the panel. Closing only hides it. God mode, fly, creative, free cam, spawned items, granted admins, the skill-loss percent, and the action console stay as they were. Esc also closes the panel without undoing those changes. The footer reads `0.2`, and BepInEx lists the same number.

Rebind the keys in Valheim's Controls menu, or in BepInEx Configuration Manager (F1), under Valheim QoL CM. The binding is named Toggle QoL panel.

A player who is not an admin gets no panel.

## Controls

- **Players.** The left table lists who is connected. Click a row. The selected row stays highlighted. Teleport, spawn, and grant admin use that player.
- **Items.** The item filter sits under the player table. Matching items are rows under the filter. Click a row to select it. Quantity and quality stay on the right and do not cover that table.
- **God, Fly, Creative, Free cam.** Each button toggles that mode on your own character. Creative turns on vanilla no-build-cost. Free cam turns on the vanilla free camera. Turning one off leaves the others alone. The buttons do nothing while you are dead.
- **Bring me to player / Bring player to me.** Moves between you and the selected connected player. There is no name box and no coordinate box.
- **Spawn.** Set quantity and quality on the right, then click Spawn. Quantity starts at 1. Values below 1 or above 100 do not spawn. Quality cannot pass the item's vanilla maximum. The items appear at the selected player's feet.
- **Skill loss.** The slider is the percent of each current skill level removed on the next death. It starts at 5. 0 removes none. 100 clears skills. Apply saves it for the server without a restart. Gear still drops as in vanilla.
- **Grant admin.** Confirm to add the selected player's Steam ID from the connection. If that connection has no Steam ID, a single text field appears. Confirm again after typing a 17-digit Steam ID. There is no ban control.
- **Console.** Action messages append along the bottom. Closing the panel keeps those lines.

## Server setting

The skill-loss percent is stored in `BepInEx\config\valheim.qol.cm.cfg`. On a server, the server file wins. Joining clients use that value.
