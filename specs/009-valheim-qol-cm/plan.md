# Pick filter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Pick tab, open to every player, that limits what the local player's auto-pickup takes, with Woodcutting, Mining, Farming, Custom, and Pick all presets, a searchable table, and a filter that survives relog.

**Architecture:** Core owns the preset lists, the draft (on/off plus prefabs), the lit-preset rule, the file format, and the Custom-save rule. `PickFilterService` loads and saves `qol-cm-pick-filter.txt` and answers `Allows(prefab)`. `PickFilterPatch` sets a scope flag around `Player.AutoPickup` and makes `ItemDrop.CanPickup` return false for a filtered prefab inside that scope, so the drop is skipped before the magnet pull and the E key is untouched. `ItemCatalog` takes the item catalog out of `ItemSpawner` so both Spawn and Pick read it. `ConsoleView` builds the tab row from the admin flag: six tabs for an admin, Pick alone for anyone else.

**Tech Stack:** C# / .NET Framework 4.7.2 plugin on BepInEx 5 with Jötunn and Harmony; netstandard2.0 Core with no Unity or Valheim types; xUnit on net8.0 for Core.

## Global Constraints

- Version string is `1.9` in the plugin (`Plugin.cs` attribute, const, and load log), `src/ValheimQoLCM/ValheimQoLCM.csproj`, `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`, the panel header in `ConsoleView.cs`, `README.md`, and `docs/USAGE.md`.
- Specs 001 through 008 keep the strings they already shipped.
- Core (`src/ValheimQoLCM.Core`) has no Unity or Valheim types. Every public member has an XML doc (`-warnaserror` with `GenerateDocumentationFile`).
- The pick filter sends nothing to the host or other players. No ads, bans, or network services beyond BepInEx, Jötunn, and Valheim.
- New defects go in `specs/009-valheim-qol-cm/issues.md` and cite a requirement.
- Plugin source is decompiled IL (DEF-005). New plugin files are written as normal C#; `UnityEngine.Object` null checks use `== null`. Do not rewrite existing decompiled code beyond the lines a task names.
- Gate before any commit lands on `master`: `dotnet build ValheimQoLCM.sln -warnaserror`, `dotnet test ValheimQoLCM.sln`, security scan of the diff, version coherence.
- Run `dotnet test ValheimQoLCM.sln` and `dotnet build ValheimQoLCM.sln -warnaserror` from the repository root. Build output is `src/ValheimQoLCM/bin/Debug/net472/`. The game loads `BepInEx\plugins\ValheimQoLCM\` under the folder in the gitignored `src/ValheimQoLCM/Valheim.local.props` (`ValheimDir`); copy `ValheimQoLCM.dll`, `ValheimQoLCM.pdb`, `ValheimQoLCM.Core.dll`, and `ValheimQoLCM.Core.pdb` there before a play check.

## Files

- Create: `src/ValheimQoLCM.Core/PickFilterDraft.cs` — on/off plus sorted distinct prefabs; immutable.
- Create: `src/ValheimQoLCM.Core/PickFilter.cs` — preset lists, `Allows`, `MatchPreset`, `CanApply`, messages, log line.
- Create: `src/ValheimQoLCM.Core/PickFilterState.cs` — applied draft plus saved Custom list; `Parse`, `Format`, `Apply`.
- Create: `tests/ValheimQoLCM.Core.Tests/PickFilterTests.cs`
- Create: `src/ValheimQoLCM/ItemCatalog.cs` — catalog moved out of `ItemSpawner`, plus `Contains`, `LabelFor`, `Search`.
- Modify: `src/ValheimQoLCM/ItemSpawner.cs` — remove the catalog, call `ItemCatalog`.
- Modify: `src/ValheimQoLCM/ItemCatalogPatch.cs` — call `ItemCatalog.Rebuild()`.
- Create: `src/ValheimQoLCM/PickFilterService.cs` — load, save, `Allows`, `Apply`.
- Create: `src/ValheimQoLCM/PickFilterPatch.cs` — Harmony scope flag and `CanPickup` prefix.
- Modify: `src/ValheimQoLCM/PluginStorage.cs` — read and write `qol-cm-pick-filter.txt`.
- Modify: `src/ValheimQoLCM/Plugin.cs` — load the filter at start; version `1.9`.
- Modify: `src/ValheimQoLCM/ConsoleManager.cs` — open for every player; admin flag drives the tab row; Esc hides the dropdown first.
- Modify: `src/ValheimQoLCM/ConsoleView.cs` — page list from the admin flag; Pick tab; version `1.9`.
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj`, `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj` — version `1.9`.
- Modify: `README.md`, `docs/USAGE.md` — `1.9`, Pick section, changelog.
- Modify: `AGENTS.md` — plan pointer to spec 009.
- Modify: `CONSTITUTION.md` — done in the spec commit.
- Modify: `specs/009-valheim-qol-cm/spec.md`, `status.md`, `tasks.md`, `issues.md`; `specs/backlog.md` if an item is deferred.
- Modify: `tasks/active_context.md`, `tasks/progress.md`, `human-issues.md`, Obsidian `QoL CM/lessons-learned.md`.

---

### Task 1: Core draft and preset rules

**Files:**
- Create: `src/ValheimQoLCM.Core/PickFilterDraft.cs`
- Create: `src/ValheimQoLCM.Core/PickFilter.cs`
- Create: `tests/ValheimQoLCM.Core.Tests/PickFilterTests.cs`

**Interfaces:**
- Produces: `PickFilterDraft(bool on, IEnumerable<string>? items)`, `.On`, `.Items` (`IReadOnlyList<string>`, ordinal-sorted, distinct, trimmed), `.Contains(string)`, `.Add(string)` (turns the filter on), `.Remove(string)`, `.Known(Func<string,bool>)`, `.SameAs(PickFilterDraft)`, `PickFilterDraft.PickAll`.
- Produces: `PickFilter.Woodcutting/Mining/Farming/Custom/PickAll` string constants, `PickFilter.PresetNames`, `PickFilter.Preset(string name, Func<string,bool>? isKnown = null)`, `PickFilter.AllPresetItems()`, `PickFilter.Allows(PickFilterDraft applied, string? prefab)`, `PickFilter.MatchPreset(PickFilterDraft draft, Func<string,bool>? isKnown = null)`, `PickFilter.CanApply(PickFilterDraft)`, `PickFilter.EmptyListMessage`, `PickFilter.NoCustomMessage`, `PickFilter.CountText(int)`, `PickFilter.LogLine(PickFilterDraft, Func<string,bool>? isKnown = null)`.

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using System.Linq;
using ValheimQoLCM.Core;
using Xunit;

namespace ValheimQoLCM.Core.Tests;

public class PickFilterTests
{
    private static bool Known(string prefab) => prefab != "Blackwood";

