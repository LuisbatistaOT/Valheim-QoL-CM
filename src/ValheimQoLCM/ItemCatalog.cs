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
