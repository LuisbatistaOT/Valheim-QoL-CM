# Task plan — spec 001

Each task cites the frozen spec. Check a task only after its test or review step passes.

## Checkpoint 1: Spec freeze

- [x] **Task 1:** Write constitution invariants (REQ-8, REQ-9).
- [x] **Task 2:** Freeze `spec.md` for version 0.001 (REQ-1 through REQ-10).
- [x] **Task 3:** Write Gherkin scenarios in `test/features/admin-panel.feature`.

## Checkpoint 2: Domain rules

- [x] **Task 4:** Add Core result, clamp, and admin gate types (REQ-1, REQ-6, REQ-8).
- [x] **Task 5:** Add independent mode toggles and the dead-character rejection (REQ-3).
- [x] **Task 6:** Add spawn quantity and quality checks (REQ-5).
- [x] **Task 7:** Add teleport selection and Steam ID checks (REQ-4, REQ-7).
- [x] **Task 8:** Add xUnit tests that match the Gherkin outcomes and run `dotnet test`.

## Checkpoint 3: Plugin shell

- [x] **Task 9:** Create the net472 plugin with Jötunn dependency and version 0.001 (REQ-2, REQ-9).
- [x] **Task 10:** Bind the admin-only skill-loss config and the Ctrl+Tab shortcut (REQ-1, REQ-6).
- [x] **Task 11:** Open and close the wood panel for admins only (REQ-1, REQ-2).

## Checkpoint 4: Gameplay actions

- [x] **Task 12:** Apply mode flags on the local admin character (REQ-3).
- [x] **Task 13:** List connected players and teleport through the host (REQ-4, REQ-9).
- [x] **Task 14:** Spawn the chosen quantity at the selected player's feet (REQ-5).
- [x] **Task 15:** Substitute the synced percent during death skill loss (REQ-6).
- [x] **Task 16:** Grant admin from the connection ID, with a typed fallback (REQ-7).

## Checkpoint 5: Acceptance

- [x] **Task 17:** Write `docs/USAGE.md` for install path, requirements, and controls (REQ-10).
- [x] **Task 18:** Copy the DLL into `BepInEx\plugins` and record the load check (REQ-9).
- [x] **Task 19:** Record any defect in `issues.md` with its spec id before closing it.
