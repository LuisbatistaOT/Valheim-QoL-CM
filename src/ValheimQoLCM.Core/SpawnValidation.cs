namespace ValheimQoLCM.Core;

public static class SpawnValidation
{
	public const int MinQuantity = 1;

	public const int MaxQuantity = 100;

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
