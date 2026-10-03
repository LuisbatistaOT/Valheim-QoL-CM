# Active development context

## Current focus

- Spec 001, version 0.001, is implemented and the DLL loaded in the local Valheim client.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- Skill-loss percent is an admin-only BepInEx config so the server value overwrites clients.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- The typed Steam ID field is created with the panel and shown only when the selected connection has no Steam ID.
- BepInEx prints the chainloader version as 0.1. The panel and docs keep 0.001. See ISS-004.

## Immediate next steps

1. Play a local world and a Steam-hosted server with a second player to click through teleport, spawn, death, and grant admin.
2. Start spec 002 only after you confirm V1 is still waiting.
