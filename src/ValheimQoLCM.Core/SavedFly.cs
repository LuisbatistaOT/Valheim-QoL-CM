namespace ValheimQoLCM.Core;

/// <summary>Fly choice saved by the plugin. Spec 007.</summary>
public static class SavedFly
{
	/// <summary>Fly for the next session. A missing value is off.</summary>
	public static bool Choose(bool? saved)
	{
		return saved == true;
	}
}
