# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5. That remains the last GitHub release until 1.6 is published.
- Spec 006 is frozen at version 1.6. Local play confirmed the panel hotkey on 2026-10-07. Spec 007 has not started.

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
- The panel hotkey is numpad +, or Shift and the =/+ key. `KeyCode.Plus` never arrives from the keyboard, so a saved `Plus` binding is rewritten. Backtick does not open the panel unless someone binds it again.
- The Obsidian `lessons-learned.md` file was read before this spec. The spec 001 cycle in that file stops at spec 003. Spec 006 added the plus-key lesson there.

## Immediate next steps

1. Spec 007 waits until this freeze is the base it starts from.
2. Hosted-server play for spec 006 is still open: copy the 1.6 DLLs to that server, then check Kill enemies and a skill-loss percent of 0 after relog. This machine has no dedicated-server executable.
