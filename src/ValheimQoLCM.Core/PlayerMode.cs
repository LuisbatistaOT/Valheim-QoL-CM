namespace ValheimQoLCM.Core;

/// <summary>The toggles on the Cheats tab.</summary>
public enum PlayerMode
{
	/// <summary>God mode.</summary>
	God,
	/// <summary>Fly. Saved by the plugin between sessions.</summary>
	Fly,
	/// <summary>Creative, which is vanilla no-build-cost.</summary>
	Creative,
	/// <summary>Vanilla free camera.</summary>
	FreeCam,
	/// <summary>Vanilla ghost mode.</summary>
	Ghost
}
