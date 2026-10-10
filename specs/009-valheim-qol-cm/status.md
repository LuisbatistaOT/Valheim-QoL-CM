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

## Gate

Not run yet.
