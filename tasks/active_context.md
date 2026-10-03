# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is next. It is layout only. Gameplay rules stay as frozen in spec 001.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- Skill-loss percent is an admin-only BepInEx config so the server value overwrites clients.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- The typed Steam ID field is created with the panel and shown only when the selected connection has no Steam ID.
- BepInEx prints the chainloader version as 0.1. The panel and docs keep 0.001. See ISS-004.
- ISS-007, ISS-008, and ISS-009 are not implemented in 0.001.

## Immediate next steps

1. Plan spec 002 from ISS-007, ISS-008, and ISS-009. Do not change spec 001 gameplay.
