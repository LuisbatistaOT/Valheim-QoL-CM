# Valheim QoL CM 1.6

Spec 006. The panel opens with the + key. Skill loss of 0% stays 0% after relog. Kill enemies reports a miss instead of doing nothing quietly. Spec 001 gameplay, the spec 003 modules, and the spec 004 item list stay as they were. Version 1.5 remains the last published release.

## Requirements

- Valheim with BepInEx 5.4.23.5 (BepInExPack Valheim).
- Jötunn 2.30.1 already loaded. This plugin does not download it.
- The same `ValheimQoLCM.dll` on every machine that should share the skill-loss percent. A player without the plugin keeps vanilla skill loss.

## Where to place the plugin

Copy this folder into the game:

`Valheim\BepInEx\plugins\ValheimQoLCM\`

The folder must contain both `ValheimQoLCM.dll` and `ValheimQoLCM.Core.dll`. Leave the Valheim install otherwise unchanged. Restart the game, or the dedicated server, after copying.

The dedicated server needs the same two DLL files under that server's `BepInEx\plugins\ValheimQoLCM\` directory.

## First admin

On a local world, the host is the admin. On a server, an admin is a Steam ID already listed in that world's `adminlist.txt`. The plugin does not promote anyone by itself. Admins can add another connected player from the panel.

## How to open the panel

Join a world as an admin and press **+** (the =/+ key, not the numpad). Press **+** again to close the panel. Closing only hides it. God mode, fly, creative, free cam, ghost mode, tamed animals, killed enemies, spawned items, granted admins, the skill-loss percent, and the action console stay as they were. Esc also closes the panel without undoing those changes. The header reads `Valheim QoL - CM` and `Version 1.6`. BepInEx lists `1.6`. By Alfamud, at the bottom right, opens https://github.com/LuisbatistaOT/Valheim-QoL-CM.

A saved backtick binding, or the older Ctrl+Tab binding, is rewritten to +. Backtick does not open or close the panel unless someone binds it again. Rebind the keys in Valheim's Controls menu, or in BepInEx Configuration Manager (F1), under Valheim QoL CM. The binding is named Toggle QoL panel.

A player who is not an admin gets no panel.

## Controls

The panel is four wood modules in a grid, with the action console along the bottom.

- **Player Management.** Top left. Connected players are rows in a fixed list. Click a row to highlight it. Bring me to player, Bring player to me, and Grant admin sit under that list. They stay gray until another connected player is selected. Your own row, marked `(you)`, can be highlighted, and those three actions stay gray because they apply to someone else. If the connection has no Steam ID, one text field appears under Grant admin.
- **Global Cheats.** Top right. God, Fly, Creative, Free cam, and Ghost. Each mode button toggles that mode on your own character and stays clickable no matter which player or item is selected. Creative turns on vanilla no-build-cost. Free cam turns on the vanilla free camera. Ghost turns on vanilla ghost mode, so enemies ignore you. Turning one off leaves the others alone. Tame and Kill enemies sit under the grid. They do not show On or Off. Tame asks the world host to tame the creatures the vanilla tame command would tame. Kill enemies asks the host to kill untamed enemies within 1000 of you. Players and tamed creatures stay alive. The console reports how many. A dead character rejects Ghost, Tame, and Kill enemies, and the console says `Character is dead.`
- **Item Spawner.** Bottom left. Rows use the in-game item names. The filter matches those names. A row is listed only when that prefab is the one Spawn creates and its `m_icons` list has at least one icon. Valheim will not pick up a drop with an empty icon list, so those copies stay out of the search. If two listed items share an in-game name, each row also shows its prefab name. The list has a fixed height, a dark background, and a vertical scrollbar. Quantity is a row under the list: `-`, the number, `+`, `x10`, and Max Stack. Quality is the row under that. Spawn is under both. Those controls stay gray until an item row is clicked. Choosing an item sets the quantity to 1 and the quality to 1. The console repeats that row's name. `x10` multiplies the quantity by 10, still capped at 100. Max Stack sets the quantity to that item's vanilla stack size, capped at 100. Values below 1 or above 100 do not spawn. Quality cannot pass the item's vanilla maximum. With no other player highlighted, items spawn at your feet. Highlight another player and they spawn at that player's feet.
- **Server skill loss.** Bottom right. This percent is for every player on the next death, not for the highlighted row. The slider is beside a fixed box that shows `0%` through `100%`. Apply skill loss is centered under the slider. It starts at 5 until an admin applies a different percent. 0 removes none. 100 clears skills. Apply saves it for the server without a restart, and that saved value is still there after relog. Gear still drops as in vanilla.
- **Grant admin.** Confirm to add the selected player's Steam ID from the connection. If that connection has no Steam ID, type one 17-digit ID and confirm again. There is no ban control.
- **Console.** Action messages append along the bottom. Closing the panel keeps those lines.

## Server setting

The skill-loss percent is stored by the world host, beside the plugin, and shown in `BepInEx\config\valheim.qol.cm.cfg`. On a server, the host value wins. Joining clients use that value. A saved 0 stays 0. Jötunn can reset the admin config entry to its default of 5 when a client joins; the host file is what the next death and the slider use.

Two logs sit beside the plugin DLLs. `qol-cm.log` repeats each action-console line. `qol-cm-debug.log` records the skill-loss percent loaded at startup and, for each Kill enemies click, the admin position, radius 1000, how many characters were seen, and how many were killed.
