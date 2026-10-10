using System.Globalization;

namespace ValheimQoLCM.Core;

public static class SavedSkillLoss
{
	public const float DefaultPercent = 5f;

	public static float? Parse(string? text)
	{
		if (text == null || string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		string s = text.Trim();
		if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return null;
		}
		return SkillLoss.ClampPercent(result);
	}

	public static string Format(float percent)
	{
		return SkillLoss.ClampPercent(percent).ToString("0.########", CultureInfo.InvariantCulture);
	}

	public static float Choose(float? saved, float incoming)
	{
		return saved.HasValue ? saved.Value : SkillLoss.ClampPercent(incoming);
	}
}
