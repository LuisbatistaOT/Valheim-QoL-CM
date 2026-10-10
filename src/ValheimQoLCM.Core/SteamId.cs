using System;

namespace ValheimQoLCM.Core;

/// <summary>Steam ID rules for Grant admin.</summary>
public static class SteamId
{
	/// <summary>True for seventeen digits starting with <c>7656</c>, after trimming.</summary>
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

	/// <summary>
	/// The ID to grant: the live connection's ID when well formed, otherwise the typed one.
	/// Fails with <c>Steam ID is not valid.</c> when neither is.
	/// </summary>
	public static ActionResult<string> Resolve(string? connectionId, string? typedId)
	{
		string? fromConnection = connectionId?.Trim();
		if (fromConnection != null && IsWellFormed(fromConnection))
		{
			return ActionResult<string>.Success(fromConnection);
		}
		string? fromTyped = typedId?.Trim();
		if (fromTyped != null && IsWellFormed(fromTyped))
		{
			return ActionResult<string>.Success(fromTyped);
		}
		return ActionResult<string>.Fail("Steam ID is not valid.");
	}
}
