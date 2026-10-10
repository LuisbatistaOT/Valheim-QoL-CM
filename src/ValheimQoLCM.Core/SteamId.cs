using System;

namespace ValheimQoLCM.Core;

public static class SteamId
{
	public static bool IsWellFormed(string? id)
	{
		if (id == null || string.IsNullOrWhiteSpace(id))
		{
			return false;
		}
		string text = id.Trim();
		if (text.Length != 17 || !text.StartsWith("7656", StringComparison.Ordinal))
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] < '0' || text[i] > '9')
			{
				return false;
			}
		}
		return true;
	}

	public static ActionResult<string> Resolve(string? connectionId, string? typedId)
	{
		if (IsWellFormed(connectionId))
		{
			return ActionResult<string>.Success(connectionId.Trim());
		}
		if (IsWellFormed(typedId))
		{
			return ActionResult<string>.Success(typedId.Trim());
		}
		return ActionResult<string>.Fail("Steam ID is not valid.");
	}
}
