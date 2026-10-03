using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>A player currently connected to the world.</summary>
public sealed class ConnectedPlayer
{
    /// <summary>Creates a connected-player row.</summary>
    public ConnectedPlayer(string name, string? steamId, bool isSelf)
    {
        Name = name;
        SteamId = steamId;
        IsSelf = isSelf;
    }

    /// <summary>Character name shown in the panel.</summary>
    public string Name { get; }

    /// <summary>SteamID64 from the live connection. Null when the session does not expose one.</summary>
    public string? SteamId { get; }

    /// <summary>True when this row is the local character.</summary>
    public bool IsSelf { get; }
}

/// <summary>Teleport and grant-admin actions. The host applies them.</summary>
public static class AdminCommands
{
    /// <summary>Lists players from the live session. Names are selected, not typed.</summary>
    public static IReadOnlyList<ConnectedPlayer> ListConnected()
    {
        var rows = new List<ConnectedPlayer>();
        if (ZNet.instance == null)
        {
            return rows;
        }

        var local = Player.m_localPlayer;
        var localName = local != null ? local.GetPlayerName() : null;
        var includedSelf = false;
        foreach (ZNet.PlayerInfo info in ZNet.instance.GetPlayerList())
        {
            if (string.IsNullOrEmpty(info.m_name))
            {
                continue;
            }

            var isSelf = !string.IsNullOrEmpty(localName) && info.m_name == localName;
            if (isSelf)
            {
                includedSelf = true;
            }

            rows.Add(new ConnectedPlayer(info.m_name, ReadSteamId(info), isSelf));
        }

        if (!includedSelf && local != null && localName is string selfName && selfName.Length > 0)
        {
            var localId = local.GetPlayerID().ToString();
            rows.Insert(0, new ConnectedPlayer(selfName, SteamId.IsWellFormed(localId) ? localId : null, true));
        }

        return rows;
    }

    /// <summary>Moves the admin to the selected connected player.</summary>
    public static ActionResult<string> RequestBringMe(string? targetName)
    {
        var selection = Select(targetName);
        if (!selection.Ok)
        {
            return selection;
        }

        Plugin.Send("bring-me", package => package.Write(selection.Data));
        return ActionResult<string>.Success(selection.Data);
    }

    /// <summary>Moves the selected connected player to the admin.</summary>
    public static ActionResult<string> RequestBringTarget(string? targetName)
    {
        var selection = Select(targetName);
        if (!selection.Ok)
        {
            return selection;
        }

        Plugin.Send("bring-them", package => package.Write(selection.Data));
        return ActionResult<string>.Success(selection.Data);
    }

    /// <summary>Grants admin using the connection Steam ID, or the typed fallback when that ID is missing.</summary>
    public static ActionResult<string> RequestGrant(string targetName, string? typedId)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<string>.Fail("Admins only.");
        }

        ConnectedPlayer? selected = null;
        foreach (var row in ListConnected())
        {
            if (row.Name == targetName)
            {
                selected = row;
                break;
            }
        }

        if (selected == null)
        {
            return ActionResult<string>.Fail("Player is not connected.");
        }

        var resolved = SteamId.Resolve(selected.SteamId, typedId);
        if (!resolved.Ok)
        {
            return resolved;
        }

        Plugin.Send("grant", package => package.Write(resolved.Data));
        return resolved;
    }

    /// <summary>Host-side move of the requesting admin to another player.</summary>
    public static void ApplyBringMe(long sender, string targetName)
    {
        if (!TryPosition(targetName, out var position))
        {
            Plugin.Reply(sender, "Player is not connected.");
            return;
        }

        var adminName = SenderName(sender);
        if (adminName == null)
        {
            Plugin.Reply(sender, "Admin player was not found.");
            return;
        }

        if (adminName == targetName)
        {
            Plugin.Reply(sender, "Select another player.");
            return;
        }

        Plugin.TeleportPlayer(adminName, position);
        Plugin.Reply(sender, "Moved you to " + targetName + ".");
    }

    /// <summary>Host-side move of another player to the requesting admin.</summary>
    public static void ApplyBringTarget(long sender, string targetName)
    {
        var adminName = SenderName(sender);
        if (adminName == null || !TryPosition(adminName, out var position))
        {
            Plugin.Reply(sender, "Admin player was not found.");
            return;
        }

        if (!TryPosition(targetName, out _))
        {
            Plugin.Reply(sender, "Player is not connected.");
            return;
        }

        if (adminName == targetName)
        {
            Plugin.Reply(sender, "Select another player.");
            return;
        }

        Plugin.TeleportPlayer(targetName, position);
        Plugin.Reply(sender, "Moved " + targetName + " to you.");
    }

    /// <summary>Host-side add of a Steam ID to the vanilla admin list.</summary>
    public static void ApplyGrant(long sender, string steamId)
    {
        if (!SteamId.IsWellFormed(steamId))
        {
            Plugin.Reply(sender, "Steam ID is not valid.");
            return;
        }

        var list = ZNet.instance != null ? ZNet.instance.m_adminList : null;
        if (list == null)
        {
            Plugin.Reply(sender, "Admin list is unavailable.");
            return;
        }

        if (list.Contains(steamId))
        {
            Plugin.Reply(sender, steamId + " is already an admin.");
            return;
        }

        list.Add(steamId);
        list.Save();
        Plugin.Reply(sender, "Granted admin to " + steamId + ".");
    }

    private static ActionResult<string> Select(string? targetName)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<string>.Fail("Admins only.");
        }

        var connected = false;
        var isSelf = false;
        foreach (var row in ListConnected())
        {
            if (row.Name != targetName)
            {
                continue;
            }

            connected = true;
            isSelf = row.IsSelf;
            break;
        }

        return TeleportSelection.Select(targetName, connected, isSelf);
    }

    private static string? ReadSteamId(ZNet.PlayerInfo info)
    {
        var raw = info.m_userInfo.m_id.m_userID;
        if (SteamId.IsWellFormed(raw))
        {
            return raw.Trim();
        }

        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        var digits = string.Empty;
        foreach (var character in raw)
        {
            if (character >= '0' && character <= '9')
            {
                digits += character;
            }
        }

        return SteamId.IsWellFormed(digits) ? digits : null;
    }

    private static bool TryPosition(string playerName, out Vector3 position)
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

    private static string? SenderName(long sender)
    {
        if (sender == 0L)
        {
            return Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerName() : null;
        }

        var peer = ZNet.instance != null ? ZNet.instance.GetPeer(sender) : null;
        return peer != null ? peer.m_playerName : null;
    }
}