    [Fact]
    public void Preset_lists_are_non_empty_distinct_and_do_not_overlap()
    {
        var wood = PickFilter.Preset(PickFilter.Woodcutting);
        var mining = PickFilter.Preset(PickFilter.Mining);
        var farming = PickFilter.Preset(PickFilter.Farming);

        Assert.NotEmpty(wood);
        Assert.NotEmpty(mining);
        Assert.NotEmpty(farming);
        Assert.Equal(wood.Count, wood.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(wood.Intersect(mining, StringComparer.Ordinal));
        Assert.Empty(wood.Intersect(farming, StringComparer.Ordinal));
        Assert.Empty(mining.Intersect(farming, StringComparer.Ordinal));
        Assert.Empty(PickFilter.Preset(PickFilter.Custom));
        Assert.Empty(PickFilter.Preset(PickFilter.PickAll));
    }

    [Fact]
    public void Pick_all_allows_everything_and_a_list_allows_only_its_items()
    {
        Assert.True(PickFilter.Allows(PickFilterDraft.PickAll, "Feathers"));
        Assert.True(PickFilter.Allows(PickFilterDraft.PickAll, null));

        var wood = new PickFilterDraft(true, new[] { "Wood", "FineWood" });
        Assert.True(PickFilter.Allows(wood, "Wood"));
        Assert.False(PickFilter.Allows(wood, "Feathers"));
        Assert.False(PickFilter.Allows(wood, null));
    }

    [Fact]
    public void Draft_sorts_trims_and_dedupes_items()
    {
        var draft = new PickFilterDraft(true, new[] { " Wood", "FineWood", "Wood", "", "  " });

        Assert.Equal(new[] { "FineWood", "Wood" }, draft.Items);
        Assert.True(draft.Contains("Wood"));
        Assert.False(draft.Contains("Stone"));
    }

    [Fact]
    public void Adding_to_pick_all_turns_the_filter_on_and_removing_keeps_it_on()
    {
        var added = PickFilterDraft.PickAll.Add("Wood");
        Assert.True(added.On);
        Assert.Equal(new[] { "Wood" }, added.Items);

        var removed = added.Remove("Wood");
        Assert.True(removed.On);
        Assert.Empty(removed.Items);
        Assert.False(PickFilter.CanApply(removed));
        Assert.True(PickFilter.CanApply(PickFilterDraft.PickAll));
        Assert.True(PickFilter.CanApply(added));
    }

    [Fact]
    public void Match_names_each_preset_custom_and_pick_all()
    {
        Assert.Equal(PickFilter.PickAll, PickFilter.MatchPreset(PickFilterDraft.PickAll));
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting))));
        Assert.Equal(PickFilter.Mining, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining))));
        Assert.Equal(PickFilter.Farming, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Farming))));

        var miningWithoutStone = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining).Where(p => p != "Stone"));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(miningWithoutStone));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(new PickFilterDraft(true, Array.Empty<string>())));
    }

    [Fact]
    public void A_prefab_the_game_lacks_still_lets_the_preset_match()
    {
        var staged = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting, Known));

        Assert.DoesNotContain("Blackwood", staged.Items);
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(staged, Known));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(staged));

        var fromFile = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting));
        Assert.True(fromFile.Known(Known).SameAs(staged));
    }

    [Fact]
    public void Same_as_compares_on_flag_and_items()
    {
        var a = new PickFilterDraft(true, new[] { "Wood" });
        Assert.True(a.SameAs(new PickFilterDraft(true, new[] { "Wood" })));
        Assert.False(a.SameAs(new PickFilterDraft(false, new[] { "Wood" })));
        Assert.False(a.SameAs(new PickFilterDraft(true, new[] { "Stone" })));
        Assert.False(a.SameAs(null!));
    }

    [Fact]
    public void Log_line_and_count_text()
    {
        Assert.Equal("Pick filter: Pick all.", PickFilter.LogLine(PickFilterDraft.PickAll));
        Assert.Equal("Pick filter: Custom, 1 item.", PickFilter.LogLine(new PickFilterDraft(true, new[] { "Wood" })));
        var mining = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining));
        Assert.Equal("Pick filter: Mining, " + mining.Items.Count + " items.", PickFilter.LogLine(mining));
        Assert.Equal("2 items", PickFilter.CountText(2));
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test ValheimQoLCM.sln --nologo`
Expected: build error `CS0246: The type or namespace name 'PickFilter' could not be found`.

- [ ] **Step 3: Write the draft**

`src/ValheimQoLCM.Core/PickFilterDraft.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ValheimQoLCM.Core;

/// <summary>
/// A pick filter: off (vanilla auto-pickup) or on with the prefabs auto-pickup may take.
/// Immutable; every change returns a new draft. Items are trimmed, distinct, and ordinal-sorted.
/// </summary>
public sealed class PickFilterDraft
{
	private readonly HashSet<string> _set;

	/// <summary>True when auto-pickup is limited to <see cref="Items"/>. False is Pick all.</summary>
	public bool On { get; }

	/// <summary>Prefab names auto-pickup may take while <see cref="On"/>. Sorted with ordinal comparison.</summary>
	public IReadOnlyList<string> Items { get; }

	/// <summary>The filter off with no items: vanilla auto-pickup.</summary>
	public static PickFilterDraft PickAll => new PickFilterDraft(false, null);

	/// <summary>Create a draft. Null, empty, and whitespace names are dropped; duplicates collapse.</summary>
	public PickFilterDraft(bool on, IEnumerable<string>? items)
	{
		On = on;
		_set = new HashSet<string>(StringComparer.Ordinal);
		if (items != null)
		{
			foreach (string item in items)
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					_set.Add(item.Trim());
				}
			}
		}
		List<string> sorted = _set.ToList();
		sorted.Sort(StringComparer.Ordinal);
		Items = sorted;
	}

	/// <summary>True when the prefab is in the list.</summary>
	public bool Contains(string prefab)
	{
		return _set.Contains(prefab);
	}

	/// <summary>Same items plus this prefab. Adding turns the filter on.</summary>
	public PickFilterDraft Add(string prefab)
	{
		return new PickFilterDraft(true, Items.Concat(new[] { prefab }));
	}

	/// <summary>Same flag, items without this prefab.</summary>
	public PickFilterDraft Remove(string prefab)
	{
		return new PickFilterDraft(On, Items.Where(item => item != prefab));
	}

	/// <summary>Same flag, only the items the game knows. Names a game update removed drop out here.</summary>
	public PickFilterDraft Known(Func<string, bool> isKnown)
	{
		return new PickFilterDraft(On, Items.Where(isKnown));
	}

	/// <summary>Same flag and the same items. Apply stays gray while this is true against the applied filter.</summary>
	public bool SameAs(PickFilterDraft other)
	{
		return other != null && On == other.On && Items.SequenceEqual(other.Items, StringComparer.Ordinal);
	}
}
```

- [ ] **Step 4: Write the rules**

`src/ValheimQoLCM.Core/PickFilter.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ValheimQoLCM.Core;

/// <summary>Pick tab rules: the preset lists, what the applied filter allows, which preset button is lit, and the log line.</summary>
public static class PickFilter
{
	/// <summary>Preset: every kind of wood.</summary>
	public const string Woodcutting = "Woodcutting";

	/// <summary>Preset: stone, ores, and scraps.</summary>
	public const string Mining = "Mining";

	/// <summary>Preset: crops and seeds.</summary>
	public const string Farming = "Farming";

	/// <summary>The saved custom list, or any list that equals no preset.</summary>
	public const string Custom = "Custom";

	/// <summary>Filter off: vanilla auto-pickup.</summary>
	public const string PickAll = "Pick all";

	/// <summary>Status line when the staged list is empty while the filter is on.</summary>
	public const string EmptyListMessage = "Add an item or choose Pick all.";

	/// <summary>Status line when Custom is clicked before any custom list was applied.</summary>
	public const string NoCustomMessage = "No custom list yet. Add or remove an item, then Apply.";

	/// <summary>Button order on the Pick tab.</summary>
	public static readonly IReadOnlyList<string> PresetNames = new[] { Woodcutting, Mining, Farming, Custom, PickAll };

	private static readonly string[] WoodcuttingItems = { "Wood", "FineWood", "RoundLog", "ElderBark", "YggdrasilWood", "Blackwood" };

	private static readonly string[] MiningItems = { "Stone", "CopperOre", "TinOre", "CopperScrap", "IronScrap", "SilverOre", "BlackMetalScrap", "FlametalOre", "FlametalOreNew", "Obsidian", "Chitin", "BlackMarble", "Softtissue", "Grausten" };

	private static readonly string[] FarmingItems = { "Carrot", "CarrotSeeds", "Turnip", "TurnipSeeds", "Onion", "OnionSeeds", "Barley", "Flax", "JotunPuffs", "Magecap", "Fiddlehead", "Vineberry", "SmokePuff" };

	/// <summary>
	/// Prefabs a preset stages. Custom and Pick all return an empty list. When <paramref name="isKnown"/> is given,
	/// names it rejects are left out, so a preset still matches on a game that lacks one of its items.
	/// </summary>
	public static IReadOnlyList<string> Preset(string name, Func<string, bool>? isKnown = null)
	{
		string[] items = name switch
		{
			Woodcutting => WoodcuttingItems,
			Mining => MiningItems,
			Farming => FarmingItems,
			_ => Array.Empty<string>(),
		};
		return isKnown == null ? (IReadOnlyList<string>)items : items.Where(isKnown).ToList();
	}

	/// <summary>Every prefab in the three preset lists, for the drop log on tab open.</summary>
	public static IEnumerable<string> AllPresetItems()
	{
		return WoodcuttingItems.Concat(MiningItems).Concat(FarmingItems);
	}

	/// <summary>True when auto-pickup may take this prefab: the filter is off, or the prefab is listed.</summary>
	public static bool Allows(PickFilterDraft applied, string? prefab)
	{
		if (!applied.On)
		{
			return true;
		}
		return prefab != null && applied.Contains(prefab);
	}

	/// <summary>
	/// The one lit button: Pick all when the filter is off; a preset whose known items equal the staged items; otherwise Custom.
	/// </summary>
	public static string MatchPreset(PickFilterDraft draft, Func<string, bool>? isKnown = null)
	{
		if (!draft.On)
		{
			return PickAll;
		}
		foreach (string name in new[] { Woodcutting, Mining, Farming })
		{
			IReadOnlyList<string> items = Preset(name, isKnown);
			if (items.Count > 0 && items.Count == draft.Items.Count && items.All(draft.Contains))
			{
				return name;
			}
		}
		return Custom;
	}

