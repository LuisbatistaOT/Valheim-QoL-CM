using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>Who a tame or kill-enemies click affects, and what the console says.</summary>
public static class NearbyActions
{
    /// <summary>Radius argument passed to the vanilla tame command. The game method does not apply a second filter.</summary>
    public const float TameRadius = 20f;

    /// <summary>Distance used by the vanilla kill-nearby-enemies command.</summary>
    public const float KillRadius = 1000f;

    /// <summary>Rejects a click from a non-admin or a dead character.</summary>
    public static ActionResult<int> Begin(bool isAdmin, bool characterIsDead)
    {
        if (!isAdmin)
        {
            return ActionResult<int>.Fail("Admins only.");
        }

        if (characterIsDead)
        {
            return ActionResult<int>.Fail("Character is dead.");
        }

        return ActionResult<int>.Success(0);
    }

    /// <summary>True for a loaded non-player that can be tamed.</summary>
    public static bool IsTameTarget(bool isPlayer, bool hasTameable)
    {
        return !isPlayer && hasTameable;
    }

    /// <summary>True for an untamed non-player inside the kill radius that is not a building piece.</summary>
    public static bool IsKillTarget(bool isPlayer, bool hasPiece, bool isTamed, float distance)
    {
        return !isPlayer && !hasPiece && !isTamed && distance <= KillRadius;
    }

    /// <summary>Console line for a tame click.</summary>
    public static string TameMessage(int count)
    {
        return count == 0 ? "No tameable animal was nearby." : "Tamed " + count + ".";
    }

    /// <summary>Console line for a kill-enemies click.</summary>
    public static string KillMessage(int count)
    {
        return count == 0 ? "No enemy was nearby." : "Killed " + count + ".";
    }

    /// <summary>Debug line for a kill-enemies click. A miss still names the position and the counts.</summary>
    public static string KillDebug(float x, float y, float z, int seen, int killed)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Kill enemies at {0}, {1}, {2} radius {3} seen {4} killed {5}.",
            x.ToString("0.##", CultureInfo.InvariantCulture),
            y.ToString("0.##", CultureInfo.InvariantCulture),
            z.ToString("0.##", CultureInfo.InvariantCulture),
            KillRadius.ToString("0", CultureInfo.InvariantCulture),
            seen,
            killed);
    }
}
