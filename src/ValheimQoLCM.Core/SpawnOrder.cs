namespace ValheimQoLCM.Core;

/// <summary>A validated spawn request. Produced only by <see cref="SpawnValidation.Validate"/>.</summary>
public sealed class SpawnOrder
{
	/// <summary>Prefab name, trimmed.</summary>
	public string Prefab { get; }

	/// <summary>1 through 100.</summary>
	public int Quantity { get; }

	/// <summary>1 through the item's vanilla maximum.</summary>
	public int Quality { get; }

	/// <summary>Create an order. Callers go through <see cref="SpawnValidation.Validate"/> instead.</summary>
	public SpawnOrder(string prefab, int quantity, int quality)
	{
		Prefab = prefab;
		Quantity = quantity;
		Quality = quality;
	}
}
