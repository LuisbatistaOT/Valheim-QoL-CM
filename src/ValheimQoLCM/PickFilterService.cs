using System;
using System.Collections.Generic;
using System.Linq;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Holds the applied pick filter on this machine, saves it, and answers the auto-pickup patch.</summary>
public static class PickFilterService
{
	private static readonly HashSet<string> DroppedLogged = new HashSet<string>(StringComparer.Ordinal);

	/// <summary>Applied filter and saved Custom list. Starts as Pick all until <see cref="Load"/> runs.</summary>
	public static PickFilterState State { get; private set; } = PickFilterState.Default;

	/// <summary>Read <c>qol-cm-pick-filter.txt</c>. Called once from <c>Plugin.Awake</c>.</summary>
	public static void Load()
	{
		State = PickFilterState.Parse(PluginStorage.ReadPickFilter());
		string custom = State.Custom == null ? "none" : PickFilter.CountText(State.Custom.Count);
		// MatchPreset without isKnown: ObjectDB is not loaded during Awake, so the catalog cannot filter yet.
		PluginStorage.Debug("Pick filter loaded: " + PickFilter.MatchPreset(State.Applied) + ", " + PickFilter.CountText(State.Applied.Items.Count) + ", custom " + custom + ".");
	}

	/// <summary>One debug line per preset or applied prefab this game lacks. Safe to call again; each name logs once.</summary>
	public static void LogDropped()
	{
		if (!ItemCatalog.IsLoaded)
		{
			return;
		}
		foreach (string prefab in PickFilter.AllPresetItems().Concat(State.Applied.Items))
		{
			if (!ItemCatalog.Contains(prefab) && DroppedLogged.Add(prefab))
			{
				PluginStorage.Debug("Pick filter dropped " + prefab + ": not in this game.");
			}
		}
	}

	/// <summary>True when auto-pickup may take this prefab.</summary>
	public static bool Allows(string? prefab)
	{
		return PickFilter.Allows(State.Applied, prefab);
	}

	/// <summary>The applied filter with prefabs this game lacks left out. The Pick tab stages a copy of this.</summary>
	public static PickFilterDraft AppliedForView()
	{
		return ItemCatalog.IsLoaded ? State.Applied.Known(ItemCatalog.Contains) : State.Applied;
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
