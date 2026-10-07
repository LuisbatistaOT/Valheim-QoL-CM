# Defects and diagnostic log

**Goal:** Fix Kill enemies reporting, keep a saved skill-loss percent of 0 after relog, and move the panel hotkey to +.

**Architecture:** Core decides which hotkeys are legacy, that a saved 0 beats the default 5, and the kill debug line. The plugin writes the host file and the two logs beside its DLL. The panel stays the spec 005 view.

**Tech Stack:** C# / .NET Framework 4.7.2, BepInEx 5, Jötunn, xUnit for Core. Core has no Unity or Valheim types.

## Global Constraints

- Version string is `1.6` in the plugin, the panel, `docs/USAGE.md`, and `README.md`.
- Specs 001 through 005 keep the strings they already shipped.
- No new buttons. No ads, bans, or network services beyond BepInEx, Jötunn, and Valheim.
- Public types get XML docs. New defects go in `specs/006-valheim-qol-cm/issues.md` and cite a requirement.

## Files

- Create: `src/ValheimQoLCM.Core/PanelHotkey.cs`
- Create: `src/ValheimQoLCM.Core/SavedSkillLoss.cs`
- Modify: `src/ValheimQoLCM.Core/NearbyActions.cs`
- Modify: `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`
- Create: `src/ValheimQoLCM/PluginStorage.cs`
- Modify: `src/ValheimQoLCM/Plugin.cs`
- Modify: `src/ValheimQoLCM/ConsoleManager.cs`
- Modify: `src/ValheimQoLCM/ConsoleView.cs`
- Modify: `src/ValheimQoLCM/DeathPenaltyManager.cs`
- Modify: `src/ValheimQoLCM/NearbyCommands.cs`
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj`
- Modify: `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- Modify: `README.md`
- Modify: `docs/USAGE.md`
- Modify: `.gitignore`
- Modify: `AGENTS.md`
- Create: `specs/006-valheim-qol-cm/spec.md`
- Create: `specs/006-valheim-qol-cm/plan.md`
- Create: `specs/006-valheim-qol-cm/tasks.md`
- Create: `specs/006-valheim-qol-cm/issues.md`
- Create: `test/features/defects-and-logs.feature`
- Modify: `tasks/active_context.md`
- Modify: `tasks/progress.md`
- Modify: `human-issues.md`

## Notes

Jötunn resets an admin-only config entry to its default when a client joins, and an admin client can write that default back to the host. The host file `qol-cm-skill.txt` is the stored percent. Gameplay reads that value, not the reset entry. The BepInEx entry stays so the settings screen still shows the percent.
