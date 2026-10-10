namespace ValheimQoLCM.Core;

public sealed class ModeToggles
{
	public bool God { get; private set; }

	public bool Fly { get; private set; }

	public bool Creative { get; private set; }

	public bool FreeCam { get; private set; }

	public bool Ghost { get; private set; }

	public bool CharacterIsDead { get; set; }

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
