using SharedData = ItemDrop.ItemData.SharedData;
using PlayerInfo = ZNet.PlayerInfo;
using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class ItemSpawner
{
	private sealed class CatalogEntry
	{
		public string Prefab { get; }

		public string Token { get; }

		public string Label { get; set; }

		public int MaxStack { get; }

		public int MaxQuality { get; }

		public CatalogEntry(string prefab, string token, int maxStack, int maxQuality)
		{
			Prefab = prefab;
			Token = token;
			Label = prefab;
			MaxStack = maxStack;
			MaxQuality = maxQuality;
		}
	}

	private static readonly List<CatalogEntry> Catalog = new List<CatalogEntry>();

	public static void RebuildCatalog()
	{
		Catalog.Clear();
		if ((Object)(object)ObjectDB.instance == (Object)null)
		{
			return;
		}
		foreach (GameObject item in ObjectDB.instance.m_items)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			ItemDrop component = item.GetComponent<ItemDrop>();
			if ((Object)(object)component == (Object)null || component.m_itemData == null || component.m_itemData.m_shared == null)
			{
				continue;
			}
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(((Object)item).name);
			if ((Object)(object)itemPrefab != (Object)(object)item)
			{
				continue;
			}
			SharedData shared = component.m_itemData.m_shared;
			Sprite[] icons = shared.m_icons;
			int iconCount = ((icons != null) ? icons.Length : 0);
			if (ItemListing.CanPickUp(iconCount))
			{
				int num = shared.m_maxStackSize;
				if (num < 1)
				{
					num = 1;
				}
				int num2 = shared.m_maxQuality;
				if (num2 < 1)
				{
					num2 = 1;
				}
				Catalog.Add(new CatalogEntry(((Object)item).name, shared.m_name, num, num2));
			}
		}
		ApplyRowLabels();
	}

	private static void ApplyRowLabels()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		string[] array = new string[Catalog.Count];
		for (int i = 0; i < Catalog.Count; i++)
		{
			array[i] = Localize(Catalog[i].Token, Catalog[i].Prefab);
			string key = ItemListing.BaseName(array[i], Catalog[i].Prefab);
			dictionary.TryGetValue(key, out var value);
			dictionary[key] = value + 1;
		}
		for (int j = 0; j < Catalog.Count; j++)
		{
			string key2 = ItemListing.BaseName(array[j], Catalog[j].Prefab);
			Catalog[j].Label = ItemListing.RowLabel(array[j], Catalog[j].Prefab, dictionary[key2] > 1);
		}
	}

	public static IReadOnlyList<SpawnableItem> FindItems(string filter)
	{
		RebuildCatalog();
		string text = (string.IsNullOrWhiteSpace(filter) ? string.Empty : filter.Trim());
		List<SpawnableItem> list = new List<SpawnableItem>();
		foreach (CatalogEntry item in Catalog)
		{
			if (text.Length == 0 || item.Label.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 || item.Prefab.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 || (item.Token != null && item.Token.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))
			{
				list.Add(new SpawnableItem(item.Prefab, item.Label, item.MaxStack, item.MaxQuality));
			}
		}
		list.Sort((SpawnableItem left, SpawnableItem right) => string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase));
		return list;
	}

	public static int MaxStack(string? prefabName)
	{
		if (Catalog.Count == 0)
		{
			RebuildCatalog();
		}
		foreach (CatalogEntry item in Catalog)
		{
			if (item.Prefab == prefabName)
			{
				return item.MaxStack;
			}
		}
		return 1;
	}

	public static int MaxQuality(string? prefabName)
	{
		GameObject val = (((Object)(object)ObjectDB.instance != (Object)null) ? ObjectDB.instance.GetItemPrefab(prefabName) : null);
		ItemDrop val2 = (((Object)(object)val != (Object)null) ? val.GetComponent<ItemDrop>() : null);
		if ((Object)(object)val2 == (Object)null || val2.m_itemData == null || val2.m_itemData.m_shared == null)
		{
			return 0;
		}
		return val2.m_itemData.m_shared.m_maxQuality;
	}

	public static ActionResult<SpawnOrder> RequestSpawn(string? playerName, string? prefab, int quantity, int quality)
	{
		if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
		{
			return ActionResult<SpawnOrder>.Fail("Admins only.");
		}
		if (string.IsNullOrEmpty(playerName))
		{
			foreach (ConnectedPlayer item in AdminCommands.ListConnected())
			{
				if (item.IsSelf)
				{
					playerName = item.Name;
					break;
				}
			}
		}
		bool flag = false;
		foreach (ConnectedPlayer item2 in AdminCommands.ListConnected())
		{
			if (item2.Name == playerName)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return ActionResult<SpawnOrder>.Fail("Player is not connected.");
		}
		int num = MaxQuality(prefab);
		ActionResult<SpawnOrder> actionResult = SpawnValidation.Validate(prefab, quantity, quality, num, num > 0);
		if (!actionResult.Ok)
		{
			return actionResult;
		}
		SpawnOrder order = actionResult.Data;
		Plugin.Send("spawn", delegate(ZPackage package)
		{
			package.Write(playerName);
			package.Write(order.Prefab);
			package.Write(order.Quantity);
			package.Write(order.Quality);
		});
		return actionResult;
	}

	public static void ApplySpawn(long sender, string playerName, string prefabName, int quantity, int quality)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		if (!TryFeet(playerName, out var position))
		{
			Plugin.Reply(sender, "Player is not connected.");
			return;
		}
		GameObject val = (((Object)(object)ObjectDB.instance != (Object)null) ? ObjectDB.instance.GetItemPrefab(prefabName) : null);
		ItemDrop val2 = (((Object)(object)val != (Object)null) ? val.GetComponent<ItemDrop>() : null);
		int maxQuality = (((Object)(object)val2 != (Object)null && val2.m_itemData != null && val2.m_itemData.m_shared != null) ? val2.m_itemData.m_shared.m_maxQuality : 0);
		ActionResult<SpawnOrder> actionResult = SpawnValidation.Validate(prefabName, quantity, quality, maxQuality, (Object)(object)val2 != (Object)null);
		if (!actionResult.Ok)
		{
			Plugin.Reply(sender, actionResult.Error ?? "Unknown item.");
			return;
		}
		SharedData val3 = (((Object)(object)val2 != (Object)null) ? val2.m_itemData : null)?.m_shared;
		if ((Object)(object)val == (Object)null || val3 == null)
		{
			Plugin.Reply(sender, "Unknown item.");
			return;
		}
		int num = val3.m_maxStackSize;
		if (num < 1)
		{
			num = 1;
		}
		int num2 = actionResult.Data.Quantity;
		Vector3 val4 = position + Vector3.up;
		while (num2 > 0)
		{
			int num3 = ((num2 < num) ? num2 : num);
			GameObject val5 = Object.Instantiate<GameObject>(val, val4, Quaternion.identity);
			ItemDrop component = val5.GetComponent<ItemDrop>();
			if ((Object)(object)component != (Object)null && component.m_itemData != null)
			{
				component.m_itemData.m_stack = num3;
				component.m_itemData.m_quality = actionResult.Data.Quality;
				component.m_itemData.m_durability = component.m_itemData.GetMaxDurability();
			}
			num2 -= num3;
			val4 += Vector3.right * 0.35f;
		}
		Plugin.Reply(sender, "Spawned " + actionResult.Data.Quantity + " " + LabelFor(actionResult.Data.Prefab) + ".");
	}

	private static string LabelFor(string prefabName)
	{
		if (Catalog.Count == 0)
		{
			RebuildCatalog();
		}
		foreach (CatalogEntry item in Catalog)
		{
			if (item.Prefab == prefabName)
			{
				return item.Label;
			}
		}
		return prefabName;
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

	private static bool TryFeet(string playerName, out Vector3 position)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && localPlayer.GetPlayerName() == playerName)
		{
			position = ((Component)localPlayer).transform.position;
			return true;
		}
		if ((Object)(object)ZNet.instance != (Object)null)
		{
			foreach (PlayerInfo player in ZNet.instance.GetPlayerList())
			{
				if (player.m_name == playerName)
				{
					position = player.m_position;
					return true;
				}
			}
		}
		position = Vector3.zero;
		return false;
	}
}
