# Spec 008 status

Draft only. This is not a frozen spec, and it is not the published game. `master` on GitHub remains spec 006 at version 1.6. This branch exists so the World tab can be debugged on another day.

The panel header and `Plugin.Version` say **1.8**. `README.md`, `docs/USAGE.md`, and the plugin project file still say **1.6**, because those files came back with the published repository and this branch does not publish a release. Align them before anything lands on `master`.

## What the World tab is supposed to do

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

`Overwrite Skill loss to 0%` is staged with the five steps. Clicking it only prepares the next Apply. A preset or a slider turns that staged toggle off. Turning it back on before Apply keeps 0%. While the applied overwrite is on, a death removes no skill level, including on Hardcore. Death penalty still decides items. The host stores the flag beside the plugin in `qol-cm-skill-overwrite.txt` (`on` or `off`) and sends it to plugin clients. Until a client receives the host value, overwrite is off. The old `qol-cm-skill.txt` percent file is ignored and is not deleted. Players without the plugin follow the world's death penalty for skill loss.

## Source recovery

On 2026-10-09 the repository folder was emptied. The unpublished 1.8 commits were gone. What remains in `src/` is a decompile of the installed 1.8 plugin, placed on top of published `master` (`d2c40b3`). Expect IL comments, missing XML docs, and nullable warnings. Spec 007 and the original spec 008 markdown were not in the published repository and were not recovered. This file and `issues.md` are the record that replaces them.

Core stays free of Unity and Valheim types. The panel is a view over `WorldModifierHost` and `ValheimQoLCM.Core.WorldModifiers`. Valheim also has a type named `WorldModifiers`. Plugin code calls the core type through the alias `ModifierRules`.

## Latest play

2026-10-10, after the handles were changed from a separate centered knob to the Unity slider handle:

1. Dragging moves the mark. The mark is tall enough to cover the slider above or below.
2. Every name beside a slider stayed on "Normal".
3. Apply, quit, and login again showed the handles at the leftmost stop.

The debug line for that login had already read Combat Hard, Death penalty Normal, Resources Normal, Raids Normal, Portals Very hard, with the matching world keys. The save is intact. The panel is not showing it. Details are ISS-008, ISS-009, and ISS-010.

## Code on this branch that play has not seen

Copied after that report, and not opened in Valheim yet:

- The handle uses a negative height delta so Unity's vertical stretch keeps it about 12px tall, with an orange `UISprite`.
- The step name is written by replacing the readout that was stored with the row.
- A drag updates the draft without the old page-open check that ignored the callback.
- While the World tab is showing, each slider is set again from the staged step when its value differs.

Restart Valheim after the next copy. The checks to make are Hardcore (or the saved mix above), a drag that changes the name and lights Apply, and a quit/login that shows the same steps.

## Do not regress

- Private global-key add/remove, `UpdateWorldRates`, and `SendGlobalKeys` for the local peer. Public `SetGlobalKey(string)` does not persist.
- Skill overwrite keeping the current skill level on death, then restoring the vanilla factor.
- Leaving `passivemobs`, `nobuildcost`, `nomap`, `playerevents`, and `fire` untouched.
- The thin dark track. The complaint is the moving mark, not the bar behind it.

## Gate

`dotnet test ValheimQoLCM.sln` passed 25 tests on this branch. That run does not open the game. `dotnet build -warnaserror` is not clean: the recovered plugin has nullable warnings, and the project still generates documentation warnings unless that generation is turned off. Do not push this branch to `master` until those checks pass and the three open slider issues have been looked at in game.
