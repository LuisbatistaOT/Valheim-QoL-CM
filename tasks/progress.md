# Project milestones and progress

## Completed features

- Spec 001 is frozen at version 0.001.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is frozen at version 0.4.
- Spec 005 is frozen at version 1.5 and remains the last GitHub release until 1.6 is published.
- Spec 006 is frozen at version 1.6. The panel opens with numpad + or Shift and the =/+ key. A saved skill-loss percent of 0 is stored by the host. Kill enemies writes a debug line.
- Core rules cover the admin gate, modes including ghost, nearby tame and kill selection, teleport, spawn, skill loss, and grant admin.

## Features in progress

- None. Spec 006 is frozen. Spec 007 has not started.

## Known issues and backlog

- ISS-001 through ISS-006 are resolved inside spec 001.
- ISS-007, ISS-008, and ISS-009 are documented and deferred to spec 002: item table under the player list, a selected-player highlight, and a bottom action console.
- Spec 006 ISS-001: Jötunn resets the admin skill-loss entry to 5 when a client joins. The host file is the stored percent.
- Spec 006 ISS-002: `KeyCode.Plus` never arrives from the keyboard. The panel listens for numpad + and for Shift with the =/+ key.
- Hosted-server play for spec 006 is still open: Kill enemies, and a skill-loss percent of 0 after relog.
- This machine has no separate Valheim dedicated-server executable, so the server branch was not launched here. The same DLL is what a server would load.
- V1 is confirmed. Spec 005 published `1.5`. Spec 006 is frozen at `1.6`.
