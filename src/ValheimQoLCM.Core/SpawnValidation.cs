namespace ValheimQoLCM.Core;

/// <summary>Checks a spawn request before anything is created.</summary>
public static class SpawnValidation
{
	/// <summary>Smallest quantity one click may spawn.</summary>
	public const int MinQuantity = 1;

	/// <summary>Largest quantity one click may spawn, so a click cannot stall the game.</summary>
	public const int MaxQuantity = 100;

	/// <summary>
	/// Accept the request as a <see cref="SpawnOrder"/>, or fail with <c>Unknown item.</c>,
	/// <c>Invalid quantity.</c>, or <c>Invalid quality.</c>.
	/// </summary>
	public static ActionResult<SpawnOrder> Validate(string? prefab, int quantity, int quality, int maxQuality, bool prefabExists)
	{
		if (prefab == null || !prefabExists || string.IsNullOrWhiteSpace(prefab))
		{
			return ActionResult<SpawnOrder>.Fail("Unknown item.");
		}
		if (quantity < 1 || quantity > 100)
		{
			return ActionResult<SpawnOrder>.Fail("Invalid quantity.");
		}
		if (maxQuality < 1 || quality < 1 || quality > maxQuality)
		{
			return ActionResult<SpawnOrder>.Fail("Invalid quality.");
		}
		return ActionResult<SpawnOrder>.Success(new SpawnOrder(prefab.Trim(), quantity, quality));
	}
}
