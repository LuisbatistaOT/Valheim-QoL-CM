namespace ValheimQoLCM.Core;

/// <summary>The five Cheats-tab modes for the local character. Setting one leaves the others alone.</summary>
public sealed class ModeToggles
{
	/// <summary>God mode is on.</summary>
	public bool God { get; private set; }

	/// <summary>Fly is on.</summary>
	public bool Fly { get; private set; }

	/// <summary>Creative (vanilla no-build-cost) is on.</summary>
	public bool Creative { get; private set; }

	/// <summary>Free camera is on.</summary>
	public bool FreeCam { get; private set; }

	/// <summary>Ghost mode is on, so enemies ignore the character.</summary>
	public bool Ghost { get; private set; }

	/// <summary>Set by the caller. A dead character rejects every toggle.</summary>
	public bool CharacterIsDead { get; set; }

	/// <summary>
	/// Turn one mode on or off. Fails with <c>Character is dead.</c> while
	/// <see cref="CharacterIsDead"/> is set, and with <c>Unknown mode.</c> for a value outside <see cref="PlayerMode"/>.
	/// </summary>
	public ActionResult<bool> Set(PlayerMode mode, bool enabled)
	{
		if (CharacterIsDead)
		{
			return ActionResult<bool>.Fail("Character is dead.");
		}
		switch (mode)
		{
		case PlayerMode.God:
			God = enabled;
			break;
		case PlayerMode.Fly:
			Fly = enabled;
			break;
		case PlayerMode.Creative:
			Creative = enabled;
			break;
		case PlayerMode.FreeCam:
			FreeCam = enabled;
			break;
		case PlayerMode.Ghost:
			Ghost = enabled;
			break;
		default:
			return ActionResult<bool>.Fail("Unknown mode.");
		}
		return ActionResult<bool>.Success(enabled);
	}
}
