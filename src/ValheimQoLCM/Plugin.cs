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

/// <summary>BepInEx entry point for Valheim QoL CM 1.6.</summary>
[BepInPlugin(Guid, Name, Version)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    /// <summary>Plugin GUID. Also the BepInEx config file name.</summary>
    public const string Guid = "valheim.qol.cm";

    /// <summary>Window title.</summary>
    public const string Name = "Valheim QoL CM";

    /// <summary>Spec 006 version. The string is 1.6 so BepInEx shows the same number.</summary>
    public const string Version = "1.6";

    private const string BringMe = "bring-me";
    private const string BringThem = "bring-them";
    private const string Spawn = "spawn";
    private const string Grant = "grant";
    private const string Percent = "percent";
    private const string PercentState = "percent-state";
    private const string Tame = "tame";
    private const string KillEnemies = "kill-enemies";
    private const string Status = "status";
    private const string Teleport = "teleport";

    private static CustomRPC _actions = null!;
    private static ConfigEntry<float> _skillLoss = null!;
    private static float _appliedPercent = SavedSkillLoss.DefaultPercent;
    private static bool _percentFromHost;
    private static bool _hostPercentAdopted;
    private static bool _restoringPercent;

    /// <summary>Admin-only config entry. Gameplay reads <see cref="SkillLossPercent"/>, which the host file wins.</summary>
    public static ConfigEntry<float> SkillLossEntry => _skillLoss;

    /// <summary>Skill-loss percent the next death uses. A saved 0 stays 0.</summary>
    public static float SkillLossPercent => SkillLoss.ClampPercent(_appliedPercent);

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

        _appliedPercent = SavedSkillLoss.Choose(PluginStorage.ReadSkillLoss(), _skillLoss.Value);
        PluginStorage.Debug("Skill loss loaded " + SavedSkillLoss.Format(_appliedPercent) + ".");

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Plugin).Assembly);
        _actions = NetworkManager.Instance.AddRPC("QoLPanel", ServerReceive, ClientReceive);
        SynchronizationManager.Instance.AddInitialSynchronization(_actions, HostPercentPackage);
        SynchronizationManager.OnConfigurationSynchronized += KeepSavedPercent;
        ConsoleManager.Create(this, _skillLoss);
        Logger.LogInfo(Name + " " + Version + " loaded.");
    }

    /// <summary>Stores the percent used for the next death. The host file is written only when persist is true.</summary>
    public static void RememberPercent(float percent, bool persist)
    {
        var clamped = SkillLoss.ClampPercent(percent);
        _appliedPercent = clamped;
        _percentFromHost = true;
        if (persist)
        {
            PluginStorage.WriteSkillLoss(clamped);
            PluginStorage.Debug("Skill loss saved " + SavedSkillLoss.Format(clamped) + ".");
        }

        if (_skillLoss == null || _restoringPercent || Math.Abs(_skillLoss.Value - clamped) <= 0.0001f)
        {
            return;
        }

        _restoringPercent = true;
        _skillLoss.Value = clamped;
        _restoringPercent = false;
    }

    /// <summary>Sends the host percent to connected clients.</summary>
    public static void PushSkillLoss()
    {
        if (_actions == null || ZNet.instance == null || !ZNet.instance.IsServer())
        {
            return;
        }

        var peers = ZNet.instance.m_peers;
        if (peers == null)
        {
            return;
        }

        foreach (var peer in peers)
        {
            if (peer == null)
            {
                continue;
            }

            _actions.SendPackage(peer.m_uid, HostPercentPackage());
        }
    }

    private void Update()
    {
        AdoptHostPercent();
        ConsoleManager.Tick();
    }

    private static void AdoptHostPercent()
    {
        if (_hostPercentAdopted || ZNet.instance == null || !ZNet.instance.IsServer())
        {
            return;
        }

        _hostPercentAdopted = true;
        _percentFromHost = true;
        var saved = PluginStorage.ReadSkillLoss();
        _appliedPercent = SavedSkillLoss.Choose(saved, _appliedPercent);
        if (_skillLoss != null && Math.Abs(_skillLoss.Value - _appliedPercent) > 0.0001f)
        {
            _restoringPercent = true;
            _skillLoss.Value = _appliedPercent;
            _restoringPercent = false;
        }

        PushSkillLoss();
        PluginStorage.Debug("Skill loss host " + SavedSkillLoss.Format(_appliedPercent) + ".");
    }

    private static ZPackage HostPercentPackage()
    {
        var package = new ZPackage();
        package.Write(PercentState);
        package.Write(SkillLossPercent);
        return package;
    }

    private static void KeepSavedPercent(object sender, Jotunn.Utils.ConfigurationSynchronizationEventArgs args)
    {
        if (!_percentFromHost || _skillLoss == null || _restoringPercent)
        {
            return;
        }

        if (Math.Abs(_skillLoss.Value - SkillLossPercent) <= 0.0001f)
        {
            ConsoleManager.RefreshSkillLoss();
            return;
        }

        RememberPercent(SkillLossPercent, false);
        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            PushSkillLoss();
            PluginStorage.Debug("Skill loss restored " + SavedSkillLoss.Format(SkillLossPercent) + ".");
        }

        ConsoleManager.RefreshSkillLoss();
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
        else if (action == PercentState)
        {
            RememberPercent(package.ReadSingle(), false);
            ConsoleManager.RefreshSkillLoss();
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
