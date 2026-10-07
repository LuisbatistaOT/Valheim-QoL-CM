# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5. That remains the last published release.
- Spec 006 is in progress at version 1.6. It fixes Kill enemies reporting, skill-loss relog, and the panel hotkey. Gameplay rules stay as frozen in spec 001.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- Skill-loss percent is stored by the host beside the plugin. Jötunn resets the admin-only config entry to 5 when a client joins, so the host file wins, including 0. The panel titles that module Server skill loss because it applies on every death, not to the highlighted player.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- The typed Steam ID field is created with the panel and shown only when the selected connection has no Steam ID.
- Player teleport and grant admin stay gray when no other player is highlighted, including when the local row is highlighted.
- Quantity, quality, and Spawn stay gray until an item row is highlighted.
- BepInEx prints the chainloader version from `System.Version`. Spec 006 publishes `1.6`. A padded string such as `1.06` would display as `1.6`.
- An item row is listed only when `m_icons` has at least one entry. That is the field `Humanoid.Pickup` checks before a player can take the drop.
- Selecting an item sets quantity to 1. Max Stack still applies the vanilla stack size.
- Ghost toggles vanilla ghost mode on the local admin. Tame and Kill enemies are applied by the world host.
- Tame calls `Tameable.TameAllInArea` with radius 20, the same call as the vanilla tame command.
- Kill enemies uses the vanilla kill-nearby-enemies filters and a distance of 1000 from the requesting admin. A miss writes the console line and a debug line.
- The panel hotkey is the + key. A saved backtick or Ctrl+Tab binding is rewritten to +.
- The Obsidian `lessons-learned.md` file was read before this spec. The spec 001 cycle in that file stops at spec 003.

## Immediate next steps

1. Restart Valheim as an admin. Confirm Version 1.6, open the panel with +, and confirm backtick does nothing.
2. Apply skill loss at 0%, relog, and confirm the slider stays at 0%.
3. Click Kill enemies near an untamed enemy. Confirm the console reports a count, and that `qol-cm-debug.log` beside the plugin records the position and the counts.
4. Copy the same two DLLs to the hosted server before the public-server check. This machine has no dedicated-server executable.
