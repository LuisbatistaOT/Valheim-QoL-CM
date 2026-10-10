# Spec 008 — World tab

- Version: 1.8
- Status: frozen
- Frozen: 2026-10-10
- Cycle: third Version 2 spec. Follows spec 007 (`1.7`), whose markdown was lost with the working tree on 2026-10-09 and is summarized in `specs/007-valheim-qol-cm/README.md`.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. Specs 002 through 007 stay where they are. This spec moves the panel to tabs, adds the World tab, and replaces the percent skill-loss slider with a skill-loss overwrite.

## Requirements

### REQ-1 Tabs

- The panel SHALL have five tabs: Cheats, World, Spawn, Players, and Log. It SHALL open on Cheats.
- Cheats SHALL hold God, Fly, Creative, Free cam, Ghost, Tame, and Kill enemies. The percent skill-loss slider from spec 003 SHALL NOT be on this tab.
- Spawn SHALL hold the spec 004 item list and spawn controls. Players SHALL hold the connected list, Bring me to player, Bring player to me, and Grant admin. Log SHALL hold the action console.
- The hotkey stays spec 006: numpad + or Shift and the =/+ key. Esc closes the panel. Closing only hides it.

### REQ-2 Stepped modifiers

- The World tab SHALL show five rows: Combat, Death penalty, Resources, Raids, and Portals. Each row is a caption, a stepped slider, and a readout with the step name.
- The stops SHALL be: Combat `Very easy, Easy, Normal, Hard, Very hard`; Death penalty `Casual, Very easy, Easy, Normal, Hard, Hardcore`; Resources `Much less, Less, Normal, More, Much more, Most`; Raids `None, Much less, Less, Normal, More, Much more`; Portals `Casual, Normal, Hard, Very hard`.
- WHEN the World tab opens, each slider and readout SHALL show the step the world save holds. A missing family reads as Normal.
- WHEN a slider is dragged, the readout SHALL change to that step and Apply SHALL light.
- The moving mark SHALL stay inside its own row. The dark track behind it stays thin.

### REQ-3 Presets

- Preset buttons SHALL be Normal, Casual, Easy, Hard, and Hardcore. Immersive and Hammer are out.
- WHEN a preset is clicked, the five sliders and readouts SHALL move to that preset's steps and wait for Apply.
- Any other mix SHALL show the Custom label. Moving one slider keeps the other four staged steps.
- The preset table is: Normal all Normal; Casual `Very easy, Casual, More, None, Casual`; Easy `Easy, Normal, Normal, Less, Normal`; Hard `Hard, Normal, Normal, More, Normal`; Hardcore `Very hard, Hardcore, Normal, More, Hard`.

### REQ-4 Apply

- Apply SHALL stay gray until a staged step or the overwrite flag differs from the world.
- WHEN Apply is clicked, the host SHALL write the matching global keys into the world save, update world rates, and send the keys to peers. Writing Normal clears that family's keys.
- Apply SHALL NOT change `passivemobs`, `nobuildcost`, `nomap`, `playerevents`, or `fire`. Creative on Cheats remains free building.
- Leaving the World tab or closing the panel SHALL discard the staged copy.
- WHEN an admin applies, quits, and logs in again, the World tab SHALL show the applied steps.
- Everyone in the world gets the modifier steps, including players who do not run this plugin. Only an admin gets the panel. A remote admin sends the five steps and the overwrite flag to the host.

### REQ-5 Skill-loss overwrite

- `Overwrite Skill loss to 0%` SHALL be staged with the five steps. Clicking it only prepares the next Apply. A preset or a slider turns the staged toggle off.
- WHILE the applied overwrite is on, a death SHALL remove no skill level, including on Hardcore. Death penalty still decides items.
- The host SHALL store the flag beside the plugin in `qol-cm-skill-overwrite.txt` as `on` or `off`, and SHALL send it to plugin clients. Until a client receives the host value, overwrite is off.
- The old `qol-cm-skill.txt` percent file is ignored and is not deleted. Players without the plugin follow the world's death penalty for skill loss.

### REQ-6 Logs

- `qol-cm.log` SHALL record each Apply as `World modifiers: Combat …, Death penalty …, Resources …, Raids …, Portals …. Skill loss overwrite on|off.`
- `qol-cm-debug.log` SHALL record, when the World tab opens, the steps read from the save with the managed keys, and a `World readout` line with the readout strings, the row count, and each mark as `value@anchor`. The row count MUST be 5.
- `qol-cm-debug.log` SHALL record the host overwrite state at startup, each saved change, and each death as `kept` or `vanilla`.

### REQ-7 Version and documents

- WHEN the panel is open, the system SHALL show `Version 1.8`. The plugin, both project files, `README.md`, and `docs/USAGE.md` SHALL use that same string.
- The usage document SHALL describe the five tabs, the World tab, the overwrite, and the log lines above. The README changelog SHALL include `1.7` and `1.8`.
