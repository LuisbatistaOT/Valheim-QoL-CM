using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>BepInEx entry point for Valheim QoL CM 1.5.</summary>
[BepInPlugin(Guid, Name, Version)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    /// <summary>Plugin GUID. Also the BepInEx config file name.</summary>
    public const string Guid = "valheim.qol.cm";

    /// <summary>Window title.</summary>
    public const string Name = "Valheim QoL CM";

    /// <summary>Spec 005 version. V1 is confirmed. The string is 1.5 so BepInEx shows the same number.</summary>
    public const string Version = "1.5";

    private const string BringMe = "bring-me";
    private const string BringThem = "bring-them";
    private const string Spawn = "spawn";
    private const string Grant = "grant";
    private const string Percent = "percent";
    private const string Tame = "tame";
    private const string KillEnemies = "kill-enemies";
    private const string Status = "status";
    private const string Teleport = "teleport";

    private static CustomRPC _actions = null!;
    private static ConfigEntry<float> _skillLoss = null!;

    /// <summary>Admin-only config entry. The host value is the one that is stored.</summary>
    public static ConfigEntry<float> SkillLossEntry => _skillLoss;

    /// <summary>Server-synced skill-loss percent.</summary>
    public static float SkillLossPercent => _skillLoss == null ? 0f : SkillLoss.ClampPercent(_skillLoss.Value);

    private void Awake()
    {
        _skillLoss = Config.BindConfig(
            "DeathPenalty",
            "SkillLossPercent",
            5f,
            "Percent of each current skill level removed on death. 0 removes none. 100 clears skills.",
            true,
            1,
            new AcceptableValueRange<float>(SkillLoss.MinPercent, SkillLoss.MaxPercent),
            null,
            null);

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Plugin).Assembly);
        _actions = NetworkManager.Instance.AddRPC("QoLPanel", ServerReceive, ClientReceive);
        ConsoleManager.Create(this, _skillLoss);
        Logger.LogInfo(Name + " " + Version + " loaded.");
    }

    private void Update()
    {
        ConsoleManager.Tick();
    }

    /// <summary>True when Jötunn considers this player the host or a server admin.</summary>
    public static bool LocalIsAdmin()
    {
        return SynchronizationManager.Instance != null && SynchronizationManager.Instance.PlayerIsAdmin;
    }

    /// <summary>Sends a panel action to the world host.</summary>
    public static void Send(string action, Action<ZPackage> write)
    {
        if (!AdminGate.CanMutate(LocalIsAdmin()))
        {
            ConsoleManager.Show("Admins only.");
            return;
        }

        if (ZNet.instance == null)
        {
            ConsoleManager.Show("Not connected.");
            return;
        }

        var package = new ZPackage();
        package.Write(action);
        write(package);
        if (ZNet.instance.IsServer())
        {
            package.SetPos(0);
            ApplyServer(0L, package);
            return;
        }

        var server = ZNet.instance.GetServerPeer();
        if (server == null)
        {
            ConsoleManager.Show("Server peer missing.");
            return;
        }

        _actions.SendPackage(server.m_uid, package);
    }

    /// <summary>Shows a result on the requesting admin's panel.</summary>
    public static void Reply(long sender, string message)
    {
        if (sender == 0L || ZNet.instance == null || ZNet.instance.GetPeer(sender) == null)
        {
            ConsoleManager.Show(message);
            return;
        }

        var package = new ZPackage();
        package.Write(Status);
        package.Write(message);
        _actions.SendPackage(sender, package);
    }

    /// <summary>Asks one connected player to teleport. A missing peer moves the local host when the name matches.</summary>
    public static void TeleportPlayer(string playerName, Vector3 position)
    {
        var peer = ZNet.instance.GetPeerByPlayerName(playerName);
        if (peer != null)
        {
            var package = new ZPackage();
            package.Write(Teleport);
            package.Write(position);
            _actions.SendPackage(peer.m_uid, package);
            return;
        }

        var local = Player.m_localPlayer;
        if (local != null && local.GetPlayerName() == playerName)
        {
            local.TeleportTo(position, local.transform.rotation, true);
        }
    }

    private static IEnumerator ServerReceive(long sender, ZPackage package)
    {
        ApplyServer(sender, package);
        yield break;
    }

    private static IEnumerator ClientReceive(long sender, ZPackage package)
    {
        var action = package.ReadString();
        if (action == Teleport)
        {
            var position = package.ReadVector3();
            var local = Player.m_localPlayer;
            if (local != null && !local.IsDead())
            {
                local.TeleportTo(position, local.transform.rotation, true);
            }
        }
        else if (action == Status)
        {
            ConsoleManager.Show(package.ReadString());
        }

        yield break;
    }

    private static void ApplyServer(long sender, ZPackage package)
    {
        try
        {
            if (!SenderIsAdmin(sender))
            {
                Reply(sender, "Admins only.");
                return;
            }

            var action = package.ReadString();
            switch (action)
            {
                case BringMe:
                    AdminCommands.ApplyBringMe(sender, package.ReadString());
                    break;
                case BringThem:
                    AdminCommands.ApplyBringTarget(sender, package.ReadString());
                    break;
                case Spawn:
                    ItemSpawner.ApplySpawn(sender, package.ReadString(), package.ReadString(), package.ReadInt(), package.ReadInt());
                    break;
                case Grant:
                    AdminCommands.ApplyGrant(sender, package.ReadString());
                    break;
                case Percent:
                    DeathPenaltyManager.ApplyPercent(sender, package.ReadSingle());
                    break;
                case Tame:
                    NearbyCommands.ApplyTame(sender);
                    break;
                case KillEnemies:
                    NearbyCommands.ApplyKillEnemies(sender);
                    break;
                default:
                    Reply(sender, "Unknown action.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Jotunn.Logger.LogError(ex);
            Reply(sender, "Action failed.");
        }
    }

    private static bool SenderIsAdmin(long sender)
    {
        if (ZNet.instance == null)
        {
            return false;
        }

        if (sender == 0L)
        {
            return ZNet.instance.LocalPlayerIsAdminOrHost();
        }

        var peer = ZNet.instance.GetPeer(sender);
        if (peer == null)
        {
            return false;
        }

        var local = Player.m_localPlayer;
        if (local != null && peer.m_playerName == local.GetPlayerName() && ZNet.instance.LocalPlayerIsAdminOrHost())
        {
            return true;
        }

        var hostName = peer.m_socket != null ? peer.m_socket.GetHostName() : null;
        if (!string.IsNullOrEmpty(hostName) && ZNet.instance.IsAdmin(hostName))
        {
            return true;
        }

        return peer.m_playerID != 0L && ZNet.instance.IsAdmin(peer.m_playerID.ToString());
    }
}
