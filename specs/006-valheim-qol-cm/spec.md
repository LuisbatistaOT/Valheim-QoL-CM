# Spec 006 — Defects and diagnostic log

- Version: 1.6
- Status: frozen
- Frozen: 2026-10-07
- Cycle: first Version 2 spec. Follows frozen spec 005.
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. Specs 002 through 005 stay where they are. This spec fixes three defects and adds two log files. It does not add buttons.

## Requirements

### REQ-1 Kill enemies

- Kill enemies SHALL stay the spec 005 action: the host applies it, the radius is 1000 from that admin, and players and tamed creatures stay alive.
- WHEN the click kills nothing, the action console SHALL say that no enemy was nearby.
- WHEN the click runs, `qol-cm-debug.log` SHALL record the admin position, the radius, how many characters were seen, and how many were killed.
- A click that changes nothing and writes nothing is a failure.
- The host SHALL measure the radius from the admin's live character when that character is loaded.

### REQ-2 Skill loss

- WHEN an admin applies 0%, logout, and login, the slider, the percent box, and the next death SHALL use 0.
- The bar SHALL NOT return to the default of 5% after that relog.
- 0% SHALL remove no skill level. Gear SHALL still drop.
- The world host SHALL store the applied percent beside the plugin and SHALL send that value to clients.
- A client join SHALL NOT replace a saved 0 with the default 5.

### REQ-3 Panel hotkey

- The + key SHALL open the panel. Numpad + works with one press. On the main keyboard, Shift and the =/+ key work together. `KeyCode.Plus` alone does not, because Valheim never reports that code.
- A saved binding that is still backtick, Ctrl+Tab, or the dead `Plus` code SHALL be rewritten to numpad +.
- A binding the player chose on purpose SHALL stay as it is.
- Backtick SHALL NOT open or close the panel unless someone binds it again.
- After the rewrite, Controls and the BepInEx config screen SHALL still be able to rebind the panel.

### REQ-4 Logs

- `qol-cm.log` SHALL gain one line for each accepted or rejected action, using the same text the panel console shows.
- `qol-cm-debug.log` SHALL record the skill-loss percent loaded at startup, and the kill detail from REQ-1.
- Both files, and the host skill-loss file, SHALL sit beside the plugin DLL. Source SHALL NOT contain a profile path, a drive letter, a machine name, or an email address.
- Those three files SHALL be gitignored.

### REQ-5 Version label

- WHEN the panel is open, the system SHALL show the version string `1.6`.
- The plugin version, the usage document, and the README SHALL use that same string.
- This spec is frozen at `1.6`. The GitHub release stays at 1.5 until 1.6 is published.

### REQ-6 Usage and README

- The usage document SHALL state version `1.6`, the + hotkey, that 0% skill loss survives relog, and what the two logs record.
- The README SHALL name the same three fixes, and its changelog SHALL include `1.6`.
