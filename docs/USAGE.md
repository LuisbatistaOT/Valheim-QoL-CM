# Valheim QoL CM 1.8

Spec 008. The panel is five tabs. The World tab sets the five Valheim world modifiers from presets or stepped sliders and writes them into the world save on Apply. `Overwrite Skill loss to 0%` keeps every skill level on death. Spec 001 gameplay, the spec 004 item list, the spec 006 hotkey and logs, and the spec 007 saved fly stay as they were. This spec is frozen. Version 1.5 remains the last GitHub release.

## Requirements

- Valheim with BepInEx 5.4.23.5 (BepInExPack Valheim).
- Jötunn 2.30.1 already loaded. This plugin does not download it.
- The same `ValheimQoLCM.dll` on every machine that should share the skill-loss overwrite. World modifiers reach every player, with or without the plugin. A player without the plugin follows the world's death penalty for skill loss.

## Where to place the plugin

Copy this folder into the game:

`Valheim\BepInEx\plugins\ValheimQoLCM\`

The folder must contain both `ValheimQoLCM.dll` and `ValheimQoLCM.Core.dll`. Leave the Valheim install otherwise unchanged. Restart the game, or the dedicated server, after copying.

The dedicated server needs the same two DLL files under that server's `BepInEx\plugins\ValheimQoLCM\` directory.

## First admin

On a local world, the host is the admin. On a server, an admin is a Steam ID already listed in that world's `adminlist.txt`. The plugin does not promote anyone by itself. Admins can add another connected player from the panel.

## How to open the panel

Join a world as an admin and press numpad **+**, or hold **Shift** and press the **=/+** key. Press the same key again to close the panel. Closing only hides it. God mode, fly, creative, free cam, ghost mode, tamed animals, killed enemies, spawned items, granted admins, applied world modifiers, and the action console stay as they were. Esc also closes the panel without undoing those changes. The header reads `Valheim QoL - CM` and `Version 1.8`. BepInEx lists `1.8`. By Alfamud, at the bottom right, opens https://github.com/LuisbatistaOT/Valheim-QoL-CM.

A saved backtick binding, the older Ctrl+Tab binding, or the `Plus` code that Valheim never reports, is rewritten to numpad +. Rebind the keys in Valheim's Controls menu, or in BepInEx Configuration Manager (F1), under Valheim QoL CM. The binding is named Toggle QoL panel.

A player who is not an admin gets no panel.

## Tabs

The panel is one wood panel with a tab row across the top: **Cheats**, **World**, **Spawn**, **Players**, and **Log**. It opens on Cheats. The highlighted tab is the one showing.

### Cheats

God, Fly, Creative, Free cam, and Ghost in a grid. Each mode button toggles that mode on your own character. Creative turns on vanilla no-build-cost. Free cam turns on the vanilla free camera. Ghost turns on vanilla ghost mode, so enemies ignore you. Turning one off leaves the others alone. Fly is remembered by the plugin; a later session starts with your saved choice, and a missing value is off. Tame and Kill enemies sit under the grid. Tame asks the world host to tame the creatures the vanilla tame command would tame. Kill enemies asks the host to kill untamed enemies within 1000 of you. Players and tamed creatures stay alive. The console reports how many. A dead character rejects Ghost, Tame, and Kill enemies, and the console says `Character is dead.`

The percent skill-loss slider from earlier versions is gone. Its replacement is on the World tab.

### World

A row of preset buttons: **Normal**, **Casual**, **Easy**, **Hard**, and **Hardcore**. Under them, five rows, each a caption, a stepped slider with an orange mark on a thin dark track, and a box with the step name.

| Modifier | Steps, left to right |
| --- | --- |
| Combat | Very easy, Easy, Normal, Hard, Very hard |
| Death penalty | Casual, Very easy, Easy, Normal, Hard, Hardcore |
| Resources | Much less, Less, Normal, More, Much more, Most |
| Raids | None, Much less, Less, Normal, More, Much more |
| Portals | Casual, Normal, Hard, Very hard |

| Preset | Combat | Death penalty | Resources | Raids | Portals |
| --- | --- | --- | --- | --- | --- |
| Normal | Normal | Normal | Normal | Normal | Normal |
| Casual | Very easy | Casual | More | None | Casual |
| Easy | Easy | Normal | Normal | Less | Normal |
| Hard | Hard | Normal | Normal | More | Normal |
| Hardcore | Very hard | Hardcore | Normal | More | Hard |

When the tab opens, the marks and names show what the world save holds. A preset moves all five. Dragging one slider keeps the other four and shows the **Custom** label. Nothing changes in the world until **Apply**. Apply stays gray while the staged steps match the world. Leaving the tab or closing the panel discards staged changes.

**Overwrite Skill loss to 0%** is staged with the steps. Click it, then Apply. While it is applied, a death removes no skill level, including on Hardcore. The death penalty still decides what happens to items. A preset or a slider turns the staged overwrite off; click it again before Apply to keep it. The host stores `on` or `off` in `qol-cm-skill-overwrite.txt` beside the plugin and sends it to clients that run the plugin.

Apply writes the matching global keys into the world save, updates the live world rates, and sends the keys to every peer. Choosing Normal clears that family's keys. Apply does not touch `passivemobs`, `nobuildcost`, `nomap`, `playerevents`, or `fire`. After a quit and a new login, the World tab shows the applied steps again.

### Spawn

Rows use the in-game item names. The filter matches those names. A row is listed only when that prefab is the one Spawn creates and its `m_icons` list has at least one icon. If two listed items share an in-game name, each row also shows its prefab name. The list scrolls. Quantity is a row under the list: `-`, the number, `+`, `x10`, and Max Stack. Quality is the row under that. Spawn is under both. Those controls stay gray until an item row is clicked. Choosing an item sets the quantity to 1 and the quality to 1. `x10` multiplies the quantity by 10, still capped at 100. Max Stack sets the quantity to that item's vanilla stack size, capped at 100. Values below 1 or above 100 do not spawn. Quality cannot pass the item's vanilla maximum. With no other player highlighted on the Players tab, items spawn at your feet. Highlight another player and they spawn at that player's feet.

### Players

Connected players are rows in a list. Click a row to highlight it. Bring me to player, Bring player to me, and Grant admin sit under that list. They stay gray until another connected player is selected. Your own row, marked `(you)`, can be highlighted, and those three actions stay gray because they apply to someone else. Grant admin confirms the selected player's Steam ID from the connection. If that connection has no Steam ID, one text field appears; type one 17-digit ID and confirm again. There is no ban control.

### Log

Action messages append here and stay after you close the panel.

## Files beside the plugin

`qol-cm-skill-overwrite.txt` holds `on` or `off` for the skill-loss overwrite. The old `qol-cm-skill.txt` percent file from 1.6 is ignored and is not deleted.

`qol-cm.log` repeats each action-console line, including each Apply as `World modifiers: Combat …, Death penalty …, Resources …, Raids …, Portals …. Skill loss overwrite on|off.`

`qol-cm-debug.log` records, at startup, the host overwrite state; on each World tab open, the steps read from the save with the managed keys, and a `World readout` line with the five step names the panel shows, `Rows 5`, and each mark as `value@anchor`; and each death as `Skill overwrite death kept.` or `Skill overwrite death vanilla.` Kill enemies still records the admin position, radius 1000, how many characters were seen, and how many were killed.
