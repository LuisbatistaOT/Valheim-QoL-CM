# Spec 005 — Nearby actions

- Version: 1.5
- Status: frozen
- Frozen: 2026-10-03
- Cycle: first V1 spec. Follows frozen spec 004.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. The four wood modules in spec 003 stay where they are. The item-list rules in spec 004 stay frozen. This spec adds Ghost, Tame, and Kill enemies to Global Cheats, and it publishes version `1.5`.

V1 is confirmed. Specs 001 through 004 keep the version strings they already shipped. From this spec on, spec `N` publishes `1.N` with no extra zero. BepInEx reads the plugin version with `System.Version`, which would turn `1.05` into `1.5`.

## Requirements

### REQ-1 Ghost

- WHEN an admin clicks Ghost, the system SHALL toggle vanilla ghost mode on that admin's own character.
- The Ghost button SHALL read `Ghost: On` or `Ghost: Off` from that vanilla mode.
- WHILE ghost mode is on, enemies SHALL ignore that character, as they do for the vanilla `ghost` command.
- WHEN Ghost is turned on or off, the system SHALL leave God, Fly, Creative, and Free cam as they were.
- Ghost SHALL run on the local character. It SHALL NOT be sent to the world host.

### REQ-2 Tame

- WHEN an admin clicks Tame, the system SHALL run the vanilla `tame` command's effect at that admin's position: `Tameable.TameAllInArea` with that command's radius.
- A creature the vanilla command would leave alone SHALL stay as it was.
- The world host SHALL apply the taming. A client admin SHALL send the request to the host.
- The action console SHALL report how many creatures that call tamed.
- WHEN none qualify, the console SHALL say that no tameable animal was nearby, and the world SHALL stay as it was.

### REQ-3 Kill enemies

- WHEN an admin clicks Kill enemies, the system SHALL run the vanilla `killenemycreatures` command's effect at that admin's position, including that command's radius and lethal hit.
- A character that command would leave alive SHALL stay alive. That includes players and tamed creatures.
- The world host SHALL apply the kills. A client admin SHALL send the request to the host.
- The action console SHALL report how many enemies were killed.
- WHEN none qualify, the console SHALL say that no enemy was nearby, and the world SHALL stay as it was.

### REQ-4 Global Cheats

- Ghost, Tame, and Kill enemies SHALL sit in the Global Cheats module.
- Ghost SHALL sit in the mode grid with God, Fly, Creative, and Free cam.
- Tame and Kill enemies SHALL sit under that grid. They SHALL NOT show On or Off.
- Those three controls SHALL accept a click whether or not a player or an item is highlighted.
- IF the admin's character is dead, THEN each of those three clicks SHALL be rejected and SHALL leave ghost mode, creatures, and enemies unchanged.
- IF the caller is not an admin, THEN the system SHALL NOT toggle ghost mode, SHALL NOT tame, and SHALL NOT kill.
- Closing the panel SHALL NOT turn ghost mode off, SHALL NOT untame a creature, and SHALL NOT restore a killed enemy.

### REQ-5 Version label

- WHEN the panel is open, the system SHALL show the version string `1.5`.
- The plugin version, the usage document, and the README SHALL use that same string.
- The constitution SHALL record that V1 is confirmed and that a later spec `N` publishes `1.N` with no extra zero.

### REQ-6 Usage and README

- The usage document SHALL state version `1.5` and what Ghost, Tame, and Kill enemies do.
- The README section now titled `Players who only want the plugin` SHALL be titled `Plugin information`.
- That section SHALL say which two DLL files to copy, which folder receives them, and that BepInEx and Jötunn are required.
- The README SHALL open with what the panel is, that `1.5` is V1, and how an admin opens it.
- The README SHALL name Ghost, Tame, and Kill enemies in plain language, and its changelog SHALL include `1.5`.
