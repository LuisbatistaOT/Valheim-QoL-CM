# Spec 010 status

Drafted 2026-10-10 for version **2.10** on branch `spec-010-log-tab`, branched from `master` after spec 009 merged locally.

The spec is `spec.md`. The plan is `plan.md`. Defects are in `issues.md`. This file is the history of the cycle.

## What the Log tab does

The Log tab is a console for the admin: every action taken from the panel, stamped with the local time, in white monospace text on black. A Clear log button empties the panel; `qol-cm.log` on disk keeps the whole history.

## Decisions made while drafting

- Version `2.10`. The Constitution was amended first (commit `6c4d00d`): spec N ships as `2.N` from spec 010; specs 005 through 009 keep `1.N`.
- Record effects only: mode toggles, Tame, Kill enemies, the two teleports, Spawn, Grant admin, World Apply, Apply pick filter, Clear log, and their failures and host replies. Selections, validation feedback, confirmation prompts, and the Pick status line stay transient.
- One sink. `ConsoleManager.Record` writes the file and the Core `ActionLog` buffer; `SetStatus` keeps the transient text and no longer feeds the log. The panel and the file hold the same actions.
- `[HH:mm:ss]` local time in the panel. The file keeps its own stamp.
- Clear log empties the panel only and records `Log cleared.` so the panel is never blank without a reason.
- Session only: the buffer starts empty on each game start and is not read from disk.
- Monospace from the OS (`Consolas`, then `Courier New`), falling back to the panel font.

## Gate

Not run yet.
