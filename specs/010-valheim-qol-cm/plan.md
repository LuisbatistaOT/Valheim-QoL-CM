# Log tab Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Log tab a session console of panel actions, `[HH:mm:ss]` local time, white monospace on black, with Clear log, at version `2.10`.

**Architecture:** Core `ActionLog` owns the 40-line buffer and the line format. `ConsoleManager.Record` is the only writer: it stamps local time, appends `qol-cm.log`, and repaints the tab. `SetStatus` stops feeding the log and keeps transient text. The Log tab is a view over `ActionLog`.

**Tech Stack:** C# / netstandard2.0 Core, BepInEx 5 plugin on .NET Framework 4.7.2, Jötunn GUI, xUnit. PowerShell: one command per line, no `&&`.

## Global Constraints

- Version string `2.10` in the plugin, both project files, the panel, `README.md`, and `docs/USAGE.md`. Specs 005 through 009 stay `1.N`. `CONSTITUTION.md` already says this (commit `6c4d00d`); do not edit it again.
- Core has no Unity or Valheim types. Every public member has an XML doc. Tabs, file-scoped namespace, `<Nullable>enable</Nullable>`.
- Plugin code keeps the decompiled style: Unity null checks are `(Object)(object)x != (Object)null` with `using Object = UnityEngine.Object;`.
- The panel is a view. What counts as an action is decided by which call site calls `Record`; the buffer rule lives in Core.
- `-warnaserror`. No ads, bans, or online services. Log stays an admin tab.
- The local user name, drive letters, and profile paths never appear in a committed file.

## File map

- Create: `src/ValheimQoLCM.Core/ActionLog.cs` — buffer, cap, format, clear.
- Create: `tests/ValheimQoLCM.Core.Tests/ActionLogTests.cs` — the Core rules.
- Modify: `src/ValheimQoLCM/ConsoleManager.cs` — `Record`, `ClearLog`; delete `Show`.
- Modify: `src/ValheimQoLCM/ConsoleView.cs` — call sites, `SetStatus`, the Log tab look, Clear log, readout.
- Modify: `src/ValheimQoLCM/Plugin.cs` — `Show` to `Record`, version `2.10`.
- Modify: `src/ValheimQoLCM/PickFilterService.cs` — drop the direct file write; the view records the line.
- Modify: both csproj, `README.md`, `docs/USAGE.md` — version and the Log section.
- `AGENTS.md` already points at this plan.

---

### Task 1: ActionLog

**Files:**
- Create: `src/ValheimQoLCM.Core/ActionLog.cs`
- Test: `tests/ValheimQoLCM.Core.Tests/ActionLogTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `ActionLog.Add(DateTime localTime, string? message) -> bool`, `ActionLog.Clear()`, `ActionLog.Entries` (`IReadOnlyList<string>`, oldest first), `ActionLog.Count`, `ActionLog.Format(DateTime, string) -> string`, `ActionLog.Capacity` (40), `ActionLog.ClearedMessage` (`"Log cleared."`).

- [ ] **Step 1: Write the failing tests**

Create `tests/ValheimQoLCM.Core.Tests/ActionLogTests.cs`. The constructor clears the static buffer so tests do not leak into each other. xUnit runs one class sequentially.

```csharp
using System;
using System.Linq;
using ValheimQoLCM.Core;
using Xunit;

namespace ValheimQoLCM.Core.Tests;

public class ActionLogTests
{
    public ActionLogTests()
    {
        ActionLog.Clear();
    }

    [Fact]
    public void Add_stamps_local_time_and_keeps_oldest_first()
    {
        var ten = new DateTime(2026, 10, 10, 22, 24, 14);
        var eleven = ten.AddSeconds(1);

        Assert.True(ActionLog.Add(ten, "God is on."));
        Assert.True(ActionLog.Add(eleven, "God is off."));

        Assert.Equal(new[] { "[22:24:14] God is on.", "[22:24:15] God is off." }, ActionLog.Entries.ToArray());
        Assert.Equal(2, ActionLog.Count);
    }

