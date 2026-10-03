using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>God, fly, creative, and free cam on the local admin character.</summary>
public static class GameplayModifiers
{
    /// <summary>Reads the vanilla flag for one mode.</summary>
    public static bool IsEnabled(PlayerMode mode)
    {
        var player = Player.m_localPlayer;
        switch (mode)
        {
            case PlayerMode.God:
                return player != null && player.InGodMode();
            case PlayerMode.Fly:
                return player != null && player.InDebugFlyMode();
            case PlayerMode.Creative:
                return player != null && player.m_noPlacementCost;
            case PlayerMode.FreeCam:
                return GameCamera.InFreeFly();
            default:
                return false;
        }
    }

    /// <summary>Toggles one vanilla mode and leaves the other three alone.</summary>
    public static ActionResult<bool> Toggle(PlayerMode mode)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<bool>.Fail("Admins only.");
        }

        var player = Player.m_localPlayer;
        var toggles = new ModeToggles
        {
            CharacterIsDead = player == null || player.IsDead()
        };
        var next = !IsEnabled(mode);
        var result = toggles.Set(mode, next);
        if (!result.Ok || player == null)
        {
            return result.Ok ? ActionResult<bool>.Fail("Character is dead.") : result;
        }

        Apply(player, mode, next);
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
                var camera = GameCamera.instance;
                if (camera != null && GameCamera.InFreeFly() != enabled)
                {
                    camera.ToggleFreeFly();
                }

                break;
        }
    }
}
