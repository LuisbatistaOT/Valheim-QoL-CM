using Object = UnityEngine.Object;
using PlayerInfo = ZNet.PlayerInfo;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System.Collections.Generic;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class NearbyCommands
{
	public static ActionResult<int> RequestTame()
	{
		return Request("tame");
	}

	public static ActionResult<int> RequestKillEnemies()
	{
		return Request("kill-enemies");
	}

	public static void ApplyTame(long sender)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!TryAdmin(sender, out var position))
		{
			return;
		}
		int num = 0;
		List<Character> allCharacters = Character.GetAllCharacters();
		if (allCharacters != null)
		{
			foreach (Character item in allCharacters)
			{
				if (!((Object)(object)item == (Object)null))
				{
					bool hasTameable = (Object)(object)((Component)item).GetComponent<Tameable>() != (Object)null;
					if (NearbyActions.IsTameTarget(item.IsPlayer(), hasTameable))
					{
						num++;
					}
				}
			}
		}
		if (num > 0)
		{
			Tameable.TameAllInArea(position, 20f);
		}
		Plugin.Reply(sender, NearbyActions.TameMessage(num));
	}

	public static void ApplyKillEnemies(long sender)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		if (!TryAdmin(sender, out var position))
		{
			PluginStorage.Debug("Kill enemies rejected.");
			return;
		}
		int num = 0;
		int num2 = 0;
		List<Character> allCharacters = Character.GetAllCharacters();
		if (allCharacters != null)
		{
			foreach (Character item in allCharacters)
			{
				if (!((Object)(object)item == (Object)null))
				{
					num2++;
					float distance = Vector3.Distance(position, ((Component)item).transform.position);
					bool hasPiece = (Object)(object)((Component)item).GetComponent<Piece>() != (Object)null;
					if (NearbyActions.IsKillTarget(item.IsPlayer(), hasPiece, item.IsTamed(), distance))
					{
						item.Damage(new HitData(1E+10f));
						num++;
					}
				}
			}
		}
		PluginStorage.Debug(NearbyActions.KillDebug(position.x, position.y, position.z, num2, num));
		Plugin.Reply(sender, NearbyActions.KillMessage(num));
	}

	private static ActionResult<int> Request(string action)
	{
		Player localPlayer = Player.m_localPlayer;
		ActionResult<int> actionResult = NearbyActions.Begin(Plugin.LocalIsAdmin(), (Object)(object)localPlayer == (Object)null || ((Character)localPlayer).IsDead());
		if (!actionResult.Ok)
		{
			return actionResult;
		}
		Plugin.Send(action, delegate
		{
		});
		return actionResult;
	}

	private static bool TryAdmin(long sender, out Vector3 position)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		string text = SenderName(sender);
		if (text == null)
		{
			position = Vector3.zero;
			Plugin.Reply(sender, "Admin player was not found.");
			return false;
		}
		if (!TryPosition(text, out position))
		{
			Plugin.Reply(sender, "Admin player was not found.");
			return false;
		}
		Player val = FindPlayer(text);
		if ((Object)(object)val != (Object)null && ((Character)val).IsDead())
		{
			Plugin.Reply(sender, "Character is dead.");
			return false;
		}
		return true;
	}

	private static Player? FindPlayer(string playerName)
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null)
		{
			return null;
		}
		foreach (Player item in allPlayers)
		{
			if ((Object)(object)item != (Object)null && item.GetPlayerName() == playerName)
			{
				return item;
			}
		}
		return null;
	}

	private static string? SenderName(long sender)
	{
		if (sender == 0)
		{
			return ((Object)(object)Player.m_localPlayer != (Object)null) ? Player.m_localPlayer.GetPlayerName() : null;
		}
		return (((Object)(object)ZNet.instance != (Object)null) ? ZNet.instance.GetPeer(sender) : null)?.m_playerName;
	}

	private static bool TryPosition(string playerName, out Vector3 position)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Player val = FindPlayer(playerName);
		if ((Object)(object)val != (Object)null)
		{
			position = ((Component)val).transform.position;
			return true;
		}
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
