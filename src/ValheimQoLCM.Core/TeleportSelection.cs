namespace ValheimQoLCM.Core;

/// <summary>Selects a teleport target from the connected-player list.</summary>
public static class TeleportSelection
{
    /// <summary>Accepts another connected player. Names are chosen, not typed.</summary>
    public static ActionResult<string> Select(string? selectedName, bool stillConnected, bool isSelf)
    {
        if (selectedName == null || string.IsNullOrWhiteSpace(selectedName) || !stillConnected)
        {
            return ActionResult<string>.Fail("Player is not connected.");
        }

        if (isSelf)
        {
            return ActionResult<string>.Fail("Select another player.");
        }

        return ActionResult<string>.Success(selectedName);
    }
}