	/// <summary>Pick all always applies. A filter that is on needs at least one item.</summary>
	public static bool CanApply(PickFilterDraft draft)
	{
		return !draft.On || draft.Items.Count > 0;
	}

	/// <summary><c>1 item</c> or <c>N items</c>.</summary>
	public static string CountText(int count)
	{
		return count == 1 ? "1 item" : count + " items";
	}

	/// <summary>The <c>qol-cm.log</c> line for an Apply.</summary>
	public static string LogLine(PickFilterDraft applied, Func<string, bool>? isKnown = null)
	{
		string preset = MatchPreset(applied, isKnown);
		if (preset == PickAll)
		{
			return "Pick filter: Pick all.";
		}
		return "Pick filter: " + preset + ", " + CountText(applied.Items.Count) + ".";
	}
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ValheimQoLCM.sln --nologo`
Expected: `Passed! - Failed: 0, Passed: 35` (27 existing + 8 new).

- [ ] **Step 6: Commit**

```powershell
git add src/ValheimQoLCM.Core/PickFilterDraft.cs src/ValheimQoLCM.Core/PickFilter.cs tests/ValheimQoLCM.Core.Tests/PickFilterTests.cs
git commit -m "Spec 009: Core pick filter draft, presets, lit-preset rule, and log line."
```

---

### Task 2: Core state, file format, and the Custom-save rule

**Files:**
- Create: `src/ValheimQoLCM.Core/PickFilterState.cs`
- Modify: `tests/ValheimQoLCM.Core.Tests/PickFilterTests.cs`

**Interfaces:**
- Consumes: `PickFilterDraft`, `PickFilter.MatchPreset`, `PickFilter.Custom`.
- Produces: `PickFilterState(PickFilterDraft applied, IReadOnlyList<string>? custom)`, `.Applied`, `.Custom` (null when none saved), `PickFilterState.Default`, `.Apply(PickFilterDraft draft, Func<string,bool>? isKnown)`, `PickFilterState.Parse(string? text)`, `PickFilterState.Format(PickFilterState state)`.

File format, one key per line, prefab names separated by spaces (Valheim prefab names have no spaces):

```
mode list
list FineWood Wood
custom IronScrap Stone
```

`mode all` is Pick all. A missing or unreadable file, an unknown key, or `mode list` with no `list` line reads as Pick all with no Custom list.

- [ ] **Step 1: Add the failing tests**

Append inside `PickFilterTests`:

```csharp
    [Fact]
    public void Parse_of_missing_or_garbage_text_is_pick_all_without_custom()
    {
        foreach (string? text in new[] { null, "", "   ", "nonsense\r\nmore", "mode list" })
        {
            var state = PickFilterState.Parse(text);
            Assert.False(state.Applied.On);
            Assert.Empty(state.Applied.Items);
            Assert.Null(state.Custom);
        }
    }

    [Fact]
    public void Format_then_parse_round_trips()
    {
        var state = new PickFilterState(new PickFilterDraft(true, new[] { "Wood", "FineWood" }), new[] { "Stone", "IronScrap" });

        string text = PickFilterState.Format(state);
        var back = PickFilterState.Parse(text);

        Assert.Equal("mode list\nlist FineWood Wood\ncustom IronScrap Stone", text);
        Assert.True(back.Applied.SameAs(state.Applied));
        Assert.Equal(new[] { "IronScrap", "Stone" }, back.Custom);

        var off = PickFilterState.Parse(PickFilterState.Format(PickFilterState.Default));
        Assert.False(off.Applied.On);
        Assert.Null(off.Custom);
    }

    [Fact]
    public void Parse_accepts_windows_line_endings_and_extra_spaces()
    {
        var state = PickFilterState.Parse("mode  list\r\nlist  Wood   FineWood \r\ncustom Stone\r\n");

        Assert.True(state.Applied.On);
        Assert.Equal(new[] { "FineWood", "Wood" }, state.Applied.Items);
        Assert.Equal(new[] { "Stone" }, state.Custom);
    }

    [Fact]
    public void Apply_saves_custom_only_for_a_list_that_equals_no_preset()
    {
        var state = PickFilterState.Default;

        var mining = state.Apply(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining)), null);
        Assert.Null(mining.Custom);

        var custom = mining.Apply(new PickFilterDraft(true, new[] { "Stone", "IronScrap" }), null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, custom.Custom);

        var backToPreset = custom.Apply(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting)), null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, backToPreset.Custom);
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(backToPreset.Applied));

        var pickAll = backToPreset.Apply(PickFilterDraft.PickAll, null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, pickAll.Custom);
        Assert.False(pickAll.Applied.On);
    }
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test ValheimQoLCM.sln --nologo`
Expected: build error `CS0246: ... 'PickFilterState' could not be found`.

- [ ] **Step 3: Write the state**

`src/ValheimQoLCM.Core/PickFilterState.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ValheimQoLCM.Core;

/// <summary>What <c>qol-cm-pick-filter.txt</c> holds: the applied filter and the saved Custom list. Immutable.</summary>
public sealed class PickFilterState
{
	private static readonly char[] Separators = { ' ', '\t' };

	/// <summary>The filter auto-pickup follows now.</summary>
	public PickFilterDraft Applied { get; }

	/// <summary>The last applied list that equaled no preset, or null when none was applied yet.</summary>
	public IReadOnlyList<string>? Custom { get; }

	/// <summary>Pick all with no Custom list. What a missing file reads as.</summary>
	public static PickFilterState Default => new PickFilterState(PickFilterDraft.PickAll, null);

	/// <summary>Create a state. An empty custom list is stored as null.</summary>
	public PickFilterState(PickFilterDraft applied, IReadOnlyList<string>? custom)
	{
		Applied = applied;
		Custom = custom != null && custom.Count > 0 ? new PickFilterDraft(true, custom).Items : null;
	}

	/// <summary>
	/// The state after applying a draft. A list that equals no preset becomes the Custom list;
	/// a preset or Pick all keeps the Custom list already saved.
	/// </summary>
	public PickFilterState Apply(PickFilterDraft draft, Func<string, bool>? isKnown)
	{
		IReadOnlyList<string>? custom = PickFilter.MatchPreset(draft, isKnown) == PickFilter.Custom ? draft.Items : Custom;
		return new PickFilterState(draft, custom);
	}

	/// <summary>Read the file text. Anything unreadable is <see cref="Default"/>.</summary>
	public static PickFilterState Parse(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return Default;
		}
		bool on = false;
		List<string> items = new List<string>();
		List<string>? custom = null;
		foreach (string raw in text!.Split('\n'))
		{
			string line = raw.Trim();
			if (line.Length == 0)
			{
				continue;
			}
			int space = line.IndexOfAny(Separators);
			string key = space < 0 ? line : line.Substring(0, space);
			string value = space < 0 ? string.Empty : line.Substring(space + 1);
			string[] names = value.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
			switch (key.ToLowerInvariant())
			{
			case "mode":
				on = names.Length > 0 && names[0] == "list";
				break;
			case "list":
				items.AddRange(names);
				break;
			case "custom":
				custom = names.ToList();
				break;
			}
		}
		if (on && items.Count == 0)
		{
			on = false;
		}
		return new PickFilterState(new PickFilterDraft(on, on ? items : null), custom);
	}

	/// <summary>The file text for a state. Lines are joined with <c>\n</c>.</summary>
	public static string Format(PickFilterState state)
	{
		List<string> lines = new List<string> { "mode " + (state.Applied.On ? "list" : "all") };
		if (state.Applied.On && state.Applied.Items.Count > 0)
		{
			lines.Add("list " + string.Join(" ", state.Applied.Items));
		}
		if (state.Custom != null)
		{
			lines.Add("custom " + string.Join(" ", state.Custom));
		}
		return string.Join("\n", lines);
	}
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test ValheimQoLCM.sln --nologo`
Expected: `Passed! - Failed: 0, Passed: 39`.

- [ ] **Step 5: Lint**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 6: Commit**

```powershell
git add src/ValheimQoLCM.Core/PickFilterState.cs tests/ValheimQoLCM.Core.Tests/PickFilterTests.cs
git commit -m "Spec 009: Core pick filter state, file format, and Custom-save rule."
```

---

### Task 3: Move the item catalog out of `ItemSpawner`

Behavior-neutral for Spawn. No Core test covers this; the check is a clean build and the Spawn tab still listing items in Task 9.

**Files:**
- Create: `src/ValheimQoLCM/ItemCatalog.cs`
- Modify: `src/ValheimQoLCM/ItemSpawner.cs` (delete lines 15–115 `CatalogEntry`, `Catalog`, `RebuildCatalog`, `ApplyRowLabels`, `FindItems`; delete `MaxStack` lines 117–131; delete `LabelFor` lines 248–262; delete `Localize` lines 264–276)
- Modify: `src/ValheimQoLCM/ItemCatalogPatch.cs:14`
- Modify: `src/ValheimQoLCM/ConsoleView.cs:927` and `:1014`

**Interfaces:**
- Produces: `ItemCatalog.Rebuild()`, `ItemCatalog.FindItems(string filter)` (rebuilds every call, as Spawn did), `ItemCatalog.Search(string text, int max, Func<string,bool> exclude)` (rebuilds only when empty; sorted by label; stops at `max`), `ItemCatalog.Contains(string prefab)`, `ItemCatalog.LabelFor(string prefab)`, `ItemCatalog.MaxStack(string? prefab)`.

- [ ] **Step 1: Create `ItemCatalog.cs`**

```csharp
using SharedData = ItemDrop.ItemData.SharedData;
using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Every item prefab a player can pick up, labeled by translated name. Spawn lists it; Pick searches it.</summary>
public static class ItemCatalog
{
	private sealed class Entry
	{
		public string Prefab { get; }

