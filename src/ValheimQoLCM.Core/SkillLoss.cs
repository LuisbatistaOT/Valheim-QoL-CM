namespace ValheimQoLCM.Core;

public static class SkillLoss
{
	public const float MinPercent = 0f;

	public const float MaxPercent = 100f;

	public const bool LeavesGearUntouched = true;

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

	public static float ToFactor(float percent)
	{
		return ClampPercent(percent) / 100f;
	}

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
