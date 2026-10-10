namespace ValheimQoLCM.Core;

/// <summary>Which highlighted player Bring me and Bring player may act on.</summary>
public static class TeleportSelection
{
	/// <summary>
	/// The selected name when it is another connected player. Fails with
	/// <c>Player is not connected.</c> or <c>Select another player.</c>.
	/// </summary>
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
