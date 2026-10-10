# Spec 008 status

Frozen 2026-10-10 at version **1.8** on branch `spec-008-world-modifiers`. `master` on GitHub remains spec 006 at version 1.6 until the gate in the last section passes.

The spec is `spec.md`. The plan is `plan.md`. Defects are in `issues.md`. This file is the history of the cycle.

## What the World tab does

Tabs are Cheats, World, Spawn, Players, and Log. The panel opens on Cheats. The hotkey is numpad + or Shift and the =/+ key.

Five stepped modifiers are written into the world save on Apply:

| Modifier | Stops |
| --- | --- |
| Combat | Very easy, Easy, Normal, Hard, Very hard |
| Death penalty | Casual, Very easy, Easy, Normal, Hard, Hardcore |
| Resources | Much less, Less, Normal, More, Much more, Most |
| Raids | None, Much less, Less, Normal, More, Much more |
| Portals | Casual, Normal, Hard, Very hard |

Presets fill the five steps and wait for Apply:

| Preset | Combat | Death penalty | Resources | Raids | Portals |
| --- | --- | --- | --- | --- | --- |
| Normal | Normal | Normal | Normal | Normal | Normal |
| Casual | Very easy | Casual | More | None | Casual |
| Easy | Easy | Normal | Normal | Less | Normal |
| Hard | Hard | Normal | Normal | More | Normal |
| Hardcore | Very hard | Hardcore | Normal | More | Hard |

Immersive and Hammer are out. Apply must not change `passivemobs`, `nobuildcost`, `nomap`, `playerevents`, or `fire`. Creative on Cheats remains free building.

Any other mix is Custom. Moving one slider keeps the other four staged stops and turns the staged skill overwrite off. Leaving the World tab or closing the panel discards the staged copy. Apply stays gray until a staged step or the overwrite flag differs from the world. Writing Normal clears that modifier's keys. A missing family reads as Normal.

Only an admin gets the panel. A remote admin sends the five steps and the overwrite flag. Everyone in the world gets the modifier steps, including players who do not run this plugin.

`Overwrite Skill loss to 0%` is staged with the five steps. Clicking it only prepares the next Apply. A preset or a slider turns that staged toggle off. While the applied overwrite is on, a death removes no skill level, including on Hardcore. Death penalty still decides items. The host stores the flag beside the plugin in `qol-cm-skill-overwrite.txt` (`on` or `off`) and sends it to plugin clients. Until a client receives the host value, overwrite is off. The old `qol-cm-skill.txt` percent file is ignored and is not deleted.

## Source recovery

On 2026-10-09 the repository folder was emptied. The unpublished 1.7 and 1.8 commits were gone. What remains in `src/` is a decompile of the installed 1.8 plugin, placed on top of published `master` (`d2c40b3`). Expect IL comments and nullable warnings. Spec 007's markdown is summarized in `specs/007-valheim-qol-cm/README.md`.

Core stays free of Unity and Valheim types. The panel is a view over `WorldModifierHost` and `ValheimQoLCM.Core.WorldModifiers`. Valheim also has a type named `WorldModifiers`. Plugin code calls the core type through the alias `ModifierRules`.

## How the slider defects were found

Between 2026-10-09 and 2026-10-10 the World tab showed three symptoms: marks that did not follow presets or the save, readouts stuck on "Normal", and a reload that put every mark on the left. Five fixes were aimed at the drawing: a separate knob, the Unity handle, a negative handle height, replacing the readout object, and re-placing the handle each frame. None changed what was on screen, because the action log and the `World tab` debug line always had the right steps. The panel was storing the right data and drawing something else.

The turn came from a debug line that printed what the panel itself held, not what the save held: `World readout , , , , , Normal, Normal, Normal, Normal, Normal.` Ten readouts for five rows. The panel is built in the menu scene and again in the world scene. The first panel is destroyed by the scene change, so `Build` skipped clearing the row lists and appended the new rows behind the dead ones. Every paint loop stops at five. See ISS-011. One change in `Build` fixed all three symptoms. The 22:23 login logged `Rows 5`, the saved steps on the readouts, and marks on their fractions.

## Do not regress

- `ConsoleView.Build` clears every row list on every call. A rebuild after a scene change must not append.
- Private global-key add/remove, `UpdateWorldRates`, and `SendGlobalKeys` for the local peer. Public `SetGlobalKey(string)` does not persist.
- Skill overwrite keeping the current skill level on death, then restoring the vanilla factor.
- Leaving `passivemobs`, `nobuildcost`, `nomap`, `playerevents`, and `fire` untouched.
- The thin dark track with a 12px mark inside its own row.
- The `World readout` debug line. It is the check that the panel paints its own rows.

## Gate

`dotnet test ValheimQoLCM.sln` passes 27 tests. `dotnet build ValheimQoLCM.sln -warnaserror` is not clean: the decompiled plugin has nullable warnings and Core has missing XML docs. That is backlog DEF-001 and it blocks landing this branch on `master`. Hosted-server play is backlog DEF-002.
