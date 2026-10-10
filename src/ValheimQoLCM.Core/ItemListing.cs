namespace ValheimQoLCM.Core;

/// <summary>Which item prefabs the Spawn tab lists and how each row is named.</summary>
public static class ItemListing
{
	/// <summary>
	/// True when a drop of this prefab can be picked up. Valheim refuses a drop whose
	/// <c>m_icons</c> list is empty, so such prefabs stay out of the list.
	/// </summary>
	public static bool CanPickUp(int iconCount)
	{
		return iconCount > 0;
	}

	/// <summary>
	/// Display name for a row. The localized name wins unless it is empty or still a
	/// <c>$token</c>, in which case the prefab name is used.
	/// </summary>
	public static string BaseName(string? localizedName, string? prefab)
	{
		if (localizedName == null || localizedName.Length == 0 || localizedName[0] == '$' || string.IsNullOrWhiteSpace(localizedName))
		{
			return prefab ?? string.Empty;
		}
		return localizedName.Trim();
	}

	/// <summary>Row label. When two listed items share a name, the prefab is appended in parentheses.</summary>
	public static string RowLabel(string? localizedName, string? prefab, bool nameIsShared)
	{
		string text = BaseName(localizedName, prefab);
		if (nameIsShared && !string.IsNullOrEmpty(prefab))
		{
			return text + " (" + prefab + ")";
		}
		return text;
	}
}
