using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

[BepInPlugin("valheim.qol.cm", "Valheim QoL CM", "1.8")]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
	public const string Guid = "valheim.qol.cm";

	public const string Name = "Valheim QoL CM";

	public const string Version = "1.8";

	private const string BringMe = "bring-me";

	private const string BringThem = "bring-them";

	private const string Spawn = "spawn";

	private const string Grant = "grant";

	private const string World = "world";

	private const string OverwriteState = "overwrite-state";

	private const string Tame = "tame";

	private const string KillEnemies = "kill-enemies";

	private const string Status = "status";

	private const string Teleport = "teleport";

	private static CustomRPC _actions;

	private static bool _overwriteApplied;

	private static bool _overwriteFromHost;

	private static bool _overwriteAdopted;

	public static bool OverwriteApplied => _overwriteFromHost && _overwriteApplied;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		Harmony val = new Harmony("valheim.qol.cm");
		val.PatchAll(typeof(Plugin).Assembly);
		_actions = NetworkManager.Instance.AddRPC("QoLPanel", ServerReceive, ClientReceive);
		SynchronizationManager.Instance.AddInitialSynchronization(_actions, (Func<ZPackage>)HostOverwritePackage);
		ConsoleManager.Create(this);
		Logger.LogInfo((object)"Valheim QoL CM 1.8 loaded.");
	}

	public static void SetOverwrite(bool overwrite, bool persist)
	{
		_overwriteApplied = overwrite;
		_overwriteFromHost = true;
		if (persist)
		{
			PluginStorage.WriteOverwrite(overwrite);
			PluginStorage.Debug("Skill overwrite saved " + SkillOverwrite.Format(overwrite) + ".");
		}
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
		{
			PushOverwrite();
		}
	}

	public static void PushOverwrite()
	{
		if (_actions == null || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		List<ZNetPeer> peers = ZNet.instance.m_peers;
		if (peers == null)
		{
			return;
		}
		foreach (ZNetPeer item in peers)
		{
			if (item != null)
			{
				_actions.SendPackage(item.m_uid, HostOverwritePackage());
			}
		}
	}

	private void Update()
	{
		AdoptOverwrite();
		ConsoleManager.Tick();
	}

	private void LateUpdate()
	{
		ConsoleManager.SyncWorldMarks();
	}

	private static void AdoptOverwrite()
	{
		if (!_overwriteAdopted && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			_overwriteAdopted = true;
			SetOverwrite(PluginStorage.ReadOverwrite(), persist: false);
			PluginStorage.Debug("Skill overwrite host " + SkillOverwrite.Format(OverwriteApplied) + ".");
		}
	}

	private static ZPackage HostOverwritePackage()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		ZPackage val = new ZPackage();
		val.Write("overwrite-state");
		val.Write(OverwriteApplied);
		return val;
	}

	public static bool LocalIsAdmin()
	{
		return SynchronizationManager.Instance != null && SynchronizationManager.Instance.PlayerIsAdmin;
	}

	public static void Send(string action, Action<ZPackage> write)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (!AdminGate.CanMutate(LocalIsAdmin()))
		{
			ConsoleManager.Show("Admins only.");
			return;
		}
		if ((Object)(object)ZNet.instance == (Object)null)
		{
			ConsoleManager.Show("Not connected.");
			return;
		}
		ZPackage val = new ZPackage();
		val.Write(action);
		write(val);
		if (ZNet.instance.IsServer())
		{
			val.SetPos(0);
			ApplyServer(0L, val);
			return;
		}
		ZNetPeer serverPeer = ZNet.instance.GetServerPeer();
		if (serverPeer == null)
		{
			ConsoleManager.Show("Server peer missing.");
		}
		else
		{
			_actions.SendPackage(serverPeer.m_uid, val);
		}
	}

	public static void Reply(long sender, string message)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (sender == 0L || (Object)(object)ZNet.instance == (Object)null || ZNet.instance.GetPeer(sender) == null)
		{
			ConsoleManager.Show(message);
			return;
		}
		ZPackage val = new ZPackage();
		val.Write("status");
		val.Write(message);
		_actions.SendPackage(sender, val);
	}

	public static void TeleportPlayer(string playerName, Vector3 position)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		ZNetPeer peerByPlayerName = ZNet.instance.GetPeerByPlayerName(playerName);
		if (peerByPlayerName != null)
		{
			ZPackage val = new ZPackage();
			val.Write("teleport");
			val.Write(position);
			_actions.SendPackage(peerByPlayerName.m_uid, val);
		}
		else
		{
			Player localPlayer = Player.m_localPlayer;
			if ((Object)(object)localPlayer != (Object)null && localPlayer.GetPlayerName() == playerName)
			{
				((Character)localPlayer).TeleportTo(position, ((Component)localPlayer).transform.rotation, true);
			}
		}
	}

	private static IEnumerator ServerReceive(long sender, ZPackage package)
	{
		ApplyServer(sender, package);
		yield break;
	}

	private static IEnumerator ClientReceive(long sender, ZPackage package)
	{
		switch (package.ReadString())
		{
		case "teleport":
		{
			Vector3 position = package.ReadVector3();
			Player local = Player.m_localPlayer;
			if ((Object)(object)local != (Object)null && !((Character)local).IsDead())
			{
				((Character)local).TeleportTo(position, ((Component)local).transform.rotation, true);
			}
			break;
		}
		case "status":
			ConsoleManager.Show(package.ReadString());
			break;
		case "overwrite-state":
			SetOverwrite(package.ReadBool(), persist: false);
			break;
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
			switch (package.ReadString())
			{
			case "bring-me":
				AdminCommands.ApplyBringMe(sender, package.ReadString());
				break;
			case "bring-them":
				AdminCommands.ApplyBringTarget(sender, package.ReadString());
				break;
			case "spawn":
				ItemSpawner.ApplySpawn(sender, package.ReadString(), package.ReadString(), package.ReadInt(), package.ReadInt());
				break;
			case "grant":
				AdminCommands.ApplyGrant(sender, package.ReadString());
				break;
			case "world":
				WorldModifierHost.Apply(sender, package.ReadString(), package.ReadString(), package.ReadString(), package.ReadString(), package.ReadString(), package.ReadBool());
				break;
			case "tame":
				NearbyCommands.ApplyTame(sender);
				break;
			case "kill-enemies":
				NearbyCommands.ApplyKillEnemies(sender);
				break;
			default:
				Reply(sender, "Unknown action.");
				break;
			}
		}
		catch (Exception ex)
		{
			Jotunn.Logger.LogError((object)ex);
			Reply(sender, "Action failed.");
		}
	}

	private static bool SenderIsAdmin(long sender)
	{
		if ((Object)(object)ZNet.instance == (Object)null)
		{
			return false;
		}
		if (sender == 0)
		{
			return ZNet.instance.LocalPlayerIsAdminOrHost();
		}
		ZNetPeer peer = ZNet.instance.GetPeer(sender);
		if (peer == null)
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && peer.m_playerName == localPlayer.GetPlayerName() && ZNet.instance.LocalPlayerIsAdminOrHost())
		{
			return true;
		}
		string text = ((peer.m_socket != null) ? peer.m_socket.GetHostName() : null);
		if (!string.IsNullOrEmpty(text) && ZNet.instance.IsAdmin(text))
		{
			return true;
		}
		return peer.m_playerID != 0L && ZNet.instance.IsAdmin(peer.m_playerID.ToString());
	}
}
