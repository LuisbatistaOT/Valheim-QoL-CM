# Nearby actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Ghost, Tame, and Kill enemies to Global Cheats and publish version 1.5.

**Architecture:** Ghost is a local mode toggle, like God. Tame and Kill enemies are host actions, like spawn and teleport. Core decides who qualifies and which messages to show. The plugin calls the same Valheim methods as the vanilla console commands. The panel stays four modules.

**Tech Stack:** C# / .NET Framework 4.7.2, BepInEx 5, Jötunn, xUnit for Core. Core has no Unity or Valheim types.

## Global Constraints

- Version string is `1.5` in the plugin, the panel, `docs/USAGE.md`, and `README.md`.
- Specs 001 through 004 keep `0.001`, `0.2`, `0.3`, and `0.4`.
- From spec 005 on, spec `N` publishes `1.N` with no extra zero. `1.05` is forbidden because `System.Version` shows it as `1.5`.
- Ghost toggles vanilla ghost mode on the local admin and is not sent to the host.
- Tame runs `Tameable.TameAllInArea` on the host with radius `20`, the argument the vanilla `tame` command passes. That game method tames every loaded non-player with a `Tameable` component. Do not add a second distance filter.
- Kill enemies runs on the host at the requesting admin's position. Radius is `1000`. Damage is `new HitData(1E+10f)`. Skip players, tamed creatures, and any character with a `Piece` component. Those are the vanilla `killenemycreatures` filters. Measure the 1000 from the admin position, not from `Player.m_localPlayer`, because a dedicated server has no local player.
- A dead character rejects all three clicks. A non-admin mutates nothing.
- Closing the panel does not undo ghost mode, taming, or kills.
- The README section title is `Plugin information`.
- No ads, bans, or network services beyond BepInEx, Jötunn, and Valheim.
- Public types get XML docs. New defects go in `specs/005-valheim-qol-cm/issues.md` and cite a requirement.

## Files

- Create: `src/ValheimQoLCM.Core/NearbyActions.cs`
- Modify: `src/ValheimQoLCM.Core/ModeToggles.cs`
- Modify: `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- Modify: `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`
- Modify: `src/ValheimQoLCM/GameplayModifiers.cs`
- Create: `src/ValheimQoLCM/NearbyCommands.cs`
- Modify: `src/ValheimQoLCM/Plugin.cs`
- Modify: `src/ValheimQoLCM/ConsoleView.cs`
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj`
- Modify: `CONSTITUTION.md`
- Modify: `README.md`
- Modify: `docs/USAGE.md`
- Modify: `specs/005-valheim-qol-cm/spec.md` (status only)
- Create: `test/features/nearby-actions.feature`
- Modify: `tasks/active_context.md`
- Modify: `tasks/progress.md`
- Modify: `human-issues.md`

---

### Task 1: Core rules for Ghost, Tame, and Kill enemies

**Files:**
- Create: `src/ValheimQoLCM.Core/NearbyActions.cs`
- Modify: `src/ValheimQoLCM.Core/ModeToggles.cs`
- Test: `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`

**Interfaces:**
- Consumes: `ActionResult<T>`, `PlayerMode`, `ModeToggles.Set`
- Produces:
  - `PlayerMode.Ghost`
  - `ModeToggles.Ghost`
  - `NearbyActions.TameRadius` = `20f`
  - `NearbyActions.KillRadius` = `1000f`
  - `NearbyActions.Begin(bool isAdmin, bool characterIsDead)` returns `ActionResult<int>`
  - `NearbyActions.IsTameTarget(bool isPlayer, bool hasTameable)` returns `bool`
  - `NearbyActions.IsKillTarget(bool isPlayer, bool hasPiece, bool isTamed, float distance)` returns `bool`
  - `NearbyActions.TameMessage(int count)` and `NearbyActions.KillMessage(int count)` return `string`

- [ ] **Step 1: Write the failing tests**

Append these facts to `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`:

```csharp
[Fact]
public void Ghost_toggle_leaves_the_other_modes_alone()
{
    var modes = new ModeToggles();

    Assert.True(modes.Set(PlayerMode.Ghost, true).Ok);
    Assert.True(modes.Set(PlayerMode.God, false).Ok);

    Assert.True(modes.Ghost);
    Assert.False(modes.God);
    Assert.False(modes.Fly);
    Assert.False(modes.Creative);
    Assert.False(modes.FreeCam);
}

[Fact]
public void Dead_character_rejects_ghost()
{
    var modes = new ModeToggles { CharacterIsDead = true };

    var result = modes.Set(PlayerMode.Ghost, true);

    Assert.False(result.Ok);
    Assert.False(modes.Ghost);
    Assert.Equal("Character is dead.", result.Error);
}

[Fact]
public void Nearby_action_rejects_a_non_admin_and_a_dead_character()
{
    var stranger = NearbyActions.Begin(false, false);
    var dead = NearbyActions.Begin(true, true);

    Assert.False(stranger.Ok);
    Assert.Equal("Admins only.", stranger.Error);
    Assert.False(dead.Ok);
    Assert.Equal("Character is dead.", dead.Error);
    Assert.True(NearbyActions.Begin(true, false).Ok);
}

[Fact]
public void Tame_selects_a_tameable_creature_that_is_not_a_player()
{
    Assert.True(NearbyActions.IsTameTarget(isPlayer: false, hasTameable: true));
    Assert.False(NearbyActions.IsTameTarget(isPlayer: true, hasTameable: true));
    Assert.False(NearbyActions.IsTameTarget(isPlayer: false, hasTameable: false));
}

[Fact]
public void Kill_selects_an_untamed_enemy_inside_1000()
{
    Assert.Equal(20f, NearbyActions.TameRadius);
    Assert.Equal(1000f, NearbyActions.KillRadius);
    Assert.True(NearbyActions.IsKillTarget(false, false, false, 1000f));
    Assert.False(NearbyActions.IsKillTarget(true, false, false, 1f));
    Assert.False(NearbyActions.IsKillTarget(false, true, false, 1f));
    Assert.False(NearbyActions.IsKillTarget(false, false, true, 1f));
    Assert.False(NearbyActions.IsKillTarget(false, false, false, 1000.1f));
}

[Fact]
public void Nearby_messages_name_the_count_or_the_empty_area()
{
    Assert.Equal("Tamed 2.", NearbyActions.TameMessage(2));
    Assert.Equal("No tameable animal was nearby.", NearbyActions.TameMessage(0));
    Assert.Equal("Killed 4.", NearbyActions.KillMessage(4));
    Assert.Equal("No enemy was nearby.", NearbyActions.KillMessage(0));
}
```

- [ ] **Step 2: Run the tests and confirm they fail**

Run: `dotnet test tests/ValheimQoLCM.Core.Tests/ValheimQoLCM.Core.Tests.csproj --filter "FullyQualifiedName~Ghost_toggle|FullyQualifiedName~Dead_character_rejects_ghost|FullyQualifiedName~Nearby_"`

Expected: FAIL. `PlayerMode.Ghost` and `NearbyActions` are missing.

- [ ] **Step 3: Add Ghost to the mode tracker**

In `src/ValheimQoLCM.Core/ModeToggles.cs`, add the enum member after `FreeCam`:

```csharp
/// <summary>Vanilla ghost mode.</summary>
Ghost
```

Add the property after `FreeCam`:

```csharp
/// <summary>Ghost mode flag.</summary>
public bool Ghost { get; private set; }
```

Add this case before `default` in `Set`:

```csharp
case PlayerMode.Ghost:
    Ghost = enabled;
    break;
```

Update the class summary to mention ghost.

- [ ] **Step 4: Add the nearby-action rules**

Create `src/ValheimQoLCM.Core/NearbyActions.cs`:

```csharp
namespace ValheimQoLCM.Core;

/// <summary>Who a tame or kill-enemies click affects, and what the console says.</summary>
public static class NearbyActions
{
    /// <summary>Radius argument passed to the vanilla tame command. The game method does not apply a second filter.</summary>
    public const float TameRadius = 20f;

    /// <summary>Distance used by the vanilla kill-nearby-enemies command.</summary>
    public const float KillRadius = 1000f;

    /// <summary>Rejects a click from a non-admin or a dead character.</summary>
    public static ActionResult<int> Begin(bool isAdmin, bool characterIsDead)
    {
        if (!isAdmin)
        {
            return ActionResult<int>.Fail("Admins only.");
        }

        if (characterIsDead)
        {
            return ActionResult<int>.Fail("Character is dead.");
        }

        return ActionResult<int>.Success(0);
    }

    /// <summary>True for a loaded non-player that can be tamed.</summary>
    public static bool IsTameTarget(bool isPlayer, bool hasTameable)
    {
        return !isPlayer && hasTameable;
    }

    /// <summary>True for an untamed non-player inside the kill radius that is not a building piece.</summary>
    public static bool IsKillTarget(bool isPlayer, bool hasPiece, bool isTamed, float distance)
    {
        return !isPlayer && !hasPiece && !isTamed && distance <= KillRadius;
    }

    /// <summary>Console line for a tame click.</summary>
    public static string TameMessage(int count)
    {
        return count == 0 ? "No tameable animal was nearby." : "Tamed " + count + ".";
    }

    /// <summary>Console line for a kill-enemies click.</summary>
    public static string KillMessage(int count)
    {
        return count == 0 ? "No enemy was nearby." : "Killed " + count + ".";
    }
}
```

