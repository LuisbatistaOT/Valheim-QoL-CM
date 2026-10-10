namespace ValheimQoLCM.Core;

public static class TeleportSelection
{
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
