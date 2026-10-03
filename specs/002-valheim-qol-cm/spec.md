# Spec 002 — Panel layout

- Version: 0.2
- Status: frozen
- Frozen: 2026-10-02
- Cycle: follows spec 001. This is not V1.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. This spec changes only where controls sit and how a selection is shown.

The admin panel keeps every spec 001 action. The left side becomes two tables. The action log moves to the bottom.

## Requirements

### REQ-1 Player table

- WHEN the panel is open, the system SHALL show connected players as clickable rows on the left.
- WHEN an admin clicks a row, the system SHALL highlight that row and leave the previous row unhighlighted.
- Teleport, spawn, and grant admin SHALL keep using the highlighted player's name.
- IF the highlighted player disconnects, THEN the system SHALL clear the highlight.

### REQ-2 Item table

- The item filter SHALL sit on the left, under the player table, and SHALL stay above the item rows.
- WHEN the admin types in the filter, the system SHALL show matching items as clickable rows under that filter.
- The item rows SHALL NOT draw underneath quantity, quality, or the other right-side controls.
- WHEN an admin clicks an item row, the system SHALL highlight that row.

### REQ-3 Action console

- WHEN an action reports a message, the system SHALL append that message to a console along the bottom of the panel.
- The console SHALL keep the recent lines when the panel is closed and opened again.
- Closing the panel SHALL NOT undo god mode, fly, creative, free cam, spawned items, granted admins, or the skill-loss percent.

### REQ-4 Version label

- WHEN the panel is open, the system SHALL show the version string `0.2`.
- The usage document SHALL state version `0.2` and where the player table, item table, and action console sit.