		public string Token { get; }

		public string Label { get; set; }

		public int MaxStack { get; }

		public int MaxQuality { get; }

		public Entry(string prefab, string token, int maxStack, int maxQuality)
		{
			Prefab = prefab;
			Token = token;
			Label = prefab;
			MaxStack = maxStack;
			MaxQuality = maxQuality;
		}
	}

	private static readonly List<Entry> Entries = new List<Entry>();

	private static readonly Dictionary<string, Entry> ByPrefab = new Dictionary<string, Entry>(StringComparer.Ordinal);

	/// <summary>Read <c>ObjectDB</c> again. Called after items load and by <see cref="FindItems"/>.</summary>
	public static void Rebuild()
	{
		Entries.Clear();
		ByPrefab.Clear();
		if (ObjectDB.instance == null)
		{
			return;
		}
		foreach (GameObject item in ObjectDB.instance.m_items)
		{
			if (item == null)
			{
				continue;
			}
			ItemDrop drop = item.GetComponent<ItemDrop>();
			if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
			{
				continue;
			}
			if (ObjectDB.instance.GetItemPrefab(item.name) != item)
			{
				continue;
			}
			SharedData shared = drop.m_itemData.m_shared;
			int iconCount = shared.m_icons != null ? shared.m_icons.Length : 0;
			if (!ItemListing.CanPickUp(iconCount))
			{
				continue;
			}
			int maxStack = shared.m_maxStackSize < 1 ? 1 : shared.m_maxStackSize;
			int maxQuality = shared.m_maxQuality < 1 ? 1 : shared.m_maxQuality;
			Entry entry = new Entry(item.name, shared.m_name, maxStack, maxQuality);
			Entries.Add(entry);
			ByPrefab[entry.Prefab] = entry;
		}
		ApplyRowLabels();
	}

	private static void EnsureBuilt()
	{
		if (Entries.Count == 0)
		{
			Rebuild();
		}
	}

	private static void ApplyRowLabels()
	{
		Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		string[] localized = new string[Entries.Count];
		for (int i = 0; i < Entries.Count; i++)
		{
			localized[i] = Localize(Entries[i].Token, Entries[i].Prefab);
			string key = ItemListing.BaseName(localized[i], Entries[i].Prefab);
			counts.TryGetValue(key, out int count);
			counts[key] = count + 1;
		}
		for (int j = 0; j < Entries.Count; j++)
		{
			string key = ItemListing.BaseName(localized[j], Entries[j].Prefab);
			Entries[j].Label = ItemListing.RowLabel(localized[j], Entries[j].Prefab, counts[key] > 1);
		}
	}

	/// <summary>Spawn list: every entry whose label, prefab, or token contains the filter, sorted by label. Rebuilds first.</summary>
	public static IReadOnlyList<SpawnableItem> FindItems(string filter)
	{
		Rebuild();
		string text = string.IsNullOrWhiteSpace(filter) ? string.Empty : filter.Trim();
		List<SpawnableItem> list = new List<SpawnableItem>();
		foreach (Entry entry in Entries)
		{
			if (text.Length == 0 || Matches(entry, text))
			{
				list.Add(new SpawnableItem(entry.Prefab, entry.Label, entry.MaxStack, entry.MaxQuality));
			}
		}
		list.Sort((left, right) => string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase));
		return list;
	}

	/// <summary>Pick dropdown: up to <paramref name="max"/> matches by label, skipping prefabs <paramref name="exclude"/> accepts.</summary>
	public static IReadOnlyList<SpawnableItem> Search(string text, int max, Func<string, bool> exclude)
	{
		EnsureBuilt();
		string needle = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
		List<SpawnableItem> list = new List<SpawnableItem>();
		if (needle.Length == 0)
		{
			return list;
		}
		foreach (Entry entry in Entries)
		{
			if (!exclude(entry.Prefab) && Matches(entry, needle))
			{
				list.Add(new SpawnableItem(entry.Prefab, entry.Label, entry.MaxStack, entry.MaxQuality));
			}
		}
		list.Sort((left, right) => string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase));
		if (list.Count > max)
		{
			list.RemoveRange(max, list.Count - max);
		}
		return list;
	}

	/// <summary>True when the loaded game has this item prefab.</summary>
	public static bool Contains(string prefab)
	{
		EnsureBuilt();
		return ByPrefab.ContainsKey(prefab);
	}

	/// <summary>Translated row label, or the prefab name when it is not in the catalog.</summary>
	public static string LabelFor(string prefab)
	{
		EnsureBuilt();
		return ByPrefab.TryGetValue(prefab, out Entry entry) ? entry.Label : prefab;
	}

	/// <summary>Stack size for the Spawn quantity controls. 1 when unknown.</summary>
	public static int MaxStack(string? prefab)
	{
		EnsureBuilt();
		return prefab != null && ByPrefab.TryGetValue(prefab, out Entry entry) ? entry.MaxStack : 1;
	}

	private static bool Matches(Entry entry, string text)
	{
		return entry.Label.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
			|| entry.Prefab.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
			|| (entry.Token != null && entry.Token.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
	}

	private static string Localize(string token, string prefab)
	{
		if (string.IsNullOrEmpty(token) || Localization.instance == null)
		{
			return prefab;
		}
		string text = Localization.instance.Localize(token);
		if (string.IsNullOrEmpty(text) || text[0] == '$')
		{
			return prefab;
		}
		return text;
	}
}
```

- [ ] **Step 2: Trim `ItemSpawner.cs`**

Delete the nested `CatalogEntry` class, the `Catalog` field, `RebuildCatalog`, `ApplyRowLabels`, `FindItems`, `MaxStack`, `LabelFor`, and `Localize`. Keep `RequestSpawn`, `ApplySpawn`, `MaxQuality`, and `TryFeet`. In `ApplySpawn` replace the last line's `LabelFor(actionResult.Data.Prefab)` with `ItemCatalog.LabelFor(actionResult.Data.Prefab)`. Remove the now-unused `using SharedData = ItemDrop.ItemData.SharedData;` only if `ApplySpawn` no longer references `SharedData` (it does, at `SharedData val3 = ...`; keep it).

- [ ] **Step 3: Repoint the callers**

`src/ValheimQoLCM/ItemCatalogPatch.cs` line 14: `ItemSpawner.RebuildCatalog();` → `ItemCatalog.Rebuild();`

`src/ValheimQoLCM/ConsoleView.cs` line 927: `ItemSpawner.FindItems(filter)` → `ItemCatalog.FindItems(filter)`.
Line 1014: `ItemSpawner.MaxStack(_item)` → `ItemCatalog.MaxStack(_item)`.

- [ ] **Step 4: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`. If `CS0246 SharedData` appears in `ItemCatalog.cs`, the alias line at the top of the file is missing.

- [ ] **Step 5: Commit**

```powershell
git add src/ValheimQoLCM/ItemCatalog.cs src/ValheimQoLCM/ItemSpawner.cs src/ValheimQoLCM/ItemCatalogPatch.cs src/ValheimQoLCM/ConsoleView.cs
git commit -m "Spec 009: item catalog shared by Spawn and Pick."
```

---

### Task 4: Storage and the filter service

