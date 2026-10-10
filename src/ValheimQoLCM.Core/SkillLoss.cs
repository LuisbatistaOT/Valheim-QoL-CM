namespace ValheimQoLCM.Core;

/// <summary>Percent skill loss on death. 0 removes nothing, 100 clears the skill, gear is never touched.</summary>
public static class SkillLoss
{
	/// <summary>Lowest accepted percent.</summary>
	public const float MinPercent = 0f;

	/// <summary>Highest accepted percent.</summary>
	public const float MaxPercent = 100f;

	/// <summary>Skill loss never changes item drops.</summary>
	public const bool LeavesGearUntouched = true;

	/// <summary>Clamp to 0 through 100. NaN, infinity, and negatives become 0.</summary>
	public static float ClampPercent(float percent)
	{
		if (float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0f)
		{
			return 0f;
		}
		if (percent > 100f)
		{
			return 100f;
		}
		return percent;
	}

	/// <summary>Clamped percent as a 0 through 1 factor.</summary>
	public static float ToFactor(float percent)
	{
		return ClampPercent(percent) / 100f;
	}

	/// <summary>Skill level after a death at this percent. Never below 0.</summary>
	public static float Apply(float skillLevel, float percent)
	{
		if (skillLevel < 0f)
		{
			skillLevel = 0f;
		}
		float num = skillLevel - skillLevel * ToFactor(percent);
		return (num < 0f) ? 0f : num;
	}
}
