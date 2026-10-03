namespace ValheimQoLCM.Core;

/// <summary>Independent admin character modes.</summary>
public enum PlayerMode
{
    /// <summary>Vanilla god mode.</summary>
    God,

    /// <summary>Vanilla fly mode.</summary>
    Fly,

    /// <summary>Vanilla no-build-cost.</summary>
    Creative,

    /// <summary>Vanilla free camera.</summary>
    FreeCam,

    /// <summary>Vanilla ghost mode.</summary>
    Ghost
}

/// <summary>Tracks god, fly, creative, free cam, and ghost without coupling them.</summary>
public sealed class ModeToggles
{
    /// <summary>God mode flag.</summary>
    public bool God { get; private set; }

    /// <summary>Fly mode flag.</summary>
    public bool Fly { get; private set; }

    /// <summary>Creative mode flag.</summary>
    public bool Creative { get; private set; }

    /// <summary>Free camera flag.</summary>
    public bool FreeCam { get; private set; }

    /// <summary>Ghost mode flag.</summary>
    public bool Ghost { get; private set; }

    /// <summary>When true, toggles are rejected.</summary>
    public bool CharacterIsDead { get; set; }

    /// <summary>Sets one mode. A dead character leaves every flag unchanged.</summary>
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
