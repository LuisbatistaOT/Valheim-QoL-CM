using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>One item the panel can spawn. The label is the in-game name.</summary>
public sealed class SpawnableItem
{
    /// <summary>Creates a catalog row.</summary>
    public SpawnableItem(string prefab, string label, int maxStack, int maxQuality)
    {
        Prefab = prefab;
        Label = label;
        MaxStack = maxStack;
        MaxQuality = maxQuality;
    }

    /// <summary>Vanilla prefab name passed to the host.</summary>
    public string Prefab { get; }

    /// <summary>Localized name shown in the list.</summary>
    public string Label { get; }

    /// <summary>Vanilla stack size for this item.</summary>
    public int MaxStack { get; }

    /// <summary>Vanilla maximum quality for this item.</summary>
    public int MaxQuality { get; }
}

/// <summary>Spawns vanilla item prefabs at a connected player's feet.</summary>
public static class ItemSpawner
{
    private static readonly List<CatalogEntry> Catalog = new List<CatalogEntry>();

    /// <summary>Reads ObjectDB after it has loaded items and localization.</summary>
    public static void RebuildCatalog()
    {
        Catalog.Clear();
        if (ObjectDB.instance == null)
        {
            return;
        }

        foreach (var prefab in ObjectDB.instance.m_items)
        {
            if (prefab == null)
            {
                continue;
            }

            var drop = prefab.GetComponent<ItemDrop>();
            if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
            {
                continue;
            }

            var registered = ObjectDB.instance.GetItemPrefab(prefab.name);
            if (registered != prefab)
            {
                continue;
            }

            var shared = drop.m_itemData.m_shared;
            var icons = shared.m_icons;
            var iconCount = icons != null ? icons.Length : 0;
            if (!ItemListing.CanPickUp(iconCount))
            {
                continue;
            }

            var stack = shared.m_maxStackSize;
            if (stack < 1)
            {
                stack = 1;
            }

            var quality = shared.m_maxQuality;
            if (quality < 1)
            {
                quality = 1;
            }

            Catalog.Add(new CatalogEntry(prefab.name, shared.m_name, stack, quality));
        }

        ApplyRowLabels();
    }

    private static void ApplyRowLabels()
    {
        var sharedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var localized = new string[Catalog.Count];
        for (var i = 0; i < Catalog.Count; i++)
        {
            localized[i] = Localize(Catalog[i].Token, Catalog[i].Prefab);
            var baseName = ItemListing.BaseName(localized[i], Catalog[i].Prefab);
            sharedNames.TryGetValue(baseName, out var count);
            sharedNames[baseName] = count + 1;
        }

        for (var i = 0; i < Catalog.Count; i++)
        {
            var baseName = ItemListing.BaseName(localized[i], Catalog[i].Prefab);
            Catalog[i].Label = ItemListing.RowLabel(localized[i], Catalog[i].Prefab, sharedNames[baseName] > 1);
        }
    }