**Files:**
- Modify: `src/ValheimQoLCM/PluginStorage.cs`
- Create: `src/ValheimQoLCM/PickFilterService.cs`
- Modify: `src/ValheimQoLCM/Plugin.cs:72` (Awake)

**Interfaces:**
- Consumes: `PickFilterState`, `PickFilter`, `PickFilterDraft`, `ItemCatalog.Contains`, `ActionResult<T>.Success/Fail`.
- Produces: `PluginStorage.ReadPickFilter()` → `string?`, `PluginStorage.WritePickFilter(string)`; `PickFilterService.State`, `.Load()`, `.Allows(string? prefab)`, `.AppliedForView()`, `.Apply(PickFilterDraft)` → `ActionResult<PickFilterDraft>`.

- [ ] **Step 1: Storage**

In `PluginStorage.cs` add after the `OverwriteFile` constant:

```csharp
	private const string PickFilterFile = "qol-cm-pick-filter.txt";
```

Add after `WriteSkillLoss`:

```csharp
	public static string? ReadPickFilter()
	{
		try
		{
			string path = Path.Combine(Folder(), PickFilterFile);
			return File.Exists(path) ? File.ReadAllText(path) : null;
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not read the pick filter: " + ex.Message));
			return null;
		}
	}

	public static void WritePickFilter(string contents)
	{
		try
		{
			File.WriteAllText(Path.Combine(Folder(), PickFilterFile), contents);
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not save the pick filter: " + ex.Message));
		}
	}
```

- [ ] **Step 2: Service**

`src/ValheimQoLCM/PickFilterService.cs`:

```csharp
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Holds the applied pick filter on this machine, saves it, and answers the auto-pickup patch.</summary>
public static class PickFilterService
{
	/// <summary>Applied filter and saved Custom list. Starts as Pick all until <see cref="Load"/> runs.</summary>
	public static PickFilterState State { get; private set; } = PickFilterState.Default;

	/// <summary>Read <c>qol-cm-pick-filter.txt</c>. Called once from <c>Plugin.Awake</c>.</summary>
	public static void Load()
	{
		State = PickFilterState.Parse(PluginStorage.ReadPickFilter());
		string custom = State.Custom == null ? "none" : PickFilter.CountText(State.Custom.Count);
		PluginStorage.Debug("Pick filter loaded: " + PickFilter.MatchPreset(State.Applied) + ", " + PickFilter.CountText(State.Applied.Items.Count) + ", custom " + custom + ".");
	}

	/// <summary>True when auto-pickup may take this prefab.</summary>
	public static bool Allows(string? prefab)
	{
		return PickFilter.Allows(State.Applied, prefab);
	}

	/// <summary>The applied filter with prefabs this game lacks left out. The Pick tab stages a copy of this.</summary>
	public static PickFilterDraft AppliedForView()
	{
		return State.Applied.Known(ItemCatalog.Contains);
	}

	/// <summary>Apply a staged filter: save the file, write the action log, update <see cref="State"/>.</summary>
	public static ActionResult<PickFilterDraft> Apply(PickFilterDraft staged)
	{
		if (!PickFilter.CanApply(staged))
		{
			return ActionResult<PickFilterDraft>.Fail(PickFilter.EmptyListMessage);
		}
		State = State.Apply(staged, ItemCatalog.Contains);
		PluginStorage.WritePickFilter(PickFilterState.Format(State));
		PluginStorage.Action(PickFilter.LogLine(staged, ItemCatalog.Contains));
		return ActionResult<PickFilterDraft>.Success(staged);
	}
}
```

- [ ] **Step 3: Load at start**

In `Plugin.Awake`, before `ConsoleManager.Create(this);` add:

```csharp
		PickFilterService.Load();
```

- [ ] **Step 4: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 5: Commit**

```powershell
git add src/ValheimQoLCM/PluginStorage.cs src/ValheimQoLCM/PickFilterService.cs src/ValheimQoLCM/Plugin.cs
git commit -m "Spec 009: pick filter service loads and saves qol-cm-pick-filter.txt."
```

---

### Task 5: Auto-pickup patch

**Files:**
- Create: `src/ValheimQoLCM/PickFilterPatch.cs`

**Interfaces:**
- Consumes: `PickFilterService.Allows(string?)`.
- Valheim: `Player.AutoPickup(float dt)` is private; it calls `component.CanPickup()` per nearby `ItemDrop` and skips the drop (calling `RequestOwn()`) when that returns false, before the pull toward the player. `ItemDrop.Interact → Pickup` never goes through that branch. `Utils.GetPrefabName(GameObject)` strips `(Clone)`.

- [ ] **Step 1: Write the patch**

```csharp
using HarmonyLib;

namespace ValheimQoLCM;

/// <summary>True only while <c>Player.AutoPickup</c> is on the stack. The E key never sets it.</summary>
internal static class PickFilterScope
{
	public static bool InAutoPickup;
}

[HarmonyPatch(typeof(Player), "AutoPickup")]
internal static class AutoPickupScopePatch
{
	[HarmonyPrefix]
	private static void Enter()
	{
		PickFilterScope.InAutoPickup = true;
	}

	[HarmonyFinalizer]
	private static void Leave()
	{
		PickFilterScope.InAutoPickup = false;
	}
}

[HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.CanPickup))]
internal static class CanPickupFilterPatch
{
	[HarmonyPrefix]
	private static bool SkipFiltered(ItemDrop __instance, ref bool __result)
	{
		if (!PickFilterScope.InAutoPickup)
		{
			return true;
		}
		if (PickFilterService.Allows(PrefabName(__instance)))
		{
			return true;
		}
		__result = false;
		return false;
	}

	private static string? PrefabName(ItemDrop drop)
	{
		ItemDrop.ItemData data = drop.m_itemData;
		if (data != null && data.m_dropPrefab != null)
		{
			return data.m_dropPrefab.name;
		}
		return Utils.GetPrefabName(drop.gameObject);
	}
}
```

- [ ] **Step 2: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`. `Plugin.Awake` already calls `PatchAll(typeof(Plugin).Assembly)`, so no registration is needed.

- [ ] **Step 3: Commit**

```powershell
git add src/ValheimQoLCM/PickFilterPatch.cs
git commit -m "Spec 009: auto-pickup skips filtered drops; E is untouched."
```

---

### Task 6: Open the panel to every player; tabs from the admin flag

**Files:**
- Modify: `src/ValheimQoLCM/ConsoleView.cs` (`PageNames` at line 40, `Build` lines 128–198, `ShowTab` lines 1199–1266, new `SetAdmin` and `HideDropdown`)
- Modify: `src/ValheimQoLCM/ConsoleManager.cs` (`Tick` lines 147–182, `SetOpen` lines 184–198, `RefreshWhileOpen` lines 200–215)

**Interfaces:**
- Produces: `ConsoleView.SetAdmin(bool admin)` → `bool` (true when the panel was rebuilt), `ConsoleView.HideDropdown()` → `bool` (true when a dropdown was open; the Pick tab fills it in Task 7, so this task adds the stub returning false).
- Page names are `Cheats, World, Spawn, Players, Pick, Log` for an admin and `Pick` alone otherwise.

- [ ] **Step 1: Page lists in `ConsoleView`**

Replace line 40:

```csharp
	private static readonly string[] PageNames = new string[5] { "Cheats", "World", "Spawn", "Players", "Log" };
```

with:

```csharp
	private static readonly string[] AdminPages = new string[6] { "Cheats", "World", "Spawn", "Players", "Pick", "Log" };

	private static readonly string[] PlayerPages = new string[1] { "Pick" };

	private string[] _pageNames = PlayerPages;

	private bool _admin;

	private Transform _parent = null;
```

Every other use of `PageNames` in the file (`ShowTab` lines 1212–1229) becomes `_pageNames`.

- [ ] **Step 2: `Build` builds tabs and pages from `_pageNames`**

In `Build(Transform parent)`, after `_stepKnobs.Clear();` add:

```csharp
		_parent = parent;
		_pageNames = _admin ? AdminPages : PlayerPages;
		if (_tab >= _pageNames.Length)
		{
			_tab = 0;
		}