- [ ] **Step 5: Run the Core tests**

Run: `dotnet test tests/ValheimQoLCM.Core.Tests/ValheimQoLCM.Core.Tests.csproj`

Expected: PASS, including the previous 16 tests.

- [ ] **Step 6: Commit**

```powershell
git add src/ValheimQoLCM.Core/NearbyActions.cs src/ValheimQoLCM.Core/ModeToggles.cs tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs
git commit -m "Add Core rules for ghost, tame, and kill enemies."
```

### Task 2: Apply Ghost locally and Tame and Kill enemies on the host

**Files:**
- Modify: `src/ValheimQoLCM/GameplayModifiers.cs`
- Create: `src/ValheimQoLCM/NearbyCommands.cs`
- Modify: `src/ValheimQoLCM/Plugin.cs`

**Interfaces:**
- Consumes: `PlayerMode.Ghost`, `NearbyActions.Begin`, `IsTameTarget`, `IsKillTarget`, `TameMessage`, `KillMessage`, `TameRadius`, `Plugin.Send`, `Plugin.Reply`
- Produces:
  - `GameplayModifiers.IsEnabled(PlayerMode.Ghost)` and `Toggle(PlayerMode.Ghost)`
  - `NearbyCommands.RequestTame()` and `RequestKillEnemies()` return `ActionResult<int>`
  - `NearbyCommands.ApplyTame(long sender)` and `ApplyKillEnemies(long sender)`
  - Plugin action names `"tame"` and `"kill-enemies"`

- [ ] **Step 1: Teach the local mode switch about Ghost**

In `src/ValheimQoLCM/GameplayModifiers.cs`, add to `IsEnabled`:

```csharp
case PlayerMode.Ghost:
    return player != null && player.InGhostMode();
```

Add to `Apply`:

```csharp
case PlayerMode.Ghost:
    player.SetGhostMode(enabled);
    break;
```

Update the class summary so it names ghost. `Toggle` already rejects a dead character through `ModeToggles` and calls `Apply`. No other mode flag changes.

- [ ] **Step 2: Add the host commands**

Create `src/ValheimQoLCM/NearbyCommands.cs`:

```csharp
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Tame and kill-enemies. The host applies them at the admin's position.</summary>
public static class NearbyCommands
{
    /// <summary>Sends a tame request. A dead local character is rejected here.</summary>
    public static ActionResult<int> RequestTame()
    {
        return Request("tame");
    }

    /// <summary>Sends a kill-enemies request. A dead local character is rejected here.</summary>
    public static ActionResult<int> RequestKillEnemies()
    {
        return Request("kill-enemies");
    }

    /// <summary>Host-side tame. Uses the vanilla tame call.</summary>
    public static void ApplyTame(long sender)
    {
        if (!TryAdmin(sender, out var position))
        {
            return;
        }

        var count = 0;
        var characters = Character.GetAllCharacters();
        if (characters != null)
        {
            foreach (var character in characters)
            {
                if (character == null)
                {
                    continue;
                }

                var tameable = character.GetComponent<Tameable>() != null;
                if (NearbyActions.IsTameTarget(character.IsPlayer(), tameable))
                {
                    count++;
                }
            }
        }

        if (count > 0)
        {
            Tameable.TameAllInArea(position, NearbyActions.TameRadius);
        }

        Plugin.Reply(sender, NearbyActions.TameMessage(count));
    }

    /// <summary>Host-side kill. Same filters and hit as killenemycreatures, measured from the admin.</summary>
    public static void ApplyKillEnemies(long sender)
    {
        if (!TryAdmin(sender, out var position))
        {
            return;
        }

        var count = 0;
        var characters = Character.GetAllCharacters();
        if (characters != null)
        {
            foreach (var character in characters)
            {
                if (character == null)
                {
                    continue;
                }

                var distance = Vector3.Distance(position, character.transform.position);
                var hasPiece = character.GetComponent<Piece>() != null;
                if (!NearbyActions.IsKillTarget(character.IsPlayer(), hasPiece, character.IsTamed(), distance))
                {
                    continue;
                }

                character.Damage(new HitData(1E+10f));
                count++;
            }
        }

        Plugin.Reply(sender, NearbyActions.KillMessage(count));
    }

    private static ActionResult<int> Request(string action)
    {
        var player = Player.m_localPlayer;
        var gate = NearbyActions.Begin(Plugin.LocalIsAdmin(), player == null || player.IsDead());
        if (!gate.Ok)
        {
            return gate;
        }

        Plugin.Send(action, package => { });
        return gate;
    }

    private static bool TryAdmin(long sender, out Vector3 position)
    {
        var name = SenderName(sender);
        if (name == null || !TryPosition(name, out position))
        {
            Plugin.Reply(sender, "Admin player was not found.");
            return false;
        }

        var player = FindPlayer(name);
        if (player != null && player.IsDead())
        {
            Plugin.Reply(sender, "Character is dead.");
            return false;
        }

        return true;
    }

    private static Player? FindPlayer(string playerName)
    {
        var players = Player.GetAllPlayers();
        if (players == null)
        {
            return null;
        }

        foreach (var player in players)
        {
            if (player != null && player.GetPlayerName() == playerName)
            {
                return player;
            }
        }

        return null;
    }

    private static string? SenderName(long sender)
    {
        if (sender == 0L)
        {
            return Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerName() : null;
        }

        var peer = ZNet.instance != null ? ZNet.instance.GetPeer(sender) : null;
        return peer != null ? peer.m_playerName : null;
    }

    private static bool TryPosition(string playerName, out Vector3 position)
    {
        var local = Player.m_localPlayer;
        if (local != null && local.GetPlayerName() == playerName)
        {
            position = local.transform.position;
            return true;
        }

        if (ZNet.instance != null)
        {
            foreach (ZNet.PlayerInfo info in ZNet.instance.GetPlayerList())
            {
                if (info.m_name == playerName)
                {
                    position = info.m_position;
                    return true;
                }
            }
        }

        position = Vector3.zero;
        return false;
    }
}
```

- [ ] **Step 3: Route the two actions in the plugin**

In `src/ValheimQoLCM/Plugin.cs`, add constants next to the other action names:

```csharp
private const string Tame = "tame";
private const string KillEnemies = "kill-enemies";
```

In `ApplyServer`, before `default`:

```csharp
case Tame:
    NearbyCommands.ApplyTame(sender);
    break;
case KillEnemies:
    NearbyCommands.ApplyKillEnemies(sender);
    break;
```

Change the version constant and its summary. The summary must say this is V1, spec 005, and the string is `1.5`:

```csharp
/// <summary>Spec 005 version. V1 is confirmed. The string is 1.5 so BepInEx shows the same number.</summary>
public const string Version = "1.5";
```

Update the type summary from `0.4` to `1.5`. Do not change the GUID or the plugin name.

- [ ] **Step 4: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror`

Expected: success. `HitData` has a float constructor and `Player.GetAllPlayers` exists on the installed game. Do not change the Core rules to make the build pass.

- [ ] **Step 5: Commit**

```powershell
git add src/ValheimQoLCM/GameplayModifiers.cs src/ValheimQoLCM/NearbyCommands.cs src/ValheimQoLCM/Plugin.cs
git commit -m "Apply ghost locally and tame and kill enemies on the host."
```

### Task 3: Put the three controls in Global Cheats

**Files:**
- Modify: `src/ValheimQoLCM/ConsoleView.cs`
- Create: `test/features/nearby-actions.feature`

**Interfaces:**
- Consumes: `PlayerMode.Ghost`, `GameplayModifiers.Toggle`, `NearbyCommands.RequestTame`, `NearbyCommands.RequestKillEnemies`, `ShowFailure`
- Produces: a Ghost mode button labeled from the caption `Ghost`, plus `Tame` and `Kill enemies` under that grid

- [ ] **Step 1: Grow the cheat grid and add the two action buttons**

In `BuildCheats`, set the grid element's `preferredHeight` and `minHeight` to `118f` (three rows of 34 plus two gaps of 8). After the Free cam `AddModeButton` call, add:

```csharp
AddModeButton(PlayerMode.Ghost, "Ghost", gridObject.transform);
var actions = AddRow(body, TextAnchor.MiddleCenter);
AddFlexButton(actions, "Tame", () => ShowFailure(NearbyCommands.RequestTame()), 140f);
AddFlexButton(actions, "Kill enemies", () => ShowFailure(NearbyCommands.RequestKillEnemies()), 180f);
```

`AddModeButton` already writes `Ghost: On` or `Ghost: Off` through `RefreshModes`, because the enum name is `Ghost`. Tame and Kill enemies are not added to `_modeLabels`, so they never show On or Off. They are not added to `_playerActions` or `_spawnControls`, so a missing selection does not gray them out.

- [ ] **Step 2: Record the click behavior**

Create `test/features/nearby-actions.feature`:

```gherkin
Feature: Nearby actions

  Spec 005 adds Ghost, Tame, and Kill enemies to Global Cheats. Version 1.5.

  Scenario: Ghost toggles on the admin
    Given the admin is alive
    When the admin clicks Ghost
    Then ghost mode flips on that character
    And God, Fly, Creative, and Free cam stay as they were
    And the button reads Ghost: On or Ghost: Off

  Scenario: Tame and Kill enemies run on the host
    Given the admin is alive
    When the admin clicks Tame
    Then the host tames the creatures the vanilla tame command would tame
    And the console reports the count
    When the admin clicks Kill enemies
    Then the host kills untamed enemies within 1000 of that admin
    And players and tamed creatures stay alive
    And the console reports the count

  Scenario: A dead admin changes nothing
    Given the admin is dead
    When the admin clicks Ghost, Tame, or Kill enemies
    Then the console says Character is dead.
    And ghost mode, creatures, and enemies stay as they were