    /// <summary>Returns pickupable items whose in-game name, prefab, or token contains the filter.</summary>
    public static IReadOnlyList<SpawnableItem> FindItems(string filter)
    {
        RebuildCatalog();
        var needle = string.IsNullOrWhiteSpace(filter) ? string.Empty : filter.Trim();
        var matches = new List<SpawnableItem>();
        foreach (var entry in Catalog)
        {
            if (needle.Length != 0
                && entry.Label.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0
                && entry.Prefab.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0
                && (entry.Token == null || entry.Token.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0))
            {
                continue;
            }

            matches.Add(new SpawnableItem(entry.Prefab, entry.Label, entry.MaxStack, entry.MaxQuality));
        }

        matches.Sort((left, right) => string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase));
        return matches;
    }

    /// <summary>Vanilla stack size for a prefab. Unknown items return 1.</summary>
    public static int MaxStack(string? prefabName)
    {
        if (Catalog.Count == 0)
        {
            RebuildCatalog();
        }

        foreach (var entry in Catalog)
        {
            if (entry.Prefab == prefabName)
            {
                return entry.MaxStack;
            }
        }

        return 1;
    }

    /// <summary>Vanilla maximum quality for a prefab. Unknown items return 0.</summary>
    public static int MaxQuality(string? prefabName)
    {
        var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
        var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
        if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
        {
            return 0;
        }

        return drop.m_itemData.m_shared.m_maxQuality;
    }

    /// <summary>Asks the host to spawn an item for a connected player.</summary>
    public static ActionResult<SpawnOrder> RequestSpawn(string? playerName, string? prefab, int quantity, int quality)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<SpawnOrder>.Fail("Admins only.");
        }

        if (string.IsNullOrEmpty(playerName))
        {
            foreach (var row in AdminCommands.ListConnected())
            {
                if (row.IsSelf)
                {
                    playerName = row.Name;
                    break;
                }
            }
        }

        var exists = false;
        foreach (var row in AdminCommands.ListConnected())
        {
            if (row.Name == playerName)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            return ActionResult<SpawnOrder>.Fail("Player is not connected.");
        }

        var maxQuality = MaxQuality(prefab);
        var result = SpawnValidation.Validate(prefab, quantity, quality, maxQuality, maxQuality > 0);
        if (!result.Ok)
        {
            return result;
        }

        var order = result.Data;
        Plugin.Send("spawn", package =>
        {
            package.Write(playerName);
            package.Write(order.Prefab);
            package.Write(order.Quantity);
            package.Write(order.Quality);
        });
        return result;
    }

    /// <summary>Host-side spawn. Invalid requests create nothing.</summary>
    public static void ApplySpawn(long sender, string playerName, string prefabName, int quantity, int quality)
    {
        if (!TryFeet(playerName, out var feet))
        {
            Plugin.Reply(sender, "Player is not connected.");
            return;
        }

        var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
        var prototype = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
        var maxQuality = prototype != null && prototype.m_itemData != null && prototype.m_itemData.m_shared != null
            ? prototype.m_itemData.m_shared.m_maxQuality
            : 0;
        var result = SpawnValidation.Validate(prefabName, quantity, quality, maxQuality, prototype != null);
        if (!result.Ok)
        {
            Plugin.Reply(sender, result.Error ?? "Unknown item.");
            return;
        }

        var itemData = prototype != null ? prototype.m_itemData : null;
        var shared = itemData != null ? itemData.m_shared : null;
        if (prefab == null || shared == null)
        {
            Plugin.Reply(sender, "Unknown item.");
            return;
        }

        var maxStack = shared.m_maxStackSize;
        if (maxStack < 1)
        {
            maxStack = 1;
        }

        var remaining = result.Data.Quantity;
        var place = feet + Vector3.up;
        while (remaining > 0)
        {
            var pile = remaining < maxStack ? remaining : maxStack;
            var spawnedObject = UnityEngine.Object.Instantiate(prefab, place, Quaternion.identity);
            var drop = spawnedObject.GetComponent<ItemDrop>();
            if (drop != null && drop.m_itemData != null)
            {
                drop.m_itemData.m_stack = pile;
                drop.m_itemData.m_quality = result.Data.Quality;
                drop.m_itemData.m_durability = drop.m_itemData.GetMaxDurability();
            }

            remaining -= pile;
            place += Vector3.right * 0.35f;
        }

        Plugin.Reply(sender, "Spawned " + result.Data.Quantity + " " + LabelFor(result.Data.Prefab) + ".");
    }

    private static string LabelFor(string prefabName)
    {
        if (Catalog.Count == 0)
        {
            RebuildCatalog();
        }

        foreach (var entry in Catalog)
        {
            if (entry.Prefab == prefabName)
            {
                return entry.Label;
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

        var text = Localization.instance.Localize(token);
        if (string.IsNullOrEmpty(text) || text[0] == '$')
        {
            return prefab;
        }

        return text;
    }

    private sealed class CatalogEntry
    {
        public CatalogEntry(string prefab, string token, int maxStack, int maxQuality)
        {
            Prefab = prefab;
            Token = token;
            Label = prefab;
            MaxStack = maxStack;
            MaxQuality = maxQuality;
        }

        public string Prefab { get; }

        public string Token { get; }

        public string Label { get; set; }

        public int MaxStack { get; }

        public int MaxQuality { get; }
    }

    private static bool TryFeet(string playerName, out Vector3 position)
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

/// <summary>Fills the item catalog once ObjectDB has loaded prefabs.</summary>
[HarmonyPatch(typeof(ObjectDB), "Awake")]
internal static class ItemCatalogPatch
{
    [HarmonyPostfix]
    private static void AfterItemsLoad()
    {
        ItemSpawner.RebuildCatalog();
    }
}