    [Fact]
    public void Add_rejects_blank_messages()
    {
        Assert.False(ActionLog.Add(DateTime.Now, null));
        Assert.False(ActionLog.Add(DateTime.Now, ""));
        Assert.False(ActionLog.Add(DateTime.Now, "   "));
        Assert.Equal(0, ActionLog.Count);
    }

    [Fact]
    public void Add_drops_the_oldest_past_forty()
    {
        var start = new DateTime(2026, 10, 10, 0, 0, 0);
        for (int i = 0; i < ActionLog.Capacity + 1; i++)
        {
            ActionLog.Add(start.AddSeconds(i), "line " + i);
        }

        Assert.Equal(ActionLog.Capacity, ActionLog.Count);
        Assert.StartsWith("[00:00:01] line 1", ActionLog.Entries[0]);
        Assert.EndsWith("line 40", ActionLog.Entries[ActionLog.Count - 1]);
    }

    [Fact]
    public void Clear_empties_the_buffer()
    {
        ActionLog.Add(new DateTime(2026, 10, 10, 1, 2, 3), "God is on.");
        ActionLog.Clear();

        Assert.Equal(0, ActionLog.Count);
        Assert.Empty(ActionLog.Entries);
    }

    [Fact]
    public void Format_is_24_hour_and_zero_padded()
    {
        Assert.Equal("[00:05:09] Log cleared.", ActionLog.Format(new DateTime(2026, 10, 10, 0, 5, 9), ActionLog.ClearedMessage));
    }
}
```

- [ ] **Step 2: Run the tests and confirm they fail to compile**

```powershell
dotnet test ValheimQoLCM.sln --nologo --filter ActionLogTests
```

Expected: build error, `ActionLog` does not exist.

- [ ] **Step 3: Write ActionLog**

Create `src/ValheimQoLCM.Core/ActionLog.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>
/// The Log tab's session buffer: the newest <see cref="Capacity"/> actions, oldest first,
/// each stamped with the local time the caller passes. Not persisted.
/// </summary>
public static class ActionLog
{
	/// <summary>How many lines the panel holds.</summary>
	public const int Capacity = 40;

	/// <summary>The line Clear log records after emptying the buffer.</summary>
	public const string ClearedMessage = "Log cleared.";

	private static readonly List<string> Buffer = new List<string>();

	/// <summary>Lines held right now, oldest first, already formatted.</summary>
	public static IReadOnlyList<string> Entries => Buffer;

	/// <summary>How many lines are held.</summary>
	public static int Count => Buffer.Count;

	/// <summary><c>[HH:mm:ss] message</c>, 24-hour, zero-padded, invariant culture.</summary>
	public static string Format(DateTime localTime, string message)
	{
		return "[" + localTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message;
	}

