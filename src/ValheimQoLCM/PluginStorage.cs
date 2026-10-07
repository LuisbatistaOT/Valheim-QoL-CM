using System;
using System.Globalization;
using System.IO;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Action log, debug log, and the host skill-loss file beside the plugin DLL.</summary>
public static class PluginStorage
{
    private const string ActionLog = "qol-cm.log";
    private const string DebugLog = "qol-cm-debug.log";
    private const string SkillFile = "qol-cm-skill.txt";

    /// <summary>Appends one panel result. The text matches the action console.</summary>
    public static void Action(string message)
    {
        Append(ActionLog, message);
    }

    /// <summary>Appends a diagnostic line that is not shown on the panel.</summary>
    public static void Debug(string message)
    {
        Append(DebugLog, message);
    }

    /// <summary>Reads the host skill-loss file. Missing or invalid text means unset.</summary>
    public static float? ReadSkillLoss()
    {
        try
        {
            var path = Path.Combine(Folder(), SkillFile);
            if (!File.Exists(path))
            {
                return null;
            }

            return SavedSkillLoss.Parse(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            Jotunn.Logger.LogWarning("Could not read the saved skill-loss percent: " + ex.Message);
            return null;
        }
    }

    /// <summary>Writes the host skill-loss file, including 0.</summary>
    public static void WriteSkillLoss(float percent)
    {
        try
        {
            File.WriteAllText(Path.Combine(Folder(), SkillFile), SavedSkillLoss.Format(percent));
        }
        catch (Exception ex)
        {
            Jotunn.Logger.LogWarning("Could not save the skill-loss percent: " + ex.Message);
        }
    }

    private static void Append(string name, string message)
    {
        try
        {
            var line = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                + " "
                + message
                + Environment.NewLine;
            File.AppendAllText(Path.Combine(Folder(), name), line);
        }
        catch (Exception ex)
        {
            Jotunn.Logger.LogWarning("Could not write " + name + ": " + ex.Message);
        }
    }

    private static string Folder()
    {
        var folder = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        return string.IsNullOrEmpty(folder) ? "." : folder;
    }
}
