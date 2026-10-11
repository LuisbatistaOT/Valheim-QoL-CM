# Spec 010 - Log tab

- Version: 2.10
- Status: frozen
- Drafted: 2026-10-10
- Frozen: 2026-10-11
- Cycle: first Version 2 spec under the amended Constitution (`2.N` from spec 010). Follows spec 009 (`1.9`, Pick tab).
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. Specs 002 through 009 stay where they are. This spec changes what the Log tab records and how it looks. It also aligns the Cheats tab buttons and changes no cheat behavior. It changes nothing else on the other tabs beyond which method their actions call to report.
- Backlog review: DEF-002 (hosted-server play), DEF-003 (old percent file), DEF-004 (release PDBs), and DEF-005 (decompiled plugin source) are outside the Log tab area and stay on the backlog.

## Gap

The Log tab exists and shows a scrolling list, but that list is every message the panel ever displayed: `Selected <player>.`, `Item Feathers.`, `Invalid quantity.`, and confirmation prompts sit between the actions. Local toggles and Pick Apply reach the panel without reaching `qol-cm.log`, while host actions reach both, so the panel and the file disagree. The text is green on black with a `> ` prefix and no time. There is no way to clear it. The admin wants a console: every action taken from the panel, stamped with the time, in white monospace text on black, with a Clear log button.

## Requirements

### REQ-1 What is recorded

- The system SHALL record an **action** when the admin, from the panel, toggles a mode (God, Fly, Creative, Free cam, Ghost), clicks Tame, Kill enemies, Bring me to player, Bring player to me, Spawn, Grant admin (the confirmed click), World Apply, Apply pick filter, or Clear log.
- WHEN an action succeeds, the recorded line SHALL be the same message the panel shows today for that action (`God is on.`, `Spawned 10 Feathers.`, `Pick filter: Mining, 14 items.`, the World Apply line, the grant and teleport replies).
- WHEN an action fails, the recorded line SHALL be the failure message (`Admins only.`, `Not connected.`, `Server peer missing.`, and the `ActionResult` errors those actions return).
- WHEN the host replies to an action with a `status` message, that reply SHALL be recorded.
- The system SHALL NOT record tab clicks, the credit link, selections (`Selected …`, `Item …`), validation feedback (`Invalid quantity.`, `Invalid quality.`), confirmation prompts (`Click Grant admin again to confirm.`), the Pick status line messages (`Add an item or choose Pick all.`, `No custom list yet…`), or the World tab's read-back errors on open. Those stay transient status text.
- Every recorded action SHALL also be appended to `qol-cm.log` through the existing `PluginStorage.Action`, so the file and the panel hold the same actions. The file's stamp stays as it is.

### REQ-2 Buffer

- The log buffer SHALL hold the newest 40 actions of this game session, oldest first. The 41st drops the oldest.
- The buffer SHALL start empty when the plugin loads and SHALL survive panel rebuilds between scenes. It is not written to or read from disk.
- An empty or whitespace message SHALL NOT be recorded.
- The buffer and its line format SHALL live in Core (`ActionLog`), free of Unity and Valheim types, with tests for the cap, the order, the format, Clear, and the empty rejection.

### REQ-3 Line format

- Each panel line SHALL read `[HH:mm:ss] <message>` using the local clock, 24-hour, zero-padded. Example: `[22:24:14] Spawned 10 Black Metal Missile.`
- The `> ` prefix SHALL be gone.
- One action is one entry. A message wider than the box wraps within its own entry and is never merged with its neighbour.

### REQ-4 Look

- The Log tab SHALL show one box: pure black fill, a thin light-grey border, and the box fills the page height like today.
- Text SHALL be white at 14 point.
- The text font SHALL be a monospace font from the operating system: `Consolas` first, then `Courier New`. IF neither loads, THEN the panel font is used and the tab still works.
- The newest line SHALL be visible after each new action and after the tab opens: the view scrolls to the bottom.
- The Log tab remains an admin tab. A non-admin sees the Pick tab alone, as spec 009 left it.

### REQ-5 Clear log

- A button reading `Clear log` SHALL sit under the box, right-aligned, in the panel's button style.
- WHEN it is clicked, the buffer SHALL be emptied, the box SHALL repaint, and the action `Log cleared.` SHALL be recorded as the first new line and appended to `qol-cm.log`.
- Clear log SHALL NOT delete or truncate `qol-cm.log`.

### REQ-6 Logs

- `qol-cm-debug.log` SHALL record, when the Log tab opens, `Log readout <line count>, font <font name>.` where the count equals the lines in the buffer and the font name is the one in use (`Consolas`, `Courier New`, or `panel`).
- `qol-cm.log` SHALL contain every recorded action, including `Log cleared.`

### REQ-7 Version and documents

- WHEN the panel is open, the system SHALL show `Version 2.10`. The plugin, both project files, `README.md`, and `docs/USAGE.md` SHALL use that same string, and BepInEx SHALL list `2.10`.
- `CONSTITUTION.md` SHALL say that spec N ships as `2.N` from spec 010 and that specs 005 through 009 keep `1.N`. The push-gate rule SHALL say the same.
- `docs/USAGE.md` SHALL describe the Log tab: what is recorded, the line format, Clear log, and that the file keeps the full history. The README SHALL describe version `2.10` for admins and for players, and SHALL link the wiki. The wiki version history SHALL include `2.10`.

### REQ-8 Cheats alignment

- The Cheats tab SHALL keep the same buttons and the same actions.
- God with Fly, and Creative with Free cam, SHALL be two centered rows. Each of those buttons SHALL be the same size, and the gap between the two buttons SHALL equal the gap between the rows.
- Ghost SHALL be that same size, centered on the row under those pairs.
- Tame and Kill enemies SHALL be that same size, with that same gap, centered so each sits under one column.
