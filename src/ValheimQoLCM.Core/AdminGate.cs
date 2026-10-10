namespace ValheimQoLCM.Core;

public static class AdminGate
{
	public static bool CanOpenPanel(bool isAdmin)
	{
		return isAdmin;
	}

	public static bool CanMutate(bool isAdmin)
	{
		return isAdmin;
	}
}
