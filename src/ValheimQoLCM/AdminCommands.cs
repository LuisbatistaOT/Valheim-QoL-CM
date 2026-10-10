using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;
using PlayerInfo = ZNet.PlayerInfo;

namespace ValheimQoLCM;

public static class AdminCommands
{
	public static IReadOnlyList<ConnectedPlayer> ListConnected()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		List<ConnectedPlayer> list = new List<ConnectedPlayer>();
		if ((Object)(object)ZNet.instance == (Object)null)
		{
			return list;
		}
		Player localPlayer = Player.m_localPlayer;
		string text = (((Object)(object)localPlayer != (Object)null) ? localPlayer.GetPlayerName() : null);
		bool flag = false;
		foreach (PlayerInfo player in ZNet.instance.GetPlayerList())
		{
			if (!string.IsNullOrEmpty(player.m_name))
			{
				bool flag2 = !string.IsNullOrEmpty(text) && player.m_name == text;
				if (flag2)
				{
					flag = true;
				}
				list.Add(new ConnectedPlayer(player.m_name, ReadSteamId(player), flag2));
			}
		}
		if (!flag && (Object)(object)localPlayer != (Object)null && text != null)
		{
			string text2 = text;
			if (text2.Length > 0)
			{
				string text3 = localPlayer.GetPlayerID().ToString();
				list.Insert(0, new ConnectedPlayer(text2, SteamId.IsWellFormed(text3) ? text3 : null, isSelf: true));
			}
		}
		return list;
	}

	public static ActionResult<string> RequestBringMe(string? targetName)
	{
		ActionResult<string> selection = Select(targetName);
		if (!selection.Ok)
		{
			return selection;
		}
		Plugin.Send("bring-me", delegate(ZPackage package)
		{
			package.Write(selection.Data);
		});
		return ActionResult<string>.Success(selection.Data);
	}

	public static ActionResult<string> RequestBringTarget(string? targetName)
	{
		ActionResult<string> selection = Select(targetName);
		if (!selection.Ok)
		{
			return selection;
		}
		Plugin.Send("bring-them", delegate(ZPackage package)
		{
			package.Write(selection.Data);
		});
		return ActionResult<string>.Success(selection.Data);
	}

	public static ActionResult<string> RequestGrant(string targetName, string? typedId)
	{
		if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
		{
			return ActionResult<string>.Fail("Admins only.");
		}
		ConnectedPlayer connectedPlayer = null;
		foreach (ConnectedPlayer item in ListConnected())
		{
			if (item.Name == targetName)
			{
				connectedPlayer = item;
				break;
			}
		}
		if (connectedPlayer == null)
		{
			return ActionResult<string>.Fail("Player is not connected.");
		}
		ActionResult<string> resolved = SteamId.Resolve(connectedPlayer.SteamId, typedId);
		if (!resolved.Ok)
		{
			return resolved;
		}
		Plugin.Send("grant", delegate(ZPackage package)
		{
			package.Write(resolved.Data);
		});
		return resolved;
	}

	public static void ApplyBringMe(long sender, string targetName)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!TryPosition(targetName, out var position))
		{
			Plugin.Reply(sender, "Player is not connected.");
			return;
		}
		string text = SenderName(sender);
		if (text == null)
		{
			Plugin.Reply(sender, "Admin player was not found.");
			return;
		}
		if (text == targetName)
		{
			Plugin.Reply(sender, "Select another player.");
			return;
		}
		Plugin.TeleportPlayer(text, position);
		Plugin.Reply(sender, "Moved you to " + targetName + ".");
	}

	public static void ApplyBringTarget(long sender, string targetName)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		string text = SenderName(sender);
		if (text == null || !TryPosition(text, out var position))
		{
			Plugin.Reply(sender, "Admin player was not found.");
			return;
		}
		if (!TryPosition(targetName, out var _))
		{
			Plugin.Reply(sender, "Player is not connected.");
			return;
		}
		if (text == targetName)
		{
			Plugin.Reply(sender, "Select another player.");
			return;
		}
		Plugin.TeleportPlayer(targetName, position);
		Plugin.Reply(sender, "Moved " + targetName + " to you.");
	}

	public static void ApplyGrant(long sender, string steamId)
	{
		if (!SteamId.IsWellFormed(steamId))
		{
			Plugin.Reply(sender, "Steam ID is not valid.");
			return;
		}
		SyncedList val = (((Object)(object)ZNet.instance != (Object)null) ? ZNet.instance.m_adminList : null);
		if (val == null)
		{
			Plugin.Reply(sender, "Admin list is unavailable.");
			return;
		}
		if (val.Contains(steamId))
		{
			Plugin.Reply(sender, steamId + " is already an admin.");
			return;
		}
		val.Add(steamId);
		val.Save();
		Plugin.Reply(sender, "Granted admin to " + steamId + ".");
	}

	private static ActionResult<string> Select(string? targetName)
	{
		if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
		{
			return ActionResult<string>.Fail("Admins only.");
		}
		bool stillConnected = false;
		bool isSelf = false;
		foreach (ConnectedPlayer item in ListConnected())
		{
			if (item.Name != targetName)
			{
				continue;
			}
			stillConnected = true;
			isSelf = item.IsSelf;
			break;
		}
		return TeleportSelection.Select(targetName, stillConnected, isSelf);
	}

	private static string? ReadSteamId(PlayerInfo info)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string userID = info.m_userInfo.m_id.m_userID;
		if (SteamId.IsWellFormed(userID))
		{
			return userID.Trim();
		}
		if (string.IsNullOrEmpty(userID))
		{
			return null;
		}
		string text = string.Empty;
		string text2 = userID;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			if (c >= '0' && c <= '9')
			{
				text += c;
			}
		}
		return SteamId.IsWellFormed(text) ? text : null;
	}

	private static bool TryPosition(string playerName, out Vector3 position)
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

	private static string? SenderName(long sender)
	{
		if (sender == 0)
		{
			return ((Object)(object)Player.m_localPlayer != (Object)null) ? Player.m_localPlayer.GetPlayerName() : null;
		}
		return (((Object)(object)ZNet.instance != (Object)null) ? ZNet.instance.GetPeer(sender) : null)?.m_playerName;
	}
}
