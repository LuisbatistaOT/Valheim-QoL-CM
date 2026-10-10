# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5. That remains the last GitHub release.
- Spec 006 is frozen at version 1.6 and is `master` on GitHub.
- Spec 007 is frozen at version 1.7. Its markdown was lost on 2026-10-09; `specs/007-valheim-qol-cm/README.md` summarizes it.
- Spec 008 is frozen at version 1.8 on branch `spec-008-world-modifiers`. Local play confirmed the World tab on 2026-10-10. Spec 009 has not started.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- The panel is five tabs: Cheats, World, Spawn, Players, and Log. It opens on Cheats.
- World modifiers are five stepped families written as global keys. Core owns the stops, presets, key bundles, and the parse back from keys. The host writes with the private `GlobalKeyAdd` and `GlobalKeyRemove`, then `UpdateWorldRates` and `SendGlobalKeys`. Public `SetGlobalKey(string)` only sends an RPC.
- The percent skill-loss slider is gone. `Overwrite Skill loss to 0%` is staged with the steps and applied with them. The host stores `on` or `off` in `qol-cm-skill-overwrite.txt` and sends it to plugin clients. Until a client receives the host value, overwrite is off.
- `ConsoleView.Build` clears every row list on every call. Jötunn rebuilds the GUI root per scene, and the old panel is already destroyed when the second build runs.
- The `World readout` debug line prints the panel's own readout strings, the row count, and each mark as `value@anchor`. The row count must be 5.
- Slider marks sit in a fixed 12px handle area with a `(14, 0)` handle delta. A late update re-places a mark whose anchor is off its step, because disabling the page releases the driven anchors.
- Fly is saved by the plugin (spec 007). A missing value is off.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- An item row is listed only when `m_icons` has at least one entry. Selecting an item sets quantity to 1.
- Ghost toggles vanilla ghost mode on the local admin. Tame and Kill enemies are applied by the world host.
- The panel hotkey is numpad +, or Shift and the =/+ key. `KeyCode.Plus` never arrives from the keyboard.
- `src/` is a decompile of the installed 1.8 plugin placed on top of published `master`. `dotnet build -warnaserror` is not clean until backlog DEF-001 is done.
- The Obsidian `lessons-learned.md` has sections for specs 001 through 008. Read it before the next spec.

## Immediate next steps

1. DEF-001: make `dotnet build ValheimQoLCM.sln -warnaserror` clean without changing behavior, then land `spec-008-world-modifiers` on `master` through the push gate and publish 1.8.
2. DEF-002: hosted-server play for specs 006 through 008.
3. Spec 009 waits until 1.8 is on `master`.
