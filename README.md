# Valheim QoL CM 0.001

MVP for spec 001. This is not V1.

## Requirements

- Valheim with BepInEx 5.4.23.5 (BepInExPack Valheim).
- Jötunn 2.30.1 already loaded. This plugin does not download it.
- The same `ValheimQoLCM.dll` on every machine that should share the skill-loss percent. A player without the plugin keeps vanilla skill loss.

## Where to place the plugin

Copy this folder into the game:
For example:
`F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\ValheimQoLCM\`

The folder must contain both `ValheimQoLCM.dll` and `ValheimQoLCM.Core.dll`. Leave the Valheim install otherwise unchanged. Restart the game, or the dedicated server, after copying.

The dedicated server needs the same two DLL files under that server's `BepInEx\plugins\ValheimQoLCM\` directory.

## First admin

On a local world, the host is the admin. On a server, an admin is a Steam ID already listed in that world's `adminlist.txt`. The plugin does not promote anyone by itself. Admins can add another connected player from the panel.

## How to open the panel

Join a world as an admin and press **`** (the backtick key). Press **`** again to close the panel. Closing only hides it. God mode, fly, creative, free cam, spawned items, granted admins, and the skill-loss percent stay as you set them. Esc also closes the panel without undoing those changes. The footer reads `0.001`. BepInEx's own plugin list can show `0.1` for this same build, because it parses the version as a number and drops the extra zero.

Rebind the keys in Valheim's Controls menu, or in BepInEx Configuration Manager (F1), under Valheim QoL CM. The binding is named Toggle QoL panel.

A player who is not an admin gets no panel.

## Controls

- **Players.** The list shows who is connected. Select a name. Teleport, spawn, and grant admin use that selection.
- **God, Fly, Creative, Free cam.** Each button toggles that mode on your own character. Creative turns on vanilla no-build-cost. Free cam turns on the vanilla free camera. Turning one off leaves the others alone. The buttons do nothing while you are dead.
- **Bring me to player / Bring player to me.** Moves between you and the selected connected player. There is no name box and no coordinate box.
- **Spawn.** Filter the item list, set quantity and quality, then click Spawn. Quantity starts at 1. Values below 1 or above 100 do not spawn. Quality cannot pass the item's vanilla maximum. The items appear at the selected player's feet.
- **Skill loss.** The slider is the percent of each current skill level removed on the next death. It starts at 5. 0 removes none. 100 clears skills. Apply saves it for the server without a restart. Gear still drops as in vanilla.
- **Grant admin.** Confirm to add the selected player's Steam ID from the connection. If that connection has no Steam ID, a single text field appears. Confirm again after typing a 17-digit Steam ID. There is no ban control.

## Server setting

The skill-loss percent is stored in `BepInEx\config\valheim.qol.cm.cfg`. On a server, the server file wins. Joining clients use that value.
