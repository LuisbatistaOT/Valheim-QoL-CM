namespace ValheimQoLCM.Core;

/// <summary>The <c>qol-cm-skill-overwrite.txt</c> flag. While on, a death removes no skill level.</summary>
public static class SkillOverwrite
{
	/// <summary>True only for the text <c>on</c>. Anything else, including a missing file, is off.</summary>
	public static bool Parse(string? text)
	{
		return text != null && text.Trim() == "on";
	}

	/// <summary>File text: <c>on</c> or <c>off</c>.</summary>
	public static string Format(bool overwrite)
	{
		return overwrite ? "on" : "off";
	}
}
