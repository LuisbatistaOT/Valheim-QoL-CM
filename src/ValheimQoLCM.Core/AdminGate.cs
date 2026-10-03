namespace ValheimQoLCM.Core;

/// <summary>Separates admins from regular players.</summary>
public static class AdminGate
{
    /// <summary>The panel opens only for an admin.</summary>
    public static bool CanOpenPanel(bool isAdmin) => isAdmin;

    /// <summary>Gameplay mutations are accepted only from an admin.</summary>
    public static bool CanMutate(bool isAdmin) => isAdmin;
}
