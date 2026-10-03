# Spec 004 — Item list accuracy

- Version: 0.4
- Status: frozen
- Frozen: 2026-10-03
- Cycle: follows spec 003. This is not V1.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. The wood modules in spec 003 stay where they are. This spec changes which items are listed, what a selected row does to quantity, and how that row is named.

The item list was offering drops a player cannot pick up, and selecting a row jumped the quantity to that item's stack size. The console sometimes named a different item than the prefab Spawn creates.

## Requirements

### REQ-1 Quantity on select

- WHEN an admin clicks an item row, the quantity SHALL become 1 and the quality SHALL become 1.
- That click SHALL NOT set the quantity to the item's vanilla stack size.
- Max Stack SHALL still set the quantity to that item's vanilla stack size, capped at 100.
- `x10` SHALL still multiply the current quantity by 10, capped at 100.

### REQ-2 Row name

- The name on an item row SHALL be the in-game name of the prefab Spawn will create for that row.
- WHEN that in-game name is missing or still a localization token, the row SHALL show the prefab name.
- WHEN two or more listed items share an in-game name, each of those rows SHALL also show its prefab name.
- WHEN an admin clicks a row, the console line SHALL use that same row name.

### REQ-3 Pickupable items

- The item list SHALL include a prefab only when that prefab is the one Spawn will instantiate and its shared icon list `m_icons` has at least one entry.
- Valheim refuses to pick up a drop whose icon list is empty. Those prefabs SHALL NOT appear in the list.
- A search SHALL match the in-game name, the prefab name, and the localization token of the items that remain.

### REQ-4 Version label

- WHEN the panel is open, the system SHALL show the version string `0.4`.
- The usage document SHALL state version `0.4` and the item-list rules in REQ-1 through REQ-3.
