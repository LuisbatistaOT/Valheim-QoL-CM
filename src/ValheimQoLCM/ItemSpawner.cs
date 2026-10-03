using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Spawns vanilla item prefabs at a connected player's feet.</summary>
public static class ItemSpawner
{
    /// <summary>Returns up to eight item prefab names that contain the filter.</summary>
    public static IReadOnlyList<string> FindItems(string filter)
    {
        var matches = new List<string>();
        if (ObjectDB.instance == null)
        {
            return matches;
        }

        var needle = string.IsNullOrWhiteSpace(filter) ? string.Empty : filter.Trim();
        foreach (var prefab in ObjectDB.instance.m_items)
        {
            if (prefab == null || prefab.GetComponent<ItemDrop>() == null)
            {
                continue;
            }

            var name = prefab.name;
            if (needle.Length == 0 || name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matches.Add(name);
            }

            if (matches.Count == 8)
            {
                break;
            }
        }

        return matches;
    }

    /// <summary>Vanilla maximum quality for a prefab. Unknown items return 0.</summary>
    public static int MaxQuality(string prefabName)
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
    public static ActionResult<SpawnOrder> RequestSpawn(string playerName, string prefab, int quantity, int quality)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<SpawnOrder>.Fail("Admins only.");
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
            Plugin.Reply(sender, result.Error);
            return;
        }

        var maxStack = prototype.m_itemData.m_shared.m_maxStackSize;
        if (maxStack < 1)
        {
            maxStack = 1;
        }

        var remaining = result.Data.Quantity;
        var place = feet + Vector3.up;
        while (remaining > 0)
        {
            var pile = remaining < maxStack ? remaining : maxStack;
            var spawnedObject = Object.Instantiate(prefab, place, Quaternion.identity);
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

        Plugin.Reply(sender, "Spawned " + result.Data.Quantity + " " + result.Data.Prefab + ".");
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
