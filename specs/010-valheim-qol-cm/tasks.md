# Tasks - spec 010

Plan tasks are in `plan.md`. This is the cycle checklist.

- [x] Core: `ActionLog` buffer, cap 40, `[HH:mm:ss]` format, Clear, empty rejection, with tests.
- [x] `ConsoleManager.Record`: file plus buffer plus repaint; every action call site moved to it; `SetStatus` no longer feeds the log.
- [x] Log tab: black box with border, white 14pt monospace, scroll to newest, Clear log button, `Log readout` debug line.
- [x] Version `2.10` in the plugin, both project files, the panel, README, and USAGE; Constitution and gate rule amended.
- [x] Local play: the claims in plan Task 5, including a failure line, Clear log, and the file matching the panel.
- [x] `dotnet build ValheimQoLCM.sln -warnaserror` clean and `dotnet test` green at freeze.
