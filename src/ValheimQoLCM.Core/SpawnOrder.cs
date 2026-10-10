namespace ValheimQoLCM.Core;

public sealed class SpawnOrder
{
	public string Prefab { get; }

	public int Quantity { get; }

	public int Quality { get; }

	public SpawnOrder(string prefab, int quantity, int quality)
	{
		Prefab = prefab;
		Quantity = quantity;
		Quality = quality;
	}
}
