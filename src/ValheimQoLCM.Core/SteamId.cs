using System;

namespace ValheimQoLCM.Core;

/// <summary>Resolves a Steam ID from a live connection, then from typed input.</summary>
public static class SteamId
{
    /// <summary>True for a 17-digit SteamID64.</summary>
    public static bool IsWellFormed(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var trimmed = id.Trim();
        if (trimmed.Length != 17 || !trimmed.StartsWith("7656", StringComparison.Ordinal))
        {
            return false;
        }

        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] < '0' || trimmed[i] > '9')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Uses the connection ID when it is well formed. Otherwise uses the typed fallback.</summary>
    public static ActionResult<string> Resolve(string? connectionId, string? typedId)
    {
        if (IsWellFormed(connectionId))
        {
            return ActionResult<string>.Success(connectionId!.Trim());
        }

        if (IsWellFormed(typedId))
        {
            return ActionResult<string>.Success(typedId!.Trim());
        }

        return ActionResult<string>.Fail("Steam ID is not valid.");
    }
}
