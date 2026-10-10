namespace ValheimQoLCM.Core;

/// <summary>Who may open the panel and who may change gameplay. Both answers are the admin flag.</summary>
public static class AdminGate
{
	/// <summary>True when the local player may see the panel.</summary>
	public static bool CanOpenPanel(bool isAdmin)
	{
		return isAdmin;
	}

	/// <summary>True when a request from this player may change game state.</summary>
	public static bool CanMutate(bool isAdmin)
	{
		return isAdmin;
	}
}