```

Replace the five `AddTab(...)` lines and the page construction block (from `RectTransform parent2 = (_pagesRoot = ...` through `BuildLog(AddPage(parent2, "Log"));`) with:

```csharp
		for (int t = 0; t < _pageNames.Length; t++)
		{
			AddTab(_tabRow, _pageNames[t], t);
		}
		RectTransform parent2 = (_pagesRoot = CreateStretch("Pages", _root.transform, 16f, 44f, 16f, 90f));
		for (int p = 0; p < _pageNames.Length; p++)
		{
			BuildPage(_pageNames[p], AddPage(parent2, _pageNames[p]));
		}
```

Keep `ShowTab(0);` and the rest unchanged, but change `ShowTab(0)` to `ShowTab(_tab)`. Add the method next to `Build`:

```csharp
	private void BuildPage(string name, RectTransform body)
	{
		switch (name)
		{
		case "Cheats":
			BuildCheats(body);
			break;
		case "World":
			try
			{
				BuildWorld(body);
			}
			catch (Exception ex)
			{
				Logger.LogWarning((object)("QoL world tab failed: " + ex));
			}
			break;
		case "Spawn":
			BuildItems(body);
			break;
		case "Players":
			BuildPlayers(body);
			break;
		case "Pick":
			BuildPick(body);
			break;
		default:
			BuildLog(body);
			break;
		}
	}
```

Until Task 7 adds `BuildPick`, add this stub so the build stays green; Task 7 replaces it:

```csharp
	private void BuildPick(RectTransform body)
	{
		AddLayoutText((Transform)(object)body, "Pick tab", 16, Color.white);
	}

	public bool HideDropdown()
	{
		return false;
	}
```

- [ ] **Step 3: `SetAdmin`**

Add after `SetVisible`:

```csharp
	/// <summary>Rebuilds the panel when the admin flag changes, so the tab row matches. Returns true when it rebuilt.</summary>
	public bool SetAdmin(bool admin)
	{
		if (admin == _admin && IsBuilt)
		{
			return false;
		}
		_admin = admin;
		if (_parent == null)
		{
			return false;
		}
		Build(_parent);
		return true;
	}
```

- [ ] **Step 4: `ConsoleManager` opens for everyone**

In `Tick`, replace the branch

```csharp
		else if (Plugin.LocalIsAdmin())
		{
			if (_view == null || !_view.IsBuilt)
			{
				Logger.LogWarning((object)"QoL panel could not be created.");
			}
			else
			{
				SetOpen(!_open);
			}
		}
```

with

```csharp
		else
		{
			_view.SetAdmin(Plugin.LocalIsAdmin());
			if (!_view.IsBuilt)
			{
				Logger.LogWarning((object)"QoL panel could not be created.");
			}
			else
			{
				SetOpen(!_open);
			}
		}
```

In `SetOpen`, before `_view.SetVisible(open);` add:

```csharp
		if (open)
		{
			_view.SetAdmin(Plugin.LocalIsAdmin());
		}
```

Replace `RefreshWhileOpen` with:

```csharp
	private static void RefreshWhileOpen()
	{
		if (!_open || _view == null || !_view.IsBuilt)
		{
			return;
		}
		if (Input.GetKeyDown((KeyCode)27))
		{
			if (!_view.HideDropdown())
			{
				SetOpen(open: false);
			}
			return;
		}
		if (Time.unscaledTime < _nextRefresh)
		{
			return;
		}
		_nextRefresh = Time.unscaledTime + 1f;
		if (_view.SetAdmin(Plugin.LocalIsAdmin()))
		{
			_view.SetVisible(true);
			_view.BringToFront();
		}
		_view.RefreshPlayers();
		_view.RefreshModes();
	}
```

Also update the `Config.Bind` description on `TogglePanel` from "Open or close the admin panel." to "Open or close the QoL panel. Every player gets the Pick tab; admins get the rest."

- [ ] **Step 5: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 6: Commit**

```powershell
git add src/ValheimQoLCM/ConsoleView.cs src/ValheimQoLCM/ConsoleManager.cs
git commit -m "Spec 009: panel opens for every player; tab row follows the admin flag."
```

---

### Task 7: The Pick tab

`@ui-architect` owns this layout. The page is a vertical layout: preset row, one-line hint, search field, header line, the table (scroll list, flexible height), Apply row. The dropdown is a child of the panel root, not of the page, so no layout group moves it; it is placed under the search field from the field's world corners and drawn last.

**Files:**
- Modify: `src/ValheimQoLCM/ConsoleView.cs` (fields near line 120, `Build` clear block, `BuildPick` replaces the Task 6 stub, `HideDropdown` replaces the stub, `ShowTab` and `SetVisible` hooks)

**Interfaces:**
- Consumes: `PickFilter`, `PickFilterDraft`, `PickFilterService.State/AppliedForView/Apply`, `ItemCatalog.Search/Contains/LabelFor`, `SpawnableItem.Prefab/Label`, and the view helpers `AddRow`, `AddFlexButton`, `AddLayoutText`, `AddField`, `AddScrollList`, `AddRowButton`, `PaintSelectable`, `Clear`, `SetStatus`, `ShowFailure`.

- [ ] **Step 1: Fields**

Add after `private int _ignoreSliderFrame = -1;`:

```csharp
	private static readonly string[] PickPresetNames = new string[5] { PickFilter.Woodcutting, PickFilter.Mining, PickFilter.Farming, PickFilter.Custom, PickFilter.PickAll };

	private readonly List<Button> _pickPresetButtons = new List<Button>();

	private readonly HashSet<string> _pickDroppedLogged = new HashSet<string>(StringComparer.Ordinal);

	private Text _pickHeader = null;

	private InputField _pickSearch = null;

	private RectTransform _pickTable = null;

	private GameObject _pickDropdown = null;

	private RectTransform _pickDropdownRect = null;

	private Button _applyPick = null;

	private PickFilterDraft _pickStaged = PickFilterDraft.PickAll;

	private int _pickRows;
```

In `Build`, in the clear block after `_stepKnobs.Clear();` add:

```csharp
		_pickPresetButtons.Clear();
		_pickDropdown = null;
		_pickDropdownRect = null;
```

- [ ] **Step 2: Build the tab (replaces the Task 6 stub)**

```csharp
	private void BuildPick(RectTransform body)
	{
		RectTransform presets = AddRow((Transform)(object)body, (TextAnchor)4);
		string[] names = PickPresetNames;
		foreach (string name in names)
		{
			string captured = name;
			_pickPresetButtons.Add(AddFlexButton((Transform)(object)presets, captured, (UnityAction)delegate
			{
				StagePickPreset(captured);
			}, 108f));
		}
		AddLayoutText((Transform)(object)body, "Auto-pickup takes only the items listed. Pressing E still picks up anything.", 13, new Color(0.9f, 0.86f, 0.75f, 1f));
		_pickSearch = AddField((Transform)(object)body, "Search an item to add", 0f);
		Image searchImage = ((Component)_pickSearch).GetComponent<Image>();
		if (searchImage != null)
		{
			((Graphic)searchImage).color = Color.black;
		}
		((UnityEvent<string>)(object)_pickSearch.onValueChanged).AddListener((UnityAction<string>)delegate
		{
			PaintPickDropdown();
		});
		_pickHeader = AddLayoutText((Transform)(object)body, string.Empty, 15, Color.white);
		_pickTable = AddScrollList((Transform)(object)body, alwaysShowBar: true, fasterItemScroll: true);
		RectTransform applyRow = AddRow((Transform)(object)body, (TextAnchor)4);
		_applyPick = AddFlexButton((Transform)(object)applyRow, "Apply pick filter", new UnityAction(ApplyPick), 200f);
		BuildPickDropdown();
		PaintPick();
	}

	private void BuildPickDropdown()
	{
		_pickDropdown = new GameObject("PickDropdown", new Type[4]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(VerticalLayoutGroup),
			typeof(ContentSizeFitter)
		});
		_pickDropdown.transform.SetParent(_root.transform, false);
		_pickDropdownRect = _pickDropdown.GetComponent<RectTransform>();
		_pickDropdownRect.anchorMin = Center;
		_pickDropdownRect.anchorMax = Center;
		_pickDropdownRect.pivot = new Vector2(0f, 1f);
		Image image = _pickDropdown.GetComponent<Image>();
		((Graphic)image).color = new Color(0.08f, 0.06f, 0.04f, 0.97f);
		((Graphic)image).raycastTarget = true;
		VerticalLayoutGroup layout = _pickDropdown.GetComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)layout).spacing = 2f;
		((LayoutGroup)layout).padding = new RectOffset(4, 4, 4, 4);
		((HorizontalOrVerticalLayoutGroup)layout).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)layout).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)layout).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)layout).childForceExpandHeight = false;
		ContentSizeFitter fitter = _pickDropdown.GetComponent<ContentSizeFitter>();
		fitter.horizontalFit = (FitMode)0;
		fitter.verticalFit = (FitMode)2;
		_pickDropdown.SetActive(false);
	}
