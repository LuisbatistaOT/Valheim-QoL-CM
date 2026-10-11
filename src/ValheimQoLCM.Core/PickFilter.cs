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
		return isKnown == null ? Array.AsReadOnly(items) : items.Where(isKnown).ToList();
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

	/// <summary>
	/// The draft a preset button stages. Custom stages the saved Custom list and fails with
	/// <see cref="NoCustomMessage"/> when none is saved or when <paramref name="isKnown"/> rejects every saved item.
	/// Pick all stages the filter off. Unknown names fail.
	/// </summary>
	public static ActionResult<PickFilterDraft> Stage(string name, IReadOnlyList<string>? custom, Func<string, bool>? isKnown)
	{
		switch (name)
		{
		case Custom:
			if (custom == null || custom.Count == 0)
			{
				return ActionResult<PickFilterDraft>.Fail(NoCustomMessage);
			}
			PickFilterDraft customDraft = isKnown == null ? new PickFilterDraft(true, custom) : new PickFilterDraft(true, custom).Known(isKnown);
			if (isKnown != null && customDraft.Items.Count == 0)
			{
				return ActionResult<PickFilterDraft>.Fail(NoCustomMessage);
			}
			return ActionResult<PickFilterDraft>.Success(customDraft);
		case PickAll:
			return ActionResult<PickFilterDraft>.Success(PickFilterDraft.PickAll);
		case Woodcutting:
		case Mining:
		case Farming:
			return ActionResult<PickFilterDraft>.Success(new PickFilterDraft(true, Preset(name, isKnown)));
		default:
			return ActionResult<PickFilterDraft>.Fail("Unknown preset.");
		}
	}

	/// <summary>Apply lights only for an applicable staged filter that differs from the applied one.</summary>
	public static bool ApplyEnabled(PickFilterDraft staged, PickFilterDraft applied)
	{
		return CanApply(staged) && !staged.SameAs(applied);
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
