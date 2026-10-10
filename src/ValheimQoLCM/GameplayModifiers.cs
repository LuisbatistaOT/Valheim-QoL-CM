using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class GameplayModifiers
{
	public static bool IsEnabled(PlayerMode mode)
	{
		Player localPlayer = Player.m_localPlayer;
		return mode switch
		{
			PlayerMode.God => (Object)(object)localPlayer != (Object)null && ((Character)localPlayer).InGodMode(), 
			PlayerMode.Fly => (Object)(object)localPlayer != (Object)null && localPlayer.InDebugFlyMode(), 
			PlayerMode.Creative => (Object)(object)localPlayer != (Object)null && localPlayer.m_noPlacementCost, 
			PlayerMode.FreeCam => GameCamera.InFreeFly(), 
			PlayerMode.Ghost => (Object)(object)localPlayer != (Object)null && ((Character)localPlayer).InGhostMode(), 
			_ => false, 
		};
	}

	public static ActionResult<bool> Toggle(PlayerMode mode)
	{
		return Set(mode, !IsEnabled(mode));
	}

	public static ActionResult<bool> Set(PlayerMode mode, bool enabled)
	{
		if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
		{
			return ActionResult<bool>.Fail("Admins only.");
		}
		Player localPlayer = Player.m_localPlayer;
		ModeToggles modeToggles = new ModeToggles
		{
			CharacterIsDead = ((Object)(object)localPlayer == (Object)null || ((Character)localPlayer).IsDead())
		};
		ActionResult<bool> actionResult = modeToggles.Set(mode, enabled);
		if (!actionResult.Ok || (Object)(object)localPlayer == (Object)null)
		{
			return actionResult.Ok ? ActionResult<bool>.Fail("Character is dead.") : actionResult;
		}
		Apply(localPlayer, mode, enabled);
		return ActionResult<bool>.Success(IsEnabled(mode));
	}

	private static void Apply(Player player, PlayerMode mode, bool enabled)
	{
		switch (mode)
		{
		case PlayerMode.God:
			player.SetGodMode(enabled);
			break;
		case PlayerMode.Fly:
			if (player.InDebugFlyMode() != enabled)
			{
				player.ToggleDebugFly();
			}
			if (player.InDebugFlyMode() != enabled)
			{
				player.m_debugFly = enabled;
			}
			break;
		case PlayerMode.Creative:
			player.SetNoPlacementCost(enabled);
			break;
		case PlayerMode.FreeCam:
		{
			GameCamera instance = GameCamera.instance;
			if ((Object)(object)instance != (Object)null && GameCamera.InFreeFly() != enabled)
			{
				instance.ToggleFreeFly();
			}
			break;
		}
		case PlayerMode.Ghost:
			player.SetGhostMode(enabled);
			break;
		}
	}
}
