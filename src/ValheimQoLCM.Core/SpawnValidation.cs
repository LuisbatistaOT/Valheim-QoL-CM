namespace ValheimQoLCM.Core;

/// <summary>Accepted spawn request.</summary>
public sealed class SpawnOrder
{
    /// <summary>Creates an accepted spawn request.</summary>
    public SpawnOrder(string prefab, int quantity, int quality)
    {
        Prefab = prefab;
        Quantity = quantity;
        Quality = quality;
    }

    /// <summary>Vanilla prefab name.</summary>
    public string Prefab { get; }

    /// <summary>Number of items to create.</summary>
    public int Quantity { get; }

    /// <summary>Item quality to apply.</summary>
    public int Quality { get; }
}

/// <summary>Checks item, quantity, and quality before anything is spawned.</summary>
public static class SpawnValidation
{
    /// <summary>Smallest quantity the panel can spawn.</summary>
    public const int MinQuantity = 1;

    /// <summary>Largest quantity the panel can spawn in one click.</summary>
    public const int MaxQuantity = 100;

    /// <summary>Accepts a spawn, or explains why nothing should be created.</summary>
    public static ActionResult<SpawnOrder> Validate(string? prefab, int quantity, int quality, int maxQuality, bool prefabExists)
    {
        if (prefab == null || !prefabExists || string.IsNullOrWhiteSpace(prefab))
        {
            return ActionResult<SpawnOrder>.Fail("Unknown item.");
        }

        if (quantity < MinQuantity || quantity > MaxQuantity)
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
