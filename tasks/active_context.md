# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is the current panel. Version string is `0.3`. Gameplay rules stay as frozen in spec 001.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- Skill-loss percent is an admin-only BepInEx config so the server value overwrites clients. The panel titles that module Server skill loss because it applies on every death, not to the highlighted player.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- The typed Steam ID field is created with the panel and shown only when the selected connection has no Steam ID.
- Player teleport and grant admin stay gray when no other player is highlighted, including when the local row is highlighted.
- Quantity, quality, and Spawn stay gray until an item row is highlighted.
- BepInEx prints the chainloader version from `System.Version`. Spec 003 publishes `0.3`, which matches that parser.
- ISS-007, ISS-008, and ISS-009 were handled in spec 002.

## Immediate next steps

1. Play the 0.3 panel in a hosted world and confirm the four modules, the gray states, and the item scrollbar.
