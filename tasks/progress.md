# Project milestones and progress

## Completed features

- Spec 001 is frozen at version 0.001.
- Core rules pass 16 unit tests.
- The plugin DLL is in `BepInEx\plugins\ValheimQoLCM` and logged `Valheim QoL CM 0.001 loaded` in the local client.
- Backtick opens and closes the panel. Closing does not undo gameplay changes.

## Features in progress

- Spec 003 is frozen at version 0.3.
- Spec 004 is in source at version 0.4. The item list omits drops with an empty icon list, and selecting a row sets quantity to 1. In-game confirmation is still open.

## Known issues and backlog

- ISS-001 through ISS-006 are resolved inside spec 001.
- ISS-007, ISS-008, and ISS-009 are documented and deferred to spec 002: item table under the player list, a selected-player highlight, and a bottom action console.
- This machine has no separate Valheim dedicated-server executable, so the server branch was not launched here. The same DLL is what a server would load.
- V1 is waiting for an explicit maintainer confirmation. Spec 004 is version 0.4.
