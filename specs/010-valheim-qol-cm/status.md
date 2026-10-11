# Spec 010 status

Drafted 2026-10-10. Frozen 2026-10-11 at version **2.10** on branch `spec-010-log-tab`, then merged into `master`.

The spec is `spec.md`. The plan is `plan.md`. Defects are in `issues.md`. This file is the history of the cycle.

## What the Log tab does

The Log tab is a console for the admin: every action taken from the panel, stamped with the local time, in white monospace text on black. A Clear log button empties the panel; `qol-cm.log` on disk keeps the whole history.

Cheats buttons share one size and one gap. God with Fly, and Creative with Free cam, are centered pairs. Ghost is centered under them. Tame and Kill enemies sit under the two columns.

## Decisions made while drafting

- Version `2.10`. The Constitution was amended first (commit `6c4d00d`): spec N ships as `2.N` from spec 010; specs 005 through 009 keep `1.N`.
- Record effects only: mode toggles, Tame, Kill enemies, the two teleports, Spawn, Grant admin, World Apply, Apply pick filter, Clear log, and their failures and host replies. Selections, validation feedback, confirmation prompts, and the Pick status line stay transient.
- One sink. `ConsoleManager.Record` writes the file and the Core `ActionLog` buffer; `SetStatus` keeps the transient text and no longer feeds the log. The panel and the file hold the same actions.
- `[HH:mm:ss]` local time in the panel. The file keeps its own stamp.
- Clear log empties the panel only and records `Log cleared.` so the panel is never blank without a reason.
- Session only: the buffer starts empty on each game start and is not read from disk.
- Monospace from the OS (`Consolas`, then `Courier New`), falling back to the panel font.
- The README is the short page: admins, players, installation, issues, and a link to the wiki. The wiki holds the manual and the version history.

## Play

2026-10-11, local world. The file and the readout count held every action while the box showed one line (ISS-013). After the text height follows the buffer, the console shows each line. Cheats alignment is ISS-014.

## Gate

`dotnet build ValheimQoLCM.sln -warnaserror`: 0 Warning(s), 0 Error(s).

`dotnet test ValheimQoLCM.sln`: Passed 53, Failed 0.

Release: `dotnet build ValheimQoLCM.sln -c Release -warnaserror -p:DebugType=none`. 0 Warning(s), 0 Error(s). `FileVersion` `2.10.0.0`. No PDB in `release/ValheimQoLCM`. DEF-004 is closed.