```

- [ ] **Step 3: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror`

Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/ValheimQoLCM/ConsoleView.cs test/features/nearby-actions.feature
git commit -m "Add Ghost, Tame, and Kill enemies to Global Cheats."
```

### Task 4: Publish 1.5 and rewrite the player-facing README

**Files:**
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj`
- Modify: `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- Modify: `CONSTITUTION.md`
- Modify: `README.md`
- Modify: `docs/USAGE.md`
- Modify: `specs/005-valheim-qol-cm/spec.md`
- Modify: `tasks/active_context.md`
- Modify: `tasks/progress.md`
- Modify: `human-issues.md`

**Interfaces:**
- Consumes: `Plugin.Version` already set to `1.5` in Task 2. The panel reads that constant.
- Produces: the same `1.5` string in the assemblies, constitution, README, and usage doc

- [ ] **Step 1: Set the assembly versions**

In both csproj files, set:

```xml
<Version>1.5.0</Version>
<InformationalVersion>1.5</InformationalVersion>
```

In `src/ValheimQoLCM/ValheimQoLCM.csproj` also set:

```xml
<AssemblyVersion>1.5.0.0</AssemblyVersion>
<FileVersion>1.5.0.0</FileVersion>
```

- [ ] **Step 2: Record V1 in the constitution**

Replace the opening version paragraph with:

```markdown
Version rule: spec `N` ships as `1.N` now that V1 is confirmed. Do not pad the spec number with extra zeros. BepInEx reads the plugin version with `System.Version`, which turns `1.05` into `1.5`. Specs 001 through 004 keep the strings they already shipped: `0.001`, `0.2`, `0.3`, and `0.4`.
```

Replace the version bullet under Immutable architectural invariants with:

```markdown
- IF a spec number is `N` and that spec was frozen before V1, THEN the system SHALL keep the version string that spec already published.
- IF a spec number is `N` and that spec is 005 or later, THEN the system SHALL publish the version string `1.N` in the plugin, the in-game panel, the usage document, and the Git tag.
```

- [ ] **Step 3: Rewrite the README opening and the install section**

Keep the changelog entries for 0.4, 0.3, 0.2, and 0.1. Replace the title block and the install section so the page starts like this:

```markdown
# Valheim QoL CM

A clickable admin panel for Valheim. Press backtick, then click. No console commands.

**Version 1.5** is V1. It adds Ghost, Tame, and Kill enemies. Earlier specs stay at 0.4, 0.3, 0.2, and 0.1.

## Plugin information

You need Valheim, BepInEx 5.4.23.5, and Jötunn 2.30.1. This plugin does not download them.

Copy these two files into `Valheim\BepInEx\plugins\ValheimQoLCM\`:

- `ValheimQoLCM.dll`
- `ValheimQoLCM.Core.dll`

A dedicated server uses the same two files in its own `BepInEx\plugins\ValheimQoLCM\` folder. Restart the game or the server after copying.

On a local world, the host is the first admin. On a server, an admin is a Steam ID already listed in `adminlist.txt`. The plugin does not promote anyone by itself, and it does not ban anyone.

Join as an admin and press **`** (the key left of `1`). Press it again to close the panel. Closing only hides the panel. Esc closes it too. Modes, tamed animals, killed enemies, spawned items, granted admins, the skill-loss percent, and the action log stay as you left them. A player who is not an admin gets no panel.

