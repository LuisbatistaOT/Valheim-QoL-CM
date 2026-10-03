# Spec 003 — Panel modules

- Version: 0.3
- Status: frozen
- Frozen: 2026-10-02
- Cycle: follows spec 002. This is not V1.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. This spec changes only how the wood panel is grouped and when a control can be clicked.

The admin panel keeps every spec 001 action and the spec 002 action console. Controls sit in four wood modules.

## Requirements

### REQ-1 Player Management

- WHEN the panel is open, the system SHALL show connected players and the player actions in one module titled Player Management.
- Bring me to player, Bring player to me, and Grant admin SHALL sit under the connected-player list.
- WHEN no player row is highlighted, those three controls SHALL be gray and SHALL NOT accept a click.
- WHEN the highlighted row is the local player, those three controls SHALL stay gray.
- WHEN the highlighted row is another connected player, those three controls SHALL accept a click.
- A clicked player row SHALL stay highlighted until another row is clicked or that player disconnects.

### REQ-2 Item Spawner

- WHEN the panel is open, the system SHALL show the item filter, the item list, quantity, quality, and Spawn in one module titled Item Spawner.
- The item filter SHALL sit on the top edge of the item list.
- The item list SHALL have a fixed height, a dark background, and a visible vertical scrollbar.
- Quantity and quality SHALL each show their number in a fixed-width box between a minus button and a plus button, on one horizontal row under the list.
- Spawn SHALL sit under that row, inside the same module.
- WHEN no item row is highlighted, the minus buttons, plus buttons, number boxes, and Spawn SHALL be gray and SHALL NOT accept a click.
- WHEN an item row is clicked, that row SHALL be highlighted and those controls SHALL accept a click.

### REQ-3 Global Cheats

- God, Fly, Creative, and Free cam SHALL sit in one module titled Global Cheats, in a two-by-two grid.
- Those four controls SHALL accept a click whether or not a player or an item is highlighted.

### REQ-4 Server skill loss

- The skill-loss slider, the percent readout, and Apply skill loss SHALL sit in one module titled Server skill loss.
- The percent SHALL be the server-wide death penalty from spec 001 REQ-6. The module SHALL NOT move onto the selected player, and the title SHALL NOT call it a local-player skill.
- The percent readout SHALL keep a fixed width from 0% through 100%.
- Apply skill loss SHALL sit centered under the slider.
- This module SHALL accept input whether or not a player or an item is highlighted.

### REQ-5 Version label

- WHEN the panel is open, the system SHALL show the version string `0.3`.
- The usage document SHALL state version `0.3` and where the four modules sit.
