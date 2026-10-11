# Spec 009 status

Drafted 2026-10-10 for version **1.9** on branch `spec-009-pick-filter`, branched from `master` after spec 008 merged locally.

The spec is `spec.md`. The plan is `plan.md`. Defects are in `issues.md`. This file is the history of the cycle.

## What the Pick tab does

Every player with the plugin opens the panel and sees the Pick tab. Admins see it beside the five admin tabs. The tab filters what the local player's auto-pickup takes. The E key is never filtered.

Presets: Woodcutting, Mining, Farming, Custom, Pick all. One button is lit: the staged list's match. A searchable table lists the staged items with a remove control per row. `Apply pick filter` writes `qol-cm-pick-filter.txt` beside the plugin and takes effect at once.

## Decisions made while drafting

- Filter auto-pickup only. E stays a deliberate override.
- One file beside the plugin, shared by every character and world on this machine.
- Apply refuses an empty list while the filter is on.
- Enforcement: a scope flag around `Player.AutoPickup` and a prefix on `ItemDrop.CanPickup` that returns false for a filtered prefab only inside that scope. The drop is skipped before the magnet pull. A prefix on `Humanoid.Pickup` would still let the item be pulled to the player's feet.

## Build

Nine plan tasks ran as one subagent per task with a review between tasks, then a whole-branch review. Two Important findings came out of the Task 7 review (a stale status line; preset staging and the Apply rule living in the view) and moved to Core as `PickFilter.Stage` and `PickFilter.ApplyEnabled`. The whole-branch review added the Esc frame stamp (Unity's `InputField` restores the empty text in the same frame Esc is read) and moved the dropped-prefab lines to the `ObjectDB.Awake` postfix so they run at start, not on tab open.

## Play check 2026-10-10

Claims 1 through 11 of plan Task 9 passed on the local world: start lines, six tabs at `Version 1.9`, Woodcutting, Mining, Remove to Custom, Apply and the file, auto-pickup skipping a filtered drop while E still takes it, Custom returning after a preset, relog with the applied preset lit, discard on tab change, and the non-admin login seeing Pick alone.

One defect: ISS-012. Typing `Stone` never listed Stone, because the eight matches were taken alphabetically and the exact match fell past the cut-off. `ItemCatalog.Search` now ranks exact, then starts-with, then contains. Confirmed in play after the fix.

## Gate

`dotnet build ValheimQoLCM.sln -warnaserror`: 0 Warning(s), 0 Error(s). `dotnet test ValheimQoLCM.sln`: Passed 47, Failed 0. Security scan of the branch diff against `master`: no profile paths, drive letters, machine names, addresses, or emails; the maintainer name appears only in the repository URL.

Frozen 2026-10-10 at `1.9`.