```

- [ ] **Step 3: Staging, painting, applying**

```csharp
	private void LoadPick()
	{
		_pickStaged = PickFilterService.AppliedForView();
		LogDroppedPrefabs();
		PaintPick();
		PluginStorage.Debug("Pick readout " + PickFilter.MatchPreset(_pickStaged, ItemCatalog.Contains) + ", rows " + _pickRows + ", staged " + _pickStaged.Items.Count + ".");
	}

	private void LogDroppedPrefabs()
	{
		foreach (string prefab in PickFilter.AllPresetItems().Concat(PickFilterService.State.Applied.Items))
		{
			if (!ItemCatalog.Contains(prefab) && _pickDroppedLogged.Add(prefab))
			{
				PluginStorage.Debug("Pick filter dropped " + prefab + ": not in this game.");
			}
		}
	}

	private void StagePickPreset(string name)
	{
		if (name == PickFilter.Custom)
		{
			IReadOnlyList<string> custom = PickFilterService.State.Custom;
			if (custom == null)
			{
				SetStatus(PickFilter.NoCustomMessage);
				return;
			}
			_pickStaged = new PickFilterDraft(true, custom).Known(ItemCatalog.Contains);
		}
		else if (name == PickFilter.PickAll)
		{
			_pickStaged = PickFilterDraft.PickAll;
		}
		else
		{
			_pickStaged = new PickFilterDraft(true, PickFilter.Preset(name, ItemCatalog.Contains));
		}
		HideDropdown();
		PaintPick();
	}

	private void AddPickItem(string prefab)
	{
		_pickStaged = _pickStaged.Add(prefab);
		HideDropdown();
		PaintPick();
	}

	private void RemovePickItem(string prefab)
	{
		_pickStaged = _pickStaged.Remove(prefab);
		PaintPick();
		if (!PickFilter.CanApply(_pickStaged))
		{
			SetStatus(PickFilter.EmptyListMessage);
		}
	}

	private void ApplyPick()
	{
		ActionResult<PickFilterDraft> result = PickFilterService.Apply(_pickStaged);
		if (!result.Ok)
		{
			ShowFailure(result);
			return;
		}
		_pickStaged = result.Data;
		PaintPick();
		SetStatus(PickFilter.LogLine(result.Data, ItemCatalog.Contains));
	}

	private void PaintPick()
	{
		if (_pickPresetButtons.Count == 0)
		{
			return;
		}
		string lit = PickFilter.MatchPreset(_pickStaged, ItemCatalog.Contains);
		for (int i = 0; i < _pickPresetButtons.Count && i < PickPresetNames.Length; i++)
		{
			PaintSelectable((Selectable)(object)_pickPresetButtons[i], PickPresetNames[i] == lit);
		}
		if (_pickHeader != null)
		{
			_pickHeader.text = _pickStaged.On ? lit + ", " + PickFilter.CountText(_pickStaged.Items.Count) : "Pick all, vanilla auto-pickup";
		}
		_pickRows = 0;
		if (_pickTable != null)
		{
			Clear(_pickTable);
			List<string> ordered = new List<string>(_pickStaged.Items);
			ordered.Sort((a, b) => string.Compare(ItemCatalog.LabelFor(a), ItemCatalog.LabelFor(b), StringComparison.OrdinalIgnoreCase));
			foreach (string prefab in ordered)
			{
				string captured = prefab;
				RectTransform row = AddRow((Transform)(object)_pickTable, (TextAnchor)3);
				LayoutElement rowLayout = ((Component)row).GetComponent<LayoutElement>();
				rowLayout.preferredHeight = 30f;
				rowLayout.minHeight = 30f;
				Text label = AddLayoutText((Transform)(object)row, ItemCatalog.LabelFor(captured), 15, Color.white);
				label.alignment = (TextAnchor)3;
				AddFlexButton((Transform)(object)row, "Remove", (UnityAction)delegate
				{
					RemovePickItem(captured);
				}, 84f);
				_pickRows++;
			}
		}
		if (_applyPick != null)
		{
			((Selectable)_applyPick).interactable = PickFilter.CanApply(_pickStaged) && !_pickStaged.SameAs(PickFilterService.AppliedForView());
		}
		PaintPickDropdown();
	}

	private void PaintPickDropdown()
	{
		if (_pickDropdown == null || _pickSearch == null || _pickDropdownRect == null)
		{
			return;
		}
		string text = _pickSearch.text;
		if (string.IsNullOrWhiteSpace(text) || !PickPageOpen())
		{
			_pickDropdown.SetActive(false);
			return;
		}
		Clear(_pickDropdownRect);
		IReadOnlyList<SpawnableItem> matches = ItemCatalog.Search(text, 8, _pickStaged.Contains);
		if (matches.Count == 0)
		{
			_pickDropdown.SetActive(false);
			return;
		}
		foreach (SpawnableItem match in matches)
		{
			SpawnableItem captured = match;
			Button row = AddRowButton(_pickDropdownRect, captured.Label);
			((UnityEvent)row.onClick).AddListener((UnityAction)delegate
			{
				AddPickItem(captured.Prefab);
			});
		}
		RectTransform field = ((Component)_pickSearch).GetComponent<RectTransform>();
		Vector3[] corners = new Vector3[4];
		field.GetWorldCorners(corners);
		Vector3 bottomLeft = _root.transform.InverseTransformPoint(corners[0]);
		Vector3 bottomRight = _root.transform.InverseTransformPoint(corners[3]);
		_pickDropdownRect.anchoredPosition = new Vector2(bottomLeft.x, bottomLeft.y);
		_pickDropdownRect.sizeDelta = new Vector2(bottomRight.x - bottomLeft.x, _pickDropdownRect.sizeDelta.y);
		_pickDropdown.SetActive(true);
		_pickDropdown.transform.SetAsLastSibling();
	}

	/// <summary>Closes the search dropdown. True when one was open, so Esc stops there instead of closing the panel.</summary>
	public bool HideDropdown()
	{
		if (_pickDropdown == null || !_pickDropdown.activeSelf)
		{
			return false;
		}
		_pickDropdown.SetActive(false);
		if (_pickSearch != null)
		{
			_pickSearch.SetTextWithoutNotify(string.Empty);
		}
		return true;
	}

	private bool PickPageOpen()
	{
		return IsVisible && _tab >= 0 && _tab < _pageNames.Length && _pageNames[_tab] == "Pick";
	}
```

`GetWorldCorners` fills `[0]` bottom-left, `[3]` bottom-right. The root panel is anchored and pivoted at center, so a local point equals the anchored position for anchors at `Center`. Add `using System.Linq;` at the top of the file for `Concat`.

- [ ] **Step 4: Tab hooks**

In `ShowTab`, after `bool flag = _pageNames[_tab] == "World" && _pageNames[index] != "World";` add:

```csharp
		bool leavingPick = _pageNames[_tab] == "Pick" && _pageNames[index] != "Pick";
```

After `if (flag) { _staged = _applied; }` add:

```csharp
		if (leavingPick)
		{
			_pickStaged = PickFilterService.AppliedForView();
			HideDropdown();
		}
```

At the end of `ShowTab`, after the `if (text == "World") { ... }` block, add:

```csharp
		if (text == "Pick")
		{
			try
			{
				LoadPick();
			}
			catch (Exception ex3)
			{
				Logger.LogWarning((object)("QoL pick tab failed: " + ex3));
			}
		}
```

In `SetVisible`, inside `if (_root != null)`, before `_root.SetActive(visible);` add:

```csharp
			if (!visible)
			{
				HideDropdown();
			}
```

- [ ] **Step 5: Build**

Run: `dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 6: Commit**

```powershell
git add src/ValheimQoLCM/ConsoleView.cs
git commit -m "Spec 009: Pick tab with presets, search dropdown, table, and Apply."
```

---

### Task 8: Version 1.9 and documents

**Files:**
- Modify: `src/ValheimQoLCM/Plugin.cs:18,26,73`
- Modify: `src/ValheimQoLCM/ConsoleView.cs` (the `"Version 1.8"` header text)
- Modify: `src/ValheimQoLCM/ValheimQoLCM.csproj` (`Version`, `InformationalVersion`, `AssemblyVersion`, `FileVersion`)
- Modify: `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj` (`Version`, `InformationalVersion`)
- Modify: `README.md`, `docs/USAGE.md`, `AGENTS.md`

- [ ] **Step 1: Version strings**

