# Valheim QoL CM

An admin and user console for Valheim to improve both users quality of life.

## Features in version 2.10

Join a world and press numpad **+**, or hold **Shift** and press the **=/+** key. Press it again to close. Esc closes it too. Closing only hides the panel.

On a local world the host is the admin. On a server, an admin is a Steam ID already listed in `adminlist.txt`. The plugin does not promote anyone by itself, and it has no ban control.

### For Admins

Admins see six tabs. The panel opens on **Cheats**.

#### Cheats

God, Fly, Creative, Free cam, and Ghost toggle your own character. Each label shows On or Off. Turning one off leaves the others alone. Fly is remembered for the next session. Ghost makes enemies ignore you.

**Tame** tames the creatures the vanilla tame command would tame. **Kill enemies** removes untamed enemies within 1000 of you. Players and tamed creatures stay. Both report how many they affected. A dead character cannot use Ghost, Tame, or Kill enemies.

<img width="627" height="716" alt="cheats_tab" src="https://github.com/user-attachments/assets/6db5535f-7972-493c-8283-fc5297a133d3" />

#### World

Presets **Normal**, **Casual**, **Easy**, **Hard**, and **Hardcore**, then stepped sliders for Combat, Death penalty, Resources, Raids, and Portals. A preset fills all five. Moving one slider keeps the others and marks the mix Custom.

**Overwrite Skill loss to 0%** keeps every skill level on death, including Hardcore. Gear still follows the death penalty.

Nothing changes until **Apply**. Apply writes the world save, so every player gets the change, with or without the plugin.
<img width="627" height="716" alt="word_tab" src="https://github.com/user-attachments/assets/af1ca3a5-9a2d-4238-b76e-ca0decc35fab" />

#### Spawn

Search items you can pick up, set quantity and quality, then spawn. Choosing an item sets both to 1. Quantity stays between 1 and 100. Spawn stays gray until an item is selected. With nobody else highlighted, items appear at your feet. Highlight another player and they appear at that player's feet.

<img width="627" height="716" alt="spawn_tab" src="https://github.com/user-attachments/assets/ee0fc85b-01aa-458c-8443-6deafa4e0b6e" />

#### Players

Connected players, then **Bring me to player**, **Bring player to me**, and **Grant admin**. Those three stay gray until another player is selected. Your own row can be highlighted, and the three actions stay gray on it. Grant admin asks you to click again. If the connection has no Steam ID, type one 17-digit ID and confirm.

<img width="627" height="716" alt="players_tab" src="https://github.com/user-attachments/assets/0771958d-aaee-4d67-ad91-8513e3efe3c7" />

#### Log

A black console of the actions taken from the panel. Each line is `[HH:mm:ss]` and the message, in white monospace. **Clear log** empties the box and leaves the file. The lines stay when you close the panel and start empty next session. Selecting a row or typing a bad quantity is not recorded.

<img width="627" height="716" alt="log_tab" src="https://github.com/user-attachments/assets/d38523a2-6407-488c-bbc7-c5148801d6f6" />

### For users

Everyone with the plugin sees the **Pick** tab. A player who is not an admin sees that tab alone.

#### Pick

Auto-pickup takes only the items in the list. Pressing E on a drop still picks it up.

**Woodcutting**, **Mining**, and **Farming** stage that activity's resources. **Custom** stages the last list you applied that was not a preset. **Pick all** turns the filter off. The lit button is the filter in force, including after a relog.

Type in the search field and click a match to add it. Each row has **Remove**. **Apply pick filter** writes the list on this install. An empty list cannot be applied. Leaving the tab discards changes you have not applied.
<img width="627" height="716" alt="pick_tab" src="https://github.com/user-attachments/assets/2adf450e-daac-452c-a477-bed4b50dabf9" />


## Installation

You need Valheim, BepInEx 5.4.23.5, and Jötunn 2.30.1. This plugin does not download them.

1. Download `ValheimQoLCM.dll` and `ValheimQoLCM.Core.dll` from [release/ValheimQoLCM](release/ValheimQoLCM).
2. Copy both into `Valheim\BepInEx\plugins\ValheimQoLCM\`.
3. Restart the game.

A dedicated server uses the same two files in its own `BepInEx\plugins\ValheimQoLCM\` folder. Restart the server after copying.

World modifiers reach every player. The skill-loss overwrite and the Pick list need the plugin on that machine.

## Report an issue

Bugs and feature requests go to [GitHub Issues](https://github.com/LuisbatistaOT/Valheim-QoL-CM/issues).

## Code / Development information

Gameplay rules live in `src/ValheimQoLCM.Core` and do not reference Unity. The BepInEx plugin is `src/ValheimQoLCM`. From the repository root:

```powershell
dotnet test ValheimQoLCM.sln
```

The files players copy are the two DLLs in `release/ValheimQoLCM`. Local builds read the game path from `src/ValheimQoLCM/Valheim.local.props`, which is not committed.

The manual — opening the panel, each tab, the files beside the plugin, and how a click reaches the host — is on the [wiki](https://github.com/LuisbatistaOT/Valheim-QoL-CM/wiki).