	/// <summary>Record one action. A blank message is ignored and returns false.</summary>
	public static bool Add(DateTime localTime, string? message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return false;
		}
		Buffer.Add(Format(localTime, message.Trim()));
		while (Buffer.Count > Capacity)
		{
			Buffer.RemoveAt(0);
		}
		return true;
	}

	/// <summary>Drop every line.</summary>
	public static void Clear()
	{
		Buffer.Clear();
	}
}
```

`Add` trims the message, so `"  God is on.  "` stores `God is on.`. The tests pass either way.

- [ ] **Step 4: Run the tests**

```powershell
dotnet test ValheimQoLCM.sln --nologo
```

Expected: Passed 52 (47 existing plus these 5).

- [ ] **Step 5: Commit**

```powershell
git add src/ValheimQoLCM.Core/ActionLog.cs tests/ValheimQoLCM.Core.Tests/ActionLogTests.cs
git commit -m "Spec 010: ActionLog buffer, stamp, cap, and clear, with tests."
```

---

### Task 2: One action sink

**Files:**
- Modify: `src/ValheimQoLCM/ConsoleManager.cs` (replace `Show`)
- Modify: `src/ValheimQoLCM/ConsoleView.cs` (`SetStatus`, the mode button, seven `ShowFailure` call sites, `ApplyPick`)
- Modify: `src/ValheimQoLCM/Plugin.cs` (five `ConsoleManager.Show` calls)
- Modify: `src/ValheimQoLCM/PickFilterService.cs` (one line)

**Interfaces:**
- Consumes: `ActionLog.Add`, `ActionLog.Clear`, `ActionLog.ClearedMessage`.
- Produces: `ConsoleManager.Record(string message)`, `ConsoleManager.ClearLog()`. `ConsoleView.PaintLog()` is public and, until Task 3, is the renamed `PaintConsole` reading `ActionLog.Entries`.

Success text for Spawn, Tame, Kill enemies, both teleports, Grant admin, and World Apply already arrives as a host `status` reply through `ConsoleManager.Show` (`Plugin.cs` case `"status"`). Renaming `Show` to `Record` keeps that wording. Local failures of those actions return through `ShowFailure` today and never reach the file; Task 2 sends them through `Record`.

- [ ] **Step 1: Replace `Show` in `ConsoleManager.cs`**

Delete `Show`. Add:

```csharp
/// <summary>Record one panel action: the file, the session buffer, and the Log tab.</summary>
public static void Record(string message)
{
	if (!ActionLog.Add(DateTime.Now, message))
	{
		return;
	}
	PluginStorage.Action(message);
	if (_view != null)
	{
		_view.PaintLog();
	}
	else if ((Object)(object)_plugin != (Object)null)
	{
		Logger.LogInfo((object)message);
	}
}

