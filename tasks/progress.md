# Project milestones and progress

## Completed features

- Spec 001 is frozen at version 0.001.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5 and remains the last published release.
- Core rules cover the admin gate, modes including ghost, nearby tame and kill selection, teleport, spawn, skill loss, and grant admin.
- Spec 006 adds the + hotkey rule, the saved skill-loss rule, and the kill debug line.

## Features in progress

- Spec 006 is in source at version 1.6. Kill enemies writes a debug line, a saved skill-loss percent of 0 survives relog, and the panel hotkey is +. In-game confirmation is still open. Version 1.5 remains the last published release.

## Known issues and backlog

- ISS-001 through ISS-006 are resolved inside spec 001.
- ISS-007, ISS-008, and ISS-009 are documented and deferred to spec 002: item table under the player list, a selected-player highlight, and a bottom action console.
- Spec 006 ISS-001: Jötunn resets the admin skill-loss entry to 5 when a client joins. The host file is the stored percent.
- This machine has no separate Valheim dedicated-server executable, so the server branch was not launched here. The same DLL is what a server would load.
- V1 is confirmed. Spec 005 published `1.5`. Spec 006 publishes `1.6`.
