using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Globalization;
using System.IO;
using Jotunn;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class PluginStorage
{
	private const string ActionLog = "qol-cm.log";

	private const string DebugLog = "qol-cm-debug.log";

	private const string SkillFile = "qol-cm-skill.txt";

	private const string OverwriteFile = "qol-cm-skill-overwrite.txt";

	public static void Action(string message)
	{
		Append("qol-cm.log", message);
	}

	public static void Debug(string message)
	{
		Append("qol-cm-debug.log", message);
	}

	public static float? ReadSkillLoss()
	{
		try
		{
			string path = Path.Combine(Folder(), "qol-cm-skill.txt");
			if (!File.Exists(path))
			{
				return null;
			}
			return SavedSkillLoss.Parse(File.ReadAllText(path));
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not read the saved skill-loss percent: " + ex.Message));
			return null;
		}
	}

	public static bool ReadOverwrite()
	{
		try
		{
			string path = Path.Combine(Folder(), "qol-cm-skill-overwrite.txt");
			if (!File.Exists(path))
			{
				return false;
			}
			return SkillOverwrite.Parse(File.ReadAllText(path));
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not read the skill-loss overwrite: " + ex.Message));
			return false;
		}
	}

	public static void WriteOverwrite(bool overwrite)
	{
		try
		{
			File.WriteAllText(Path.Combine(Folder(), "qol-cm-skill-overwrite.txt"), SkillOverwrite.Format(overwrite));
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not save the skill-loss overwrite: " + ex.Message));
		}
	}

	public static void WriteSkillLoss(float percent)
	{
		try
		{
			File.WriteAllText(Path.Combine(Folder(), "qol-cm-skill.txt"), SavedSkillLoss.Format(percent));
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not save the skill-loss percent: " + ex.Message));
		}
	}

	private static void Append(string name, string message)
	{
		try
		{
			string contents = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " " + message + Environment.NewLine;
			File.AppendAllText(Path.Combine(Folder(), name), contents);
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("Could not write " + name + ": " + ex.Message));
		}
	}

	private static string Folder()
	{
		string directoryName = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
		return string.IsNullOrEmpty(directoryName) ? "." : directoryName;
	}
}
