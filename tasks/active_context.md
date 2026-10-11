# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5. That remains the last GitHub release.
- Spec 006 is frozen at version 1.6 and is `master` on GitHub.
- Spec 007 is frozen at version 1.7. Its markdown was lost on 2026-10-09; `specs/007-valheim-qol-cm/README.md` summarizes it.
- Spec 008 is frozen at version 1.8 and merged into `master` locally. Local play confirmed the World tab on 2026-10-10.
- Spec 009 is frozen at version 1.9 on branch `spec-009-pick-filter`. Local play confirmed the Pick tab on 2026-10-10 (eleven claims, one defect ISS-012 fixed in play).

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- The panel is six tabs for an admin: Cheats, World, Spawn, Players, Pick, and Log. It opens on Cheats. Every other player opens the same panel and sees the Pick tab alone; the Constitution names the pick filter as not a gameplay mutation.
- The pick filter runs as a scope flag around the private `Player.AutoPickup` and a prefix on `ItemDrop.CanPickup` that returns false for a filtered prefab only inside that scope. The drop is skipped before the magnet pull; `ItemDrop.Interact` never enters that branch, so E picks up anything. The filter lives in `qol-cm-pick-filter.txt` beside the plugin, one file per install.
- Pick rules live in Core: `PickFilter.Stage`, `MatchPreset`, `ApplyEnabled`, `CanApply`, and `PickFilterState.Parse`/`Format`. The view only paints what they return.
- Unity's `InputField` consumes Esc by restoring the pre-edit text in the same frame `Input.GetKeyDown` sees it. `HideDropdown` keeps a frame stamp from the cleared `onValueChanged` so that Esc does not also close the panel.
- `ItemCatalog` is shared by Spawn and Pick. Its search ranks exact, then starts-with, then contains before trimming to eight (ISS-012).
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
- `src/` is a decompile of the installed 1.8 plugin placed on top of published `master`. `dotnet build -warnaserror` is clean: Core has XML docs, and the plugin project runs `Nullable` as `annotations` until DEF-005 rewrites it from source.
- The Obsidian `lessons-learned.md` has sections for specs 001 through 009. Read it before the next spec.

## Immediate next steps

1. Publish 1.9: build the release files without the embedded profile path (DEF-004), push `master`, tag `1.9`, and create the GitHub release.
2. DEF-002: hosted-server play for specs 006 through 009, including two players auto-picking near the same filtered drop.
3. Draft spec 010 from `master`. Review the backlog first and bring in only items in its area.
