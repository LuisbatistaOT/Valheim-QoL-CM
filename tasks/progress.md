# Project milestones and progress

## Completed features

- Spec 001 is frozen at version 0.001.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5.
- Spec 006 is frozen at version 1.6. The panel opens with numpad + or Shift and the =/+ key. A saved skill-loss percent of 0 is stored by the host. Kill enemies writes a debug line.
- Spec 007 is frozen at version 1.7. Fly is saved by the plugin. The panel hides even when a tab switch throws.
- Spec 008 is frozen at version 1.8. The panel is five tabs. The World tab sets Combat, Death penalty, Resources, Raids, and Portals from presets or stepped sliders, Apply writes the world save, and `Overwrite Skill loss to 0%` keeps skill levels on death.
- Spec 009 is frozen at version 1.9. The Pick tab is open to every player and filters what auto-pickup takes to a Woodcutting, Mining, Farming, or custom list; E still picks up anything. Admins see six tabs.
- Spec 010 is frozen at version 2.10. The Log tab records panel actions, stamped with the local time, in white monospace on black, with Clear log. Cheats buttons share one size and one gap. The release folder holds a 2.10 build with no PDBs.
- Core rules cover the admin gate, modes including ghost, saved fly, nearby tame and kill selection, teleport, spawn, grant admin, world modifier stops, presets, key bundles, the overwrite flag, slider placement, the pick filter, and the action log.

## Features in progress

- None. Spec 010 is frozen.

## Known issues and backlog

- Spec 001 ISS-001 through ISS-006 are resolved. ISS-007 through ISS-009 were delivered in spec 002.
- Spec 006 ISS-001: Jötunn resets the admin skill-loss entry to 5 when a client joins. The host file is the stored percent. Spec 008 replaced that percent with the overwrite flag.
- Spec 006 ISS-002: `KeyCode.Plus` never arrives from the keyboard.
- Spec 008 ISS-011: the panel is built per scene, and `Build` must clear every row list on each call. This was the one cause behind six slider symptoms.
- Spec 009 ISS-012: the Pick search took its eight matches alphabetically, so an exact match such as Stone fell past the cut-off. Search now ranks exact, starts-with, then contains.
- Spec 010 ISS-013: the Log console was pinned to one line. The text height now follows the buffer. Confirmed in play on 2026-10-11.
- Spec 010 ISS-014: Cheats buttons did not share a size, a gap, or a center. They do now.
- DEF-001 is closed: `dotnet build -warnaserror` is clean. Core has XML docs; the plugin project runs `Nullable` as `annotations`.
- DEF-002: hosted-server play for specs 006 through 010 is unproven. This machine has no dedicated-server executable.
- DEF-003: the old `qol-cm-skill.txt` file is ignored but not deleted.
- DEF-004 is closed: release DLLs are a 2.10 build with `-p:DebugType=none`, and the PDBs are not in the repository.
- DEF-005: the plugin source is still decompiled IL and should be rewritten before `Nullable` returns to `enable`.
- DEF-006: Players and Spawn no longer show "Click Grant admin again to confirm.", "Invalid quantity.", "Invalid quality.", or "Selected …".
