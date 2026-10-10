# World tab

**Goal:** Put the panel on five tabs and let an admin set the five Valheim world modifiers from stepped sliders or presets, with a skill-loss overwrite that keeps skill levels on death.

**Architecture:** Core owns the stop lists, the preset table, the key bundles per step, parsing the world's global keys back into steps, and the slider placement rules (`StopIndex`, `StopFraction`, `HandleNeedsPlace`). `WorldModifierHost` writes keys with the private `GlobalKeyAdd` and `GlobalKeyRemove`, then `UpdateWorldRates` and `SendGlobalKeys`. `DeathPenaltyManager` zeroes the death factor while the overwrite is on. `ConsoleView` is a view over those services and `WorldModifierDraft`.

**Tech Stack:** C# / .NET Framework 4.7.2, BepInEx 5, Jötunn, xUnit for Core. Core has no Unity or Valheim types. Valheim also has a type named `WorldModifiers`; plugin code calls the Core type through the alias `ModifierRules`.

## Global Constraints

- Version string is `1.8` in the plugin, both project files, the panel, `docs/USAGE.md`, and `README.md`.
- Specs 001 through 007 keep the strings they already shipped.
- No ads, bans, or network services beyond BepInEx, Jötunn, and Valheim.
- New defects go in `specs/008-valheim-qol-cm/issues.md` and cite a requirement.

## Files

- Create: `src/ValheimQoLCM.Core/WorldModifiers.cs`
- Create: `src/ValheimQoLCM.Core/WorldModifierDraft.cs`
- Create: `src/ValheimQoLCM.Core/SkillOverwrite.cs`
- Modify: `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`
- Create: `src/ValheimQoLCM/WorldModifierHost.cs`
- Modify: `src/ValheimQoLCM/Plugin.cs`
- Modify: `src/ValheimQoLCM/ConsoleManager.cs`
- Modify: `src/ValheimQoLCM/ConsoleView.cs`
- Modify: `src/ValheimQoLCM/DeathPenaltyManager.cs`
- Modify: `src/ValheimQoLCM/PluginStorage.cs`
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj`
- Modify: `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- Modify: `README.md`
- Modify: `docs/USAGE.md`
- Modify: `AGENTS.md`
- Modify: `.cursor/rules/master-push-e2e.mdc`
- Create: `specs/007-valheim-qol-cm/README.md`
- Create: `specs/008-valheim-qol-cm/spec.md`
- Create: `specs/008-valheim-qol-cm/plan.md`
- Create: `specs/008-valheim-qol-cm/tasks.md`
- Modify: `specs/008-valheim-qol-cm/issues.md`
- Modify: `specs/008-valheim-qol-cm/status.md`
- Modify: `specs/backlog.md`
- Modify: `tasks/active_context.md`
- Modify: `tasks/progress.md`
- Modify: `human-issues.md`
- Modify: Obsidian `QoL CM/lessons-learned.md`

## Notes

The working tree was emptied on 2026-10-09. `src/` is a decompile of the installed 1.8 plugin placed on top of published `master`. It carries IL comments. Core has XML docs on every public member. The plugin project runs `Nullable` as `annotations` so the decompiled `= null` initializers do not fail `-warnaserror`; rewriting that source is backlog DEF-005.

Jötunn raises `OnCustomGUIAvailable` once in the menu scene and again in the world scene. The menu panel is destroyed by the scene change before the second build. `ConsoleView.Build` must clear every row list on each call, not only when the old root still exists.
