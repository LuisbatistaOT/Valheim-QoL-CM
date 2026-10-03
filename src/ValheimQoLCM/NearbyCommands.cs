using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Tame and kill-enemies. The host applies them at the admin's position.</summary>
public static class NearbyCommands
{
    /// <summary>Sends a tame request. A dead local character is rejected here.</summary>
    public static ActionResult<int> RequestTame()
    {
        return Request("tame");
    }

    /// <summary>Sends a kill-enemies request. A dead local character is rejected here.</summary>
    public static ActionResult<int> RequestKillEnemies()
    {
        return Request("kill-enemies");
    }

    /// <summary>Host-side tame. Uses the vanilla tame call.</summary>
    public static void ApplyTame(long sender)
    {
        if (!TryAdmin(sender, out var position))
        {
            return;
        }

        var count = 0;
        var characters = Character.GetAllCharacters();
        if (characters != null)
        {
            foreach (var character in characters)
            {
                if (character == null)
                {
                    continue;
                }

                var tameable = character.GetComponent<Tameable>() != null;
                if (NearbyActions.IsTameTarget(character.IsPlayer(), tameable))
                {
                    count++;
                }
            }
        }

        if (count > 0)
        {
            Tameable.TameAllInArea(position, NearbyActions.TameRadius);
        }

        Plugin.Reply(sender, NearbyActions.TameMessage(count));
    }

    /// <summary>Host-side kill. Same filters and hit as killenemycreatures, measured from the admin.</summary>
    public static void ApplyKillEnemies(long sender)
    {
        if (!TryAdmin(sender, out var position))
        {
            return;
        }

        var count = 0;
        var characters = Character.GetAllCharacters();
        if (characters != null)
        {
            foreach (var character in characters)
            {
                if (character == null)
                {
                    continue;
                }

                var distance = Vector3.Distance(position, character.transform.position);
                var hasPiece = character.GetComponent<Piece>() != null;
                if (!NearbyActions.IsKillTarget(character.IsPlayer(), hasPiece, character.IsTamed(), distance))
                {
                    continue;
                }

                character.Damage(new HitData(1E+10f));
                count++;
            }
        }

        Plugin.Reply(sender, NearbyActions.KillMessage(count));
    }

    private static ActionResult<int> Request(string action)
    {
        var player = Player.m_localPlayer;
        var gate = NearbyActions.Begin(Plugin.LocalIsAdmin(), player == null || player.IsDead());
        if (!gate.Ok)
        {
            return gate;
        }

        Plugin.Send(action, package => { });
        return gate;
    }

    private static bool TryAdmin(long sender, out Vector3 position)
    {
        var name = SenderName(sender);
        if (name == null)
        {
            position = Vector3.zero;
            Plugin.Reply(sender, "Admin player was not found.");
            return false;
        }

        if (!TryPosition(name, out position))
        {
            Plugin.Reply(sender, "Admin player was not found.");
            return false;
        }

        var player = FindPlayer(name);
        if (player != null && player.IsDead())
        {
            Plugin.Reply(sender, "Character is dead.");
            return false;
        }

        return true;
    }

    private static Player? FindPlayer(string playerName)
    {
        var players = Player.GetAllPlayers();
        if (players == null)
        {
            return null;
        }

        foreach (var player in players)
        {
            if (player != null && player.GetPlayerName() == playerName)
            {
                return player;
            }
        }

        return null;
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
}
