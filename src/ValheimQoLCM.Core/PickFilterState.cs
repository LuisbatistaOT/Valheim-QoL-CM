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
