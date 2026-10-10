namespace ValheimQoLCM.Core;

public static class SkillOverwrite
{
	public static bool Parse(string? text)
	{
		return text != null && text.Trim() == "on";
	}

	public static string Format(bool overwrite)
	{
		return overwrite ? "on" : "off";
	}
}
