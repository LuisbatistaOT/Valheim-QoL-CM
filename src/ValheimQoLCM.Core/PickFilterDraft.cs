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
