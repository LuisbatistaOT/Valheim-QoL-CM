# Human issues

## 2026-10-02 — Spec 001 frozen

- Change: Replaced the typed-console draft with the frozen clickable-panel spec, version 0.001.
- Why: Admins need click controls, a live skill-loss percent, and a written trail from defects back to the spec.
- Spec: `specs/001-valheim-qol-cm/spec.md`
- Issues: none yet. New defects go in `specs/001-valheim-qol-cm/issues.md` and in this file.

## 2026-10-02 — ISS-001

- Spec: REQ-5 and REQ-8.
- What happened: the first Core build failed because `record` types need `IsExternalInit`, which netstandard2.0 does not ship.
- Resolution: action and spawn results are classes with constructors. Unit tests assert the same accept and reject outcomes.

## 2026-10-02 — ISS-002

- Spec: REQ-1, REQ-3, REQ-4, REQ-5, REQ-6, and REQ-7.
- What happened: `dotnet test` failed to compile because the test class used `[Fact]` without importing xUnit.
- Resolution: the test file imports `Xunit`.

## 2026-10-02 — ISS-003

- Spec: REQ-6.
- What happened: a unit test expected 0 percent skill loss to reduce a level of 40 to 0. The implementation left the level at 40, which matches the spec.
- Resolution: the test now expects the original level at 0 percent and an empty level at 100 percent.

## 2026-10-02 — ISS-004

- Spec: REQ-2.
- What happened: after the DLL loaded in Valheim, BepInEx printed version 0.1. The plugin's own log line printed 0.001. BepInEx parses the attribute with `System.Version`, which drops the extra zero.
- Resolution: the panel footer, usage doc, and release tag stay `0.001`. The chainloader line is the library's formatting.

## 2026-10-02 — ISS-005

- Spec: REQ-2.
- What happened: the local git tag `0.001` was created. GitHub CLI has no login, so the GitHub release was not published.
- Resolution: published as GitHub release 0.001 at https://github.com/LuisbatistaOT/Valheim-QoL-CM/releases/tag/0.001.

## 2026-10-02 — ISS-006

- Spec: REQ-1.
- What happened: the panel did not appear while the game was running. The saved hotkey was still Left Ctrl+Tab.
- Resolution: backtick opens the panel and backtick closes it. Closing hides the panel and leaves god, fly, creative, free cam, items, admins, and the skill-loss percent unchanged.

## 2026-10-02 — ISS-007

- Spec: REQ-5.
- What happened: the item filter stays open, and the match list draws underneath quantity and quality.
- Resolution: deferred to spec 002. Spec 001 stays frozen. The next spec puts the filter and a match table on the left, under the connected players, with the filter above the table.

## 2026-10-02 — ISS-008

- Spec: REQ-4.
- What happened: a connected player can be clicked, but the list does not show which row is selected.
- Resolution: deferred to spec 002. The next spec uses a clickable player table and highlights the selected row.

## 2026-10-02 — ISS-009

- Spec: REQ-8.
- What happened: action results use one status line near the top.
- Resolution: deferred to spec 002. The next spec shows that log as a console along the bottom of the panel.

## 2026-10-02 — spec 002

- Spec: REQ-1 through REQ-4 in `specs/002-valheim-qol-cm/spec.md`.
- What happened: the panel layout moved to a left player table, an item table under the filter, a highlighted selection, and a bottom action console. Version is 0.002.
- Resolution: 14 unit tests passed. No layout defect. The DLLs are in `BepInEx\plugins\ValheimQoLCM`. Restart Valheim to load them.

## 2026-10-02 — spec 002 version string

- Spec: REQ-4 in `specs/002-valheim-qol-cm/spec.md`. Issue ISS-001.
- What happened: `0.002` is not a stable BepInEx version. `System.Version` drops the extra zero and shows `0.2`.
- Resolution: the plugin, panel, usage doc, README, and git tag use `0.2`. The constitution now says spec `N` ships as `0.N`.

## 2026-10-03 — lessons file

- The Obsidian `lessons-learned.md` for this plugin is not in the vault yet, so that copy of the spec 001 cycle has not been closed.

## 2026-10-03 — spec 003 frozen, spec 004

- Spec: `specs/004-valheim-qol-cm/spec.md`. Spec 003 stays frozen. ISS-003 and ISS-004 from spec 003 move here.
- What happened: clicking an item filled the quantity to the stack size, the console sometimes named a different item than the one that spawned, and a bow search listed copies that could not be picked up.
- Resolution: quantity returns to 1 on select. The list keeps the prefab Spawn creates when `m_icons` has at least one entry. Shared names show the prefab, and the console repeats the row name. Version is 0.4.

## 2026-10-03 — spec 005 version string

- Spec: REQ-5 in `specs/005-valheim-qol-cm/spec.md`.
- What happened: V1 was confirmed. Spec 004 was already published as `0.4`, so the new actions could not reuse that number. The string `1.04` would display as `1.4` because `System.Version` drops the extra zero.
- Resolution: this release is spec 005 at `1.5` in the plugin, the panel, the usage doc, and the README. Specs 001 through 004 keep their published strings.

## 2026-10-07 — spec 006

- Spec: REQ-1, REQ-2, and REQ-3 in `specs/006-valheim-qol-cm/spec.md`. Issue ISS-001.
- What happened: Kill enemies could finish without a useful report. Skill loss of 0% returned to 5% after relog because Jötunn resets the admin config entry to its default when a client joins. The panel hotkey was backtick, which clashes with another plugin.
- Resolution: the host stores the applied percent and sends it to clients. Kill enemies writes a debug line with position, radius, seen, and killed. The panel opens with numpad + or Shift and the =/+ key, because `KeyCode.Plus` never arrives from the keyboard. Version is `1.6`.

## 2026-10-07 — spec 006 frozen

- Spec: `specs/006-valheim-qol-cm/spec.md`. Issues ISS-001 and ISS-002.
- What happened: local play confirmed the panel opens and closes with numpad + and with Shift and the =/+ key. The hosted-server checks for Kill enemies and for 0% skill loss after relog are still open.
- Resolution: spec 006 is frozen at `1.6`. Spec 007 has not started. Version 1.5 remains the last GitHub release until 1.6 is published.
