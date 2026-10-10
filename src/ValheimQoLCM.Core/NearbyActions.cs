using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>Rules for Tame and Kill enemies: who may click, which characters count, and the console lines.</summary>
public static class NearbyActions
{
	/// <summary>Radius of the vanilla tame command, in meters.</summary>
	public const float TameRadius = 20f;

	/// <summary>Radius of Kill enemies from the admin, in meters.</summary>
	public const float KillRadius = 1000f;

	/// <summary>Gate for both clicks. Fails for a non-admin and for a dead character.</summary>
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

	/// <summary>True for a non-player character that carries a tameable component.</summary>
	public static bool IsTameTarget(bool isPlayer, bool hasTameable)
	{
		return !isPlayer && hasTameable;
	}

	/// <summary>True for an untamed, non-player, non-piece character within <see cref="KillRadius"/>.</summary>
	public static bool IsKillTarget(bool isPlayer, bool hasPiece, bool isTamed, float distance)
	{
		return !isPlayer && !hasPiece && !isTamed && distance <= 1000f;
	}

	/// <summary>Console line after Tame.</summary>
	public static string TameMessage(int count)
	{
		return (count == 0) ? "No tameable animal was nearby." : ("Tamed " + count + ".");
	}

	/// <summary>Console line after Kill enemies.</summary>
	public static string KillMessage(int count)
	{
		return (count == 0) ? "No enemy was nearby." : ("Killed " + count + ".");
	}

	/// <summary>Debug-log line for one Kill enemies click: admin position, radius, seen, and killed.</summary>
	public static string KillDebug(float x, float y, float z, int seen, int killed)
	{
		return string.Format(CultureInfo.InvariantCulture, "Kill enemies at {0}, {1}, {2} radius {3} seen {4} killed {5}.", x.ToString("0.##", CultureInfo.InvariantCulture), y.ToString("0.##", CultureInfo.InvariantCulture), z.ToString("0.##", CultureInfo.InvariantCulture), 1000f.ToString("0", CultureInfo.InvariantCulture), seen, killed);
	}
}