/// <summary>Empty the session buffer, then record <see cref="ActionLog.ClearedMessage"/>.</summary>
public static void ClearLog()
{
	ActionLog.Clear();
	Record(ActionLog.ClearedMessage);
}
```

`DateTime` needs `using System;`, which the file already has. `ActionLog` is in `ValheimQoLCM.Core`, which the file already imports.

- [ ] **Step 2: Point the host path at `Record`**

In `src/ValheimQoLCM/Plugin.cs`, replace all five `ConsoleManager.Show(` with `ConsoleManager.Record(`. They are `"Admins only."`, `"Not connected."`, `"Server peer missing."`, `Reply`, and the `"status"` case. No other `Show` calls exist.

- [ ] **Step 3: `SetStatus` stops feeding the log**

In `ConsoleView.cs`, delete the field `_logLines` and the const `MaxLogLines`. Replace `SetStatus` with:

```csharp
public void SetStatus(string message)
{
	if (!string.IsNullOrEmpty(message) && (Object)(object)_pickStatus != (Object)null)
	{
		_pickStatus.text = message;
	}
}
```

Replace `PaintConsole` with a public method and update its one caller in `Build` (`PaintConsole();` becomes `PaintLog();`):

```csharp
/// <summary>Repaint the Log tab from <see cref="ActionLog"/>.</summary>
public void PaintLog()
{
	if ((Object)(object)_console == (Object)null)
	{
		return;
	}
	_console.text = string.Join("\n", ActionLog.Entries);
}
```

`string.Join` is in `System`, already imported. `ActionLog` is in `ValheimQoLCM.Core`, already imported.

- [ ] **Step 4: Action call sites record; transient text stays on `SetStatus`**

Add this helper next to `ShowFailure`. Do not change `ShowFailure`; Pick staging and a refused Pick Apply still use it, and those messages stay off the log.

```csharp
private static void Report<T>(ActionResult<T> result)
{
	if (!result.Ok && !string.IsNullOrEmpty(result.Error))
	{
		ConsoleManager.Record(result.Error);
	}
}
```

Replace `ShowFailure(` with `Report(` at exactly these seven sites, and nowhere else:

- `BuildPlayers`: `AdminCommands.RequestBringMe` and `AdminCommands.RequestBringTarget`
- `BuildCheats`: `NearbyCommands.RequestTame` and `NearbyCommands.RequestKillEnemies`
- `Spawn`: `ItemSpawner.RequestSpawn`
- `Grant`: `AdminCommands.RequestGrant`
- `ApplyWorld`: `WorldModifierHost.RequestApply`

In `AddModeButton`, replace the `SetStatus(...)` line with:

```csharp
ConsoleManager.Record(actionResult.Ok ? (caption + " is " + (actionResult.Data ? "on" : "off") + ".") : (actionResult.Error ?? string.Empty));
```

In `ApplyPick`, replace `SetStatus(PickFilter.LogLine(result.Data, PickKnown));` with:

```csharp
ConsoleManager.Record(PickFilter.LogLine(result.Data, PickKnown));
```

Leave `SetStatus(PickFilter.EmptyListMessage)` in `LoadPick` and `RemovePickItem`, and leave `LoadWorld`'s `SetStatus(actionResult.Error)`. Those are transient.

- [ ] **Step 5: One file line per Pick Apply**

In `PickFilterService.Apply`, delete the `PluginStorage.Action(...)` line. `ApplyPick` now records the same line, and `Record` writes the file. Leaving both would write the line twice.

- [ ] **Step 6: Build and test**

```powershell
dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet
dotnet test ValheimQoLCM.sln --nologo
```

Expected: `0 Warning(s)`, `0 Error(s)`, Passed 52. Confirm `Select-String -Path src\ValheimQoLCM\*.cs -Pattern 'ConsoleManager\.Show|_logLines'` prints nothing.

- [ ] **Step 7: Commit**

```powershell
git add src/ValheimQoLCM/ConsoleManager.cs src/ValheimQoLCM/ConsoleView.cs src/ValheimQoLCM/Plugin.cs src/ValheimQoLCM/PickFilterService.cs
git commit -m "Spec 010: panel actions go through Record; status text stays off the log."
```

---

### Task 3: CLI look, Clear log, readout

**Files:**
- Modify: `src/ValheimQoLCM/ConsoleView.cs` (`BuildLog`, `PaintLog`, `ShowTab`, two new fields)

**Interfaces:**
- Consumes: `ConsoleManager.ClearLog()`, `ActionLog.Entries`, `ActionLog.Count`, `PaintLog`.
- Produces: the Log tab described in REQ-4, REQ-5, and REQ-6.

- [ ] **Step 1: Fields**

Next to `private Text _console`, add:

```csharp
private ScrollRect _logScroll = null;

private string _logFontName = "panel";
```

In `Build`, where it assigns `_console = null`, also assign `_logScroll = null`.

- [ ] **Step 2: Black box, white monospace text**

In `BuildLog`, the box image is tinted black over the `text_field` sprite. Replace that sprite assignment and its color:

```csharp
Image component = val.GetComponent<Image>();
component.sprite = SolidSprite();
component.type = Image.Type.Simple;
((Graphic)component).color = new Color(0.75f, 0.75f, 0.75f, 1f);
```

`(Image.Type)0` is `Simple`. The viewport already sits inside this image. Change its offsets and color so the black fill leaves a 1px grey edge and room for the scrollbar:

```csharp
component3.offsetMin = new Vector2(1f, 1f);
component3.offsetMax = new Vector2(-15f, -1f);
((Graphic)val2.GetComponent<Image>()).color = Color.black;
```

Replace the `_console = AddLayoutText(...)` call. Size 14, white, wrap, then the monospace face:

```csharp
_console = AddLayoutText((Transform)(object)component4, string.Empty, 14, Color.white);
_console.alignment = (TextAnchor)0;
_console.horizontalOverflow = (HorizontalWrapMode)0;
ApplyConsoleFont(_console);
_logScroll = val.GetComponent<ScrollRect>();
```

`(TextAnchor)0` is upper left. `(HorizontalWrapMode)0` is Wrap, so a long line wraps inside its own entry and the next action starts after the newline.

Add these two helpers next to `BuildLog`:

```csharp
private static Sprite _solid = null;

private static Sprite SolidSprite()
{
	if ((Object)(object)_solid != (Object)null)
	{
		return _solid;
	}
	Texture2D texture = new Texture2D(1, 1);
	texture.SetPixel(0, 0, Color.white);
	texture.Apply();
	_solid = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
	return _solid;
}

private void ApplyConsoleFont(Text text)
{
	_logFontName = "panel";
	try
	{
		Font consolas = Font.CreateDynamicFontFromOSFont("Consolas", 14);
		if (IsFace(consolas, "Consolas"))
		{
			text.font = consolas;
			_logFontName = "Consolas";
			return;
		}
		Font courier = Font.CreateDynamicFontFromOSFont("Courier New", 14);
		if (IsFace(courier, "Courier New"))
		{
			text.font = courier;
			_logFontName = "Courier New";
		}
	}
	catch (Exception ex)
	{
		Logger.LogWarning((object)("QoL log font failed: " + ex.Message));
	}
}

private static bool IsFace(Font font, string face)
{
	return (Object)(object)font != (Object)null && string.Equals(font.name, face, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 3: Scroll to the newest line**

Extend `PaintLog` so a repaint ends at the bottom:

```csharp
public void PaintLog()
{
	if ((Object)(object)_console == (Object)null)
	{
		return;
	}
	_console.text = string.Join("\n", ActionLog.Entries);
	if ((Object)(object)_logScroll != (Object)null)
	{
		Canvas.ForceUpdateCanvases();
		_logScroll.verticalNormalizedPosition = 0f;
	}
}
```

`0` is the bottom of a vertical `ScrollRect`.

- [ ] **Step 4: Clear log button**

At the end of `BuildLog`, after the scrollbar is wired:

```csharp
RectTransform clearRow = AddRow((Transform)(object)body, (TextAnchor)5);
AddFlexButton((Transform)(object)clearRow, "Clear log", new UnityAction(ClearLog), 140f);
```

`(TextAnchor)5` is middle right, so the button sits on the right of its row under the box. The box has `flexibleHeight = 1`, so the row lands below it.

```csharp
private void ClearLog()
{
	ConsoleManager.ClearLog();
}
```

- [ ] **Step 5: Readout when the tab opens**

Add `LoadLog` and call it from `ShowTab` the way `LoadPick` is called. `Build` already sets `_building` inside a `try`/`finally`, and `ShowTab` runs while it is true, so a scene rebuild does not write the line.

```csharp
private void LoadLog()
{
	PaintLog();
	if (_building)
	{
		return;
	}
	PluginStorage.Debug("Log readout " + ActionLog.Count + ", font " + _logFontName + ".");
}
```

In `ShowTab`, after the `Pick` block:

```csharp
if (text == "Log")
{
	try
	{
		LoadLog();
	}
	catch (Exception ex4)
	{
		Logger.LogWarning((object)("QoL log tab failed: " + ex4));
	}
}
```

- [ ] **Step 6: Build**

```powershell
dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet
dotnet test ValheimQoLCM.sln --nologo
```

Expected: `0 Warning(s)`, `0 Error(s)`, Passed 52.

- [ ] **Step 7: Commit**

```powershell
git add src/ValheimQoLCM/ConsoleView.cs
git commit -m "Spec 010: Log tab is a black console with white monospace text, Clear log, and a readout line."
```

---

### Task 4: Version 2.10 and documents

**Files:**
- Modify: `src/ValheimQoLCM/Plugin.cs`, `src/ValheimQoLCM/ConsoleView.cs` (the header text), both csproj, `README.md`, `docs/USAGE.md`
- Confirm: `CONSTITUTION.md`, `.cursor/rules/master-push-e2e.mdc`, `AGENTS.md` (already done)

**Interfaces:**
- Consumes: nothing new.
- Produces: the `2.10` string everywhere REQ-7 names.

- [ ] **Step 1: Version strings**

`Plugin.cs`: the `BepInPlugin` version, `public const string Version`, and the `loaded` log line, each `1.9` to `2.10`.

`ConsoleView.cs`: `AddText("Version 1.9", ...)` becomes `"Version 2.10"`.

`src/ValheimQoLCM/ValheimQoLCM.csproj`: `<Version>2.10.0</Version>`, `<InformationalVersion>2.10</InformationalVersion>`, `<AssemblyVersion>2.10.0.0</AssemblyVersion>`, `<FileVersion>2.10.0.0</FileVersion>`.

`src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`: `<Version>2.10.0</Version>`, `<InformationalVersion>2.10</InformationalVersion>`.

Confirm, do not edit: `CONSTITUTION.md` contains `2.N` and `010 or later`; `AGENTS.md` points at `specs/010-valheim-qol-cm/plan.md`.

- [ ] **Step 2: README**

Replace the version sentence (the line starting `**Version 1.9** is spec 009`) with:

`**Version 2.10** is spec 010, frozen. The Log tab is a console of the actions taken from the panel, each stamped with the local time, in white monospace on black, with a Clear log button. Version 1.5 remains the last GitHub release. Earlier specs stay at 1.9, 1.8, 1.7, 1.6, 0.4, 0.3, 0.2, and 0.1.`

In the header sentence, `**Version 1.9**` becomes `**Version 2.10**`. In the build paragraph, `**1.9**, spec 009` becomes `**2.10**, spec 010`.

Insert before `### 1.9` in the changelog:

```markdown
### 2.10

- The Log tab records actions taken from the panel: mode toggles, Tame, Kill enemies, the two teleports, Spawn, Grant admin, World Apply, Apply pick filter, and Clear log, plus their failures. Selecting a row or typing a bad quantity is not recorded.
- Each line reads `[HH:mm:ss] message` in white monospace on a black box. Clear log empties the panel and leaves the file.
- `qol-cm.log` keeps every recorded action. The panel starts empty each session.
- The version string is `2.10`.
```

- [ ] **Step 3: USAGE**

Title: `# Valheim QoL CM 2.10`.

Replace the intro sentence (`Spec 009. The panel is six tabs...`) with:

`Spec 010. The Log tab is a console: actions taken from the panel, one line each, stamped with the local time. The Pick tab, the World tab, and the earlier gameplay stay as they were. This spec is frozen. Version 1.5 remains the last GitHub release.`

In the "How to open" paragraph, both `1.9` become `2.10`.

Replace the `### Log` section with:

```markdown
### Log

For admins. A black box with white monospace text, one line per action: `[HH:mm:ss] message`.

- Recorded: God, Fly, Creative, Free cam, Ghost, Tame, Kill enemies, Bring me to player, Bring player to me, Spawn, Grant admin, World Apply, Apply pick filter, and Clear log. A failed action records its error.
- Not recorded: tab clicks, the credit link, selecting a player or an item, a bad quantity or quality, and the Pick tab's own status line.
- **Clear log** empties the box and writes `Log cleared.` as the new first line. `qol-cm.log` beside the plugin keeps the full history. Closing the panel keeps the lines; restarting the game clears the box.
```

Under `## Files beside the plugin`, add to the `qol-cm-debug.log` sentence: `` and on each Log tab open a `Log readout` line with the line count and the font name (`Consolas`, `Courier New`, or `panel`). ``

- [ ] **Step 4: Gate**

```powershell
dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet
dotnet test ValheimQoLCM.sln --nologo --verbosity quiet
git add -A
git diff --cached master | Select-String '^\+' | Select-String '[A-Z]:\\|OneDrive|DESKTOP-|192\.168\.|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,}' | Where-Object { $_.Line -notmatch 'github\.com/LuisbatistaOT/Valheim-QoL-CM' }
git diff --cached master | Select-String '^\+' | Select-String ([Environment]::UserName) | Where-Object { $_.Line -notmatch 'github\.com/LuisbatistaOT/Valheim-QoL-CM' }
```

Expected: `0 Warning(s)`, `0 Error(s)`, Passed 52, and no scan output. A hit that is only the scan regex inside this plan is not a leak; this task does not change the plan, so the plan's regex text is already on the branch and will show. Confirm each printed line is that regex.

- [ ] **Step 5: Commit**

```powershell
git commit -m "Spec 010: version 2.10, README and USAGE Log section."
```

---

### Task 5: Play check and freeze

**Files:**
- Modify: `specs/010-valheim-qol-cm/issues.md`, `status.md`, `tasks.md`, `spec.md` (Status: frozen), `plan.md` (tick the boxes), `tasks/active_context.md`, `tasks/progress.md`, `human-issues.md`, Obsidian `lessons-learned.md`.

- [ ] **Step 1: Install**

```powershell
dotnet build src\ValheimQoLCM\ValheimQoLCM.csproj -warnaserror --no-incremental --nologo --verbosity quiet
$props = Get-Content src\ValheimQoLCM\Valheim.local.props -Raw
$dir = [regex]::Match($props, '<ValheimDir>(.*?)</ValheimDir>').Groups[1].Value
$dest = Join-Path $dir "BepInEx\plugins\ValheimQoLCM"
Copy-Item src\ValheimQoLCM\bin\Debug\ValheimQoLCM.dll,src\ValheimQoLCM\bin\Debug\ValheimQoLCM.pdb,src\ValheimQoLCM\bin\Debug\ValheimQoLCM.Core.dll,src\ValheimQoLCM\bin\Debug\ValheimQoLCM.Core.pdb $dest
```

The output folder is `bin\Debug`, not `bin\Debug\net472`. Never print `$dir` into a committed file. Confirm the DLL's `FileVersion` is `2.10.0.0`.

- [ ] **Step 2: Play, one claim per check**

With `qol-cm-debug.log` open beside the game:

1. BepInEx log has `Valheim QoL CM 2.10 loaded.` The panel header reads `Version 2.10`.
2. Log tab: black box, thin grey edge, white text, Clear log at the lower right. Debug: `Log readout 0, font Consolas.` (or `Courier New` or `panel` when those are what loaded).
3. Toggle God twice: `[HH:mm:ss] God is on.` then `God is off.` The time matches the local clock, not UTC.
4. Select a player and an item, and type a quantity of 0: the log gains no line.
5. Spawn one Wood: `Spawned 1 Wood.` Apply a pick preset: `Pick filter: <preset>, <count> items.`
6. Click Clear log: the box holds only `[HH:mm:ss] Log cleared.` `qol-cm.log` still has the earlier lines plus `Log cleared.`
7. Close the panel and open it, then switch tabs away and back: the line is still there. Quit the game and relaunch: the box starts empty.
8. A non-admin login still opens Pick alone.

Record each defect in `issues.md` with the REQ it cites. Fix, rebuild, copy, and re-check only the failed claim.

- [ ] **Step 3: Freeze**

`spec.md`: `Status: frozen`, add `Frozen: <date>`. `status.md`: Gate section with the build and test output and the play date. `tasks.md`: tick every line. `plan.md`: tick the steps that ran. `tasks/active_context.md` and `tasks/progress.md`: spec 010 frozen at 2.10. `human-issues.md`: one entry. Obsidian `lessons-learned.md`: a `## Spec 010 - version 2.10` section.

- [ ] **Step 4: Gate, commit, push**

Re-run the Task 4 Step 4 gate. Then:

```powershell
git add -A
git commit -m "Freeze spec 010 at 2.10: Log tab."
git push origin spec-010-log-tab
```

Merging into `master` is a separate decision. Publishing the GitHub release waits on DEF-004, as spec 009 left it.
