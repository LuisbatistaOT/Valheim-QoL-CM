using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>
/// The host-side skill-loss percent file from spec 006. Spec 008 ignores that file in favor of
/// <see cref="SkillOverwrite"/>, but the parsing rules stay so a saved 0 is never read as the default.
/// </summary>
public static class SavedSkillLoss
{
	/// <summary>Vanilla percent used when nothing is saved.</summary>
	public const float DefaultPercent = 5f;

	/// <summary>Percent from the file text, clamped. Null for an empty or non-numeric file.</summary>
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

	/// <summary>File text for a percent, invariant culture.</summary>
	public static string Format(float percent)
	{
		return SkillLoss.ClampPercent(percent).ToString("0.########", CultureInfo.InvariantCulture);
	}

	/// <summary>The saved percent when there is one, including 0. Otherwise the incoming value, clamped.</summary>
	public static float Choose(float? saved, float incoming)
	{
		return saved.HasValue ? saved.Value : SkillLoss.ClampPercent(incoming);
	}
}