`Plugin.cs`: `[BepInPlugin("valheim.qol.cm", "Valheim QoL CM", "1.9")]`, `public const string Version = "1.9";`, `Logger.LogInfo((object)"Valheim QoL CM 1.9 loaded.");`
`ConsoleView.cs`: `AddText("Version 1.9", ...)`.
`ValheimQoLCM.csproj`: `<Version>1.9.0</Version>`, `<InformationalVersion>1.9</InformationalVersion>`, `<AssemblyVersion>1.9.0.0</AssemblyVersion>`, `<FileVersion>1.9.0.0</FileVersion>`.
`ValheimQoLCM.Core.csproj`: `<Version>1.9.0</Version>`, `<InformationalVersion>1.9</InformationalVersion>`.

- [ ] **Step 2: README**

Line 5: replace the sentence with: `**Version 1.9** is spec 009, frozen. Every player with the plugin gets the Pick tab, which limits what auto-pickup takes to a Woodcutting, Mining, Farming, or custom list; admins keep the Cheats, World, Spawn, Players, and Log tabs. Version 1.5 remains the last GitHub release. Earlier specs stay at 1.8, 1.7, 1.6, 0.4, 0.3, 0.2, and 0.1.`
Line 25: `**Version 1.8**` → `**Version 1.9**`, and add after "The panel opens on the Cheats tab.": `A player who is not an admin opens the same panel and sees only the Pick tab.`
Line 47: `**1.8**, spec 008` → `**1.9**, spec 009`.
Add before `### 1.8` in the changelog:

```markdown
### 1.9

- Pick tab, open to every player with the plugin. Auto-pickup takes only the listed items; pressing E still picks up anything.
- Presets Woodcutting, Mining, Farming, Custom, and Pick all. The lit button shows the filter in force, including after relog.
- A search field with a dropdown adds items; each row has Remove. Apply pick filter writes `qol-cm-pick-filter.txt` beside the plugin.
- Admins see six tabs: Cheats, World, Spawn, Players, Pick, and Log.
- The version string is `1.9`.
```

- [ ] **Step 3: USAGE**

Line 1: `# Valheim QoL CM 1.9`. Line 27: replace `Join a world as an admin and press` with `Join a world and press`; replace both `1.8` with `1.9`; add the sentence `Everyone with the plugin sees the Pick tab. Admins also see Cheats, World, Spawn, Players, and Log.`
Under `## Tabs`, update the tab list to six and add before `### Log`:

```markdown
### Pick

For every player. Auto-pickup takes only the items in the table. Pressing E on a drop still picks it up.

- **Woodcutting**, **Mining**, **Farming** stage a list of that activity's resources. **Custom** stages the last list you applied that was not a preset. **Pick all** turns the filter off.
- The lit button is the list in the table. On open it shows the filter in force, so after relog or in a new world it reads what you last applied; vanilla lights Pick all.
- Type in the search field to see up to eight matching items; click one to add it. Each row has **Remove**.
- **Apply pick filter** lights when the table differs from the filter in force. An empty table cannot be applied; add an item or choose Pick all.
- Leaving the tab or closing the panel discards unapplied changes.
```

Under `## Files beside the plugin` add: `` `qol-cm-pick-filter.txt`: the applied pick filter and the saved Custom list. `qol-cm.log` records each apply as `Pick filter: <preset>, <count> items.` or `Pick filter: Pick all.` ``

- [ ] **Step 4: Plan pointer**

`AGENTS.md` already points at `specs/009-valheim-qol-cm/plan.md` (moved in the plan commit). Confirm with `Select-String -Path AGENTS.md -Pattern "009"`.

- [ ] **Step 5: Gate**

```powershell
dotnet build ValheimQoLCM.sln -warnaserror --nologo --verbosity quiet
dotnet test ValheimQoLCM.sln --nologo --verbosity quiet
git add -A
git diff --cached master | Select-String '^\+' | Select-String '[A-Z]:\\|OneDrive|DESKTOP-|192\.168\.|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,}' | Where-Object { $_.Line -notmatch 'github\.com/LuisbatistaOT/Valheim-QoL-CM' }
git diff --cached master | Select-String '^\+' | Select-String ([Environment]::UserName)
Select-String -Path README.md,docs\USAGE.md,src\ValheimQoLCM\*.csproj,src\ValheimQoLCM.Core\*.csproj,src\ValheimQoLCM\Plugin.cs,src\ValheimQoLCM\ConsoleView.cs -Pattern '1\.8' | Where-Object { $_.Line -notmatch 'changelog|### 1\.8|stay at|spec 008|Spec 008|Specs 00' }
```

Expected: `0 Error(s)`, `Passed! ... Passed: 39`, no security hits, and the last command prints only lines that intentionally keep `1.8` (changelog heading, "stay at" sentence).

- [ ] **Step 6: Commit**

```powershell
git commit -m "Spec 009: version 1.9, README and USAGE Pick section, plan pointer."
```

---

### Task 9: Play check and freeze

**Files:**
- Modify: `specs/009-valheim-qol-cm/issues.md`, `status.md`, `tasks.md`, `spec.md` (Status: frozen), `tasks/active_context.md`, `tasks/progress.md`, `human-issues.md`, Obsidian `lessons-learned.md`.

- [ ] **Step 1: Install the build**

```powershell
$props = Get-Content src\ValheimQoLCM\Valheim.local.props -Raw
$dir = [regex]::Match($props, '<ValheimDir>(.*?)</ValheimDir>').Groups[1].Value
$dest = Join-Path $dir "BepInEx\plugins\ValheimQoLCM"
Copy-Item src\ValheimQoLCM\bin\Debug\net472\ValheimQoLCM.dll,src\ValheimQoLCM\bin\Debug\net472\ValheimQoLCM.pdb,src\ValheimQoLCM\bin\Debug\net472\ValheimQoLCM.Core.dll,src\ValheimQoLCM\bin\Debug\net472\ValheimQoLCM.Core.pdb $dest
Get-ChildItem $dest | Select-Object Name, LastWriteTime
```

Expected: four files with today's time. Never print `$dir` into a committed file.

- [ ] **Step 2: Play, one claim per check**

In the local world, with `qol-cm-debug.log` open beside the game:

1. Start: debug log has `Pick filter loaded: Pick all, 0 items, custom none.` (first run) and BepInEx log has `Valheim QoL CM 1.9 loaded.`
2. Open the panel as the host (admin): six tabs, `Version 1.9`. Pick tab: Pick all lit, header `Pick all, vanilla auto-pickup`, empty table, Apply gray. Debug: `Pick readout Pick all, rows 0, staged 0.`
3. Click Mining: Mining lit, table shows the mining items by translated name, header `Mining, N items`, Apply lit. Debug lines `Pick filter dropped <prefab>` name any Ashlands prefab this game lacks; record them in `issues.md` and fix the list in `PickFilter.cs` and the spec if a name is wrong.
4. Click Remove on Stone: Custom lit, header `Custom, N-1 items`.
5. Type `fea` in search: dropdown with Feathers under the field, over the table. Click it: dropdown closes, Feathers in the table. Press Esc with text typed: dropdown closes, panel stays.
6. Apply: status `Pick filter: Custom, N items.`, Apply gray, `qol-cm.log` has the same line, `qol-cm-pick-filter.txt` has `mode list`, `list ...`, `custom ...`.
7. Walk over a feather and a stone: feather is picked up, stone stays on the ground and is not pulled toward you. Press E on the stone: picked up.
8. Click Woodcutting, Apply; then click Custom: the Stone-less list returns. Pick all, Apply: file has `mode all` and the `custom` line.
9. Quit to menu, log back in, open Pick: the applied preset is lit and the table matches. Debug `Pick readout` rows equal staged.
10. Close the panel, switch tabs away and back: unapplied changes are discarded.
11. Non-admin: on a world where this character is not admin (or temporarily remove the host from `adminlist.txt`, then restore it), the hotkey opens the panel with Pick alone and no admin tab; the panel does not close by itself after one second.

Record each defect in `issues.md` with the REQ it cites. Fix, rebuild, copy, re-check only the failed claim.

- [ ] **Step 3: Freeze**

`spec.md`: `Status: frozen`, add `Frozen: <date>`. `status.md`: fill the Gate section with the final `dotnet build` and `dotnet test` output and the play date. `tasks.md`: tick what passed. `tasks/active_context.md` and `tasks/progress.md`: spec 009 frozen at 1.9, next steps. `human-issues.md`: one entry for the cycle. Obsidian `lessons-learned.md`: a `## Spec 009 — version 1.9` section with what the play check taught.

- [ ] **Step 4: Gate and commit**

Run the Task 8 Step 5 gate again. Then:

```powershell
git add -A
git commit -m "Freeze spec 009 at 1.9: Pick filter."
git push origin spec-009-pick-filter
```

Merging into `master` is a separate decision for the maintainer; the push gate runs again at that point.
