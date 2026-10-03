namespace ValheimQoLCM.Core;

/// <summary>Server-wide percent of each current skill level removed on death.</summary>
public static class SkillLoss
{
    /// <summary>Lowest accepted percent.</summary>
    public const float MinPercent = 0f;

    /// <summary>Highest accepted percent.</summary>
    public const float MaxPercent = 100f;

    /// <summary>Death skill loss does not change the vanilla gear drop.</summary>
    public const bool LeavesGearUntouched = true;

    /// <summary>Clamps a stored percent into 0 through 100. Non-finite values become 0.</summary>
    public static float ClampPercent(float percent)
    {
        if (float.IsNaN(percent) || float.IsInfinity(percent) || percent < MinPercent)
        {
            return MinPercent;
        }

        if (percent > MaxPercent)
        {
            return MaxPercent;
        }

        return percent;
    }

    /// <summary>Converts a percent into the fraction passed to vanilla skill loss.</summary>
    public static float ToFactor(float percent) => ClampPercent(percent) / 100f;

    /// <summary>Returns the skill level remaining after one death.</summary>
    public static float Apply(float skillLevel, float percent)
    {
        if (skillLevel < 0f)
        {
            skillLevel = 0f;
        }

        var remaining = skillLevel - (skillLevel * ToFactor(percent));
        return remaining < 0f ? 0f : remaining;
    }
}
