namespace ValheimQoLCM.Core;

public static class ItemListing
{
	public static bool CanPickUp(int iconCount)
	{
		return iconCount > 0;
	}

	public static string BaseName(string? localizedName, string? prefab)
	{
		if (localizedName == null || localizedName.Length == 0 || localizedName[0] == '$' || string.IsNullOrWhiteSpace(localizedName))
		{
			return prefab ?? string.Empty;
		}
		return localizedName.Trim();
	}

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
