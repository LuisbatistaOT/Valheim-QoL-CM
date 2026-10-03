# Active development context

## Current focus

- Spec 001 is frozen at version 0.001 and is the MVP release.
- Spec 002 is frozen at version 0.2.
- Spec 003 is frozen at version 0.3.
- Spec 004 is the current item list. Version string is `0.4`. Gameplay rules stay as frozen in spec 001. The spec 003 modules stay where they are.

## Active technical decisions

- Jötunn `PlayerIsAdmin` is the admin gate. Local hosts count as admins.
- Skill-loss percent is an admin-only BepInEx config so the server value overwrites clients. The panel titles that module Server skill loss because it applies on every death, not to the highlighted player.
- Quantity above 100 is rejected so a click cannot spawn enough objects to stall the game.
- The typed Steam ID field is created with the panel and shown only when the selected connection has no Steam ID.
- Player teleport and grant admin stay gray when no other player is highlighted, including when the local row is highlighted.
- Quantity, quality, and Spawn stay gray until an item row is highlighted.
- BepInEx prints the chainloader version from `System.Version`. Spec 004 publishes `0.4`, which matches that parser.
- An item row is listed only when `m_icons` has at least one entry. That is the field `Humanoid.Pickup` checks before a player can take the drop.
- Selecting an item sets quantity to 1. Max Stack still applies the vanilla stack size.
- ISS-007, ISS-008, and ISS-009 were handled in spec 002.

## Immediate next steps

1. Restart Valheim and search for bow. The list should show distinct pickupable bows, quantity should stay at 1 when a row is clicked, and the console should repeat that row's name.
