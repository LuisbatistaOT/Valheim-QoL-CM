# Spec 009 — Pick filter

- Version: 1.9
- Status: draft
- Drafted: 2026-10-10
- Cycle: fourth Version 2 spec. Follows spec 008 (`1.8`, World tab).
- Inherits: gameplay rules in `specs/001-valheim-qol-cm/spec.md` stay frozen. Specs 002 through 008 stay where they are. This spec adds the Pick tab, opens the panel to every player for that tab alone, and changes nothing on the five admin tabs.
- Backlog review: DEF-002 (hosted-server play), DEF-003 (old percent file), DEF-004 (release PDBs), and DEF-005 (decompiled plugin source) are outside the pick-filter area and stay on the backlog.

## Gap

Mining, farming, and wood cutting drop more than the resource the player came for: feathers, resin, sap, eggs, mushrooms. Vanilla auto-pickup takes everything that fits by slot and weight, so the player opens the inventory and drops the rest. The Pick tab lets the player say what auto-pickup may take.

## Requirements

### REQ-1 Access

- Every player with the plugin SHALL open the panel with the spec 006 hotkey. A player who is not an admin SHALL see one tab, Pick, and the status line. The admin tabs SHALL NOT be built for that player.
- An admin SHALL see six tabs: Cheats, World, Spawn, Players, Pick, and Log. The panel still opens on Cheats for an admin.
- WHEN the admin flag changes while the panel is open, the tab row SHALL rebuild to match it. The panel SHALL NOT close for a non-admin.
- The pick filter changes only which drops the local player's auto-pickup takes. It SHALL NOT add network traffic beyond vanilla: a skipped drop still receives the ownership request vanilla sends for every nearby drop, and nothing else is sent to the host or to other players. It is not a gameplay mutation under the Constitution.

### REQ-2 Filter behavior

- WHILE the filter is on, auto-pickup SHALL skip a drop whose prefab is not in the applied list, and SHALL NOT pull that drop toward the player.
- Pressing E on a drop SHALL pick it up regardless of the filter.
- WHILE the filter is off (Pick all), auto-pickup SHALL behave as vanilla.
- The filter SHALL be stored beside the plugin in `qol-cm-pick-filter.txt` and SHALL be loaded when the plugin starts. The same filter applies to every character and world on this machine, and it survives relog and joining another world.
- A prefab in the file or in a preset that the loaded game does not have SHALL be ignored, with one `qol-cm-debug.log` line naming it. A missing or malformed file reads as Pick all with no Custom list.

### REQ-3 Presets

- Preset buttons SHALL be Woodcutting, Mining, Farming, Custom, and Pick all.
- Woodcutting SHALL stage `Wood, FineWood, RoundLog, ElderBark, YggdrasilWood, Blackwood`.
- Mining SHALL stage `Stone, CopperOre, TinOre, CopperScrap, IronScrap, SilverOre, BlackMetalScrap, FlametalOre, FlametalOreNew, Obsidian, Chitin, BlackMarble, Softtissue, Grausten`.
- Farming SHALL stage `Carrot, CarrotSeeds, Turnip, TurnipSeeds, Onion, OnionSeeds, Barley, Flax, JotunPuffs, Magecap, Fiddlehead, Vineberry, SmokePuff`.
- The three lists SHALL NOT share a prefab. Names that the game does not have are dropped under REQ-2 and the preset still lights when the rest match.
- Pick all SHALL stage an empty list with the filter off.
- Custom SHALL stage the saved Custom list. If no Custom list is saved, the status line SHALL say so and the staged list SHALL NOT change.
- WHEN a preset is clicked, the table SHALL show that list and Apply SHALL light if the staged list differs from the applied filter.

### REQ-4 Lit preset

- Exactly one preset button SHALL be lit. It is the staged list's match: a preset whose prefabs, after the REQ-2 drop of names the game lacks, equal the staged list; Pick all when the filter is staged off; Custom for any other list.
- WHEN the Pick tab opens, the staged list SHALL be a copy of the applied filter, so the lit button reflects the filter in force, including after relog or joining a new world. Vanilla behavior lights Pick all.
- WHEN an item is removed from or added to the table, the lit button SHALL move to Custom unless the result equals a preset.

### REQ-5 Table and search

- The table SHALL list the staged items, one row per item, sorted by label, showing the translated name as the Spawn tab does. Each row SHALL have a remove control. The header SHALL show the lit preset name and the item count.
- A search field SHALL sit above the table. WHILE it holds text, a dropdown SHALL overlay the table with up to 8 catalog items whose label, prefab, or token contains the text and which are not already staged. Clicking one adds it, clears the field, and hides the dropdown. Clearing the field or pressing Esc hides the dropdown without closing the panel.
- The catalog is the Spawn tab's item catalog: prefabs with at least one icon, labeled by translated name, with the prefab appended when two items share a name.

### REQ-6 Apply

- The button SHALL read `Apply pick filter`. It SHALL stay gray while the staged list equals the applied filter.
- IF the staged list is empty and the filter is staged on, THEN Apply SHALL stay gray and the status line SHALL say to add an item or choose Pick all. An active filter never has zero items.
- WHEN Apply is clicked with a list that equals no preset, the system SHALL save that list as Custom and apply it. Applying a preset or Pick all SHALL keep the saved Custom list.
- WHEN Apply is clicked, the file SHALL be written and auto-pickup SHALL follow the new filter at once.
- Leaving the Pick tab or closing the panel SHALL discard the staged copy.

### REQ-7 Logs

- `qol-cm.log` SHALL record each Apply as `Pick filter: <lit preset>, <count> items.` (`1 item` for one) or `Pick filter: Pick all.` The preset name is the one lit under REQ-4, so a saved Custom list logs as `Custom`.
- `qol-cm-debug.log` SHALL record, at plugin start, the loaded filter as `Pick filter loaded: <preset>, <count> items, custom <count> items.` (`1 item` for one; `custom none` when no Custom list is saved; Pick all reads `Pick all, 0 items`), and each dropped prefab under REQ-2. WHEN the Pick tab opens it SHALL record `Pick readout <lit preset>, rows <table row count>, staged <staged count>.` The two counts MUST be equal.

### REQ-8 Version and documents

- WHEN the panel is open, the system SHALL show `Version 1.9`. The plugin, both project files, `README.md`, and `docs/USAGE.md` SHALL use that same string.
- `CONSTITUTION.md` SHALL say that a non-admin does not see the admin tabs and does not apply gameplay mutations, and that the pick filter is local to the player's own auto-pickup.
- The usage document SHALL have a Pick section written for players, not only admins. The README changelog SHALL include `1.9`.