The header reads **Valheim QoL - CM** and **Version 1.5**. **By Alfamud** at the bottom right opens this repository.

- **Player Management.** Connected players, then bring-me, bring-player, and grant admin. Those three stay gray until another player is selected.
- **Global Cheats.** God, Fly, Creative, Free cam, and Ghost. Each one toggles your own character. Ghost makes enemies ignore you. Tame and Kill enemies sit under that grid. Tame tames the animals the vanilla tame command would tame. Kill enemies removes hostile creatures within 1000 of you. Players and tamed animals stay. Both buttons tell you how many they affected.
- **Item Spawner.** Search items you can pick up, set quantity and quality, then spawn. Choosing an item sets the quantity to 1.
- **Server skill loss.** Percent of each skill removed on the next death for every player. 0 removes none. 100 clears skills. Gear still drops.
- **Console.** Action messages sit along the bottom and stay there after you close the panel.

Full placement notes are in [docs/USAGE.md](docs/USAGE.md).
```

Keep the `Working on the project` section. Say the current release is 1.5, spec 005, and that specs 001 through 004 stay at their published versions. Add this changelog section above `### 0.4`:

```markdown
### 1.5

Spec 005. V1. Global Cheats gains Ghost, Tame, and Kill enemies.

- Ghost toggles vanilla ghost mode on your character. The other modes stay as they were.
- Tame asks the world host to tame the creatures the vanilla tame command would tame, and the console reports how many.
- Kill enemies asks the host to kill untamed enemies within 1000 of you. Players and tamed creatures stay. The console reports how many.
- A dead character cannot use the three new controls.
- The version string is `1.5`.
```

The heading `Players who only want the plugin` must not remain.

- [ ] **Step 4: Update the usage doc**

In `docs/USAGE.md`, set the title to `Valheim QoL CM 1.5` and remove "This is not V1." State that Ghost, Tame, and Kill enemies live in Global Cheats. Ghost toggles vanilla ghost mode. Tame and Kill enemies are host actions. Kill distance is 1000. A dead character is rejected. The header reads `Version 1.5`. BepInEx lists `1.5`. Keep the install paths, BepInEx 5.4.23.5, and Jötunn 2.30.1.

- [ ] **Step 5: Mark the spec frozen and update the tracker**

In `specs/005-valheim-qol-cm/spec.md`, set `Status: frozen` and add `Frozen: 2026-10-03`.

In `tasks/active_context.md` and `tasks/progress.md`, record spec 005 at version `1.5` as the current release and that V1 is confirmed. Do not write a local path, a machine name, or an email address.

Append a short entry to `human-issues.md`: V1 was confirmed, spec 004 was already `0.4`, so this release is spec 005 at `1.5` because `1.04` would display as `1.4`.

- [ ] **Step 6: Search the files that will be pushed**

Search the diff for a user-profile path, a drive-letter install path, a machine name, a local IP, and an email address. The credit `Alfamud` may remain. `https://github.com/LuisbatistaOT/Valheim-QoL-CM` may remain. Any other hit fails the task. Fix it before committing.

- [ ] **Step 7: Commit**

```powershell
git add src/ValheimQoLCM/ValheimQoLCM.csproj src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj CONSTITUTION.md README.md docs/USAGE.md specs/005-valheim-qol-cm/spec.md tasks/active_context.md tasks/progress.md human-issues.md
git commit -m "Publish version 1.5 and describe Ghost, Tame, and Kill enemies."
```

### Task 5: Prove the suite

**Files:**
- Test: `ValheimQoLCM.sln`

- [ ] **Step 1: Run the gate**

From the repository root:

```powershell
dotnet build ValheimQoLCM.sln -warnaserror
dotnet test ValheimQoLCM.sln
```

Expected: the build has no warnings, and every test passes.

- [ ] **Step 2: Stop**

Do not copy DLLs into a Valheim install unless the maintainer asks. Do not push.

The in-game check, when the maintainer runs it: open the panel as an admin, confirm `Version 1.5`, toggle Ghost, click Tame near a boar, click Kill enemies near a greyling, and confirm a player and a tamed boar stay. Repeat the three clicks while dead and confirm the console says `Character is dead.`
