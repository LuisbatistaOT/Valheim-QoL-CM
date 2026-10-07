using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>The host's saved skill-loss percent. Zero is a real setting, not a missing one.</summary>
public static class SavedSkillLoss
{
    /// <summary>Percent used when the host has never saved one.</summary>
    public const float DefaultPercent = 5f;

    /// <summary>Reads a saved percent. Blank or invalid text means nothing was saved.</summary>
    public static float? Parse(string? text)
    {
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        if (!float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return null;
        }

        return SkillLoss.ClampPercent(value);
    }

    /// <summary>Writes a percent so a later read returns the same value, including 0.</summary>
    public static string Format(float percent)
    {
        return SkillLoss.ClampPercent(percent).ToString("0.########", CultureInfo.InvariantCulture);
    }

    /// <summary>A saved percent wins, including 0. With nothing saved, the incoming value is clamped.</summary>
    public static float Choose(float? saved, float incoming)
    {
        return saved.HasValue ? saved.Value : SkillLoss.ClampPercent(incoming);
    }
}
