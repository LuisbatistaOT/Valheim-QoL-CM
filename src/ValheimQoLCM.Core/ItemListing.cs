using System;

namespace ValheimQoLCM.Core;

/// <summary>Chooses which items the spawn list may show, and how a row is named.</summary>
public static class ItemListing
{
    /// <summary>
    /// True when a player can pick the drop up.
    /// Valheim rejects a pickup when the shared icon list is empty.
    /// </summary>
    public static bool CanPickUp(int iconCount)
    {
        return iconCount > 0;
    }

    /// <summary>In-game name, or the prefab name when localization has no words yet.</summary>
    public static string BaseName(string? localizedName, string? prefab)
    {
        if (localizedName == null || localizedName.Length == 0 || localizedName[0] == '$' || string.IsNullOrWhiteSpace(localizedName))
        {
            return prefab ?? string.Empty;
        }

        return localizedName.Trim();
    }

    /// <summary>
    /// Row text for one prefab. Shared in-game names keep the prefab so the console cannot name a sibling.
    /// </summary>
    public static string RowLabel(string? localizedName, string? prefab, bool nameIsShared)
    {
        var name = BaseName(localizedName, prefab);
        if (nameIsShared && !string.IsNullOrEmpty(prefab))
        {
            return name + " (" + prefab + ")";
        }

        return name;
    }
}
