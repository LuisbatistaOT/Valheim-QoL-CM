using System.Globalization;

namespace ValheimQoLCM.Core;

public static class NearbyActions
{
	public const float TameRadius = 20f;

	public const float KillRadius = 1000f;

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

	public static bool IsTameTarget(bool isPlayer, bool hasTameable)
	{
		return !isPlayer && hasTameable;
	}

	public static bool IsKillTarget(bool isPlayer, bool hasPiece, bool isTamed, float distance)
	{
		return !isPlayer && !hasPiece && !isTamed && distance <= 1000f;
	}

	public static string TameMessage(int count)
	{
		return (count == 0) ? "No tameable animal was nearby." : ("Tamed " + count + ".");
	}

	public static string KillMessage(int count)
	{
		return (count == 0) ? "No enemy was nearby." : ("Killed " + count + ".");
	}

	public static string KillDebug(float x, float y, float z, int seen, int killed)
	{
		return string.Format(CultureInfo.InvariantCulture, "Kill enemies at {0}, {1}, {2} radius {3} seen {4} killed {5}.", x.ToString("0.##", CultureInfo.InvariantCulture), y.ToString("0.##", CultureInfo.InvariantCulture), z.ToString("0.##", CultureInfo.InvariantCulture), 1000f.ToString("0", CultureInfo.InvariantCulture), seen, killed);
	}
}
