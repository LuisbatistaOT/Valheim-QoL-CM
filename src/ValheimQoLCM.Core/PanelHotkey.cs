using System.Collections.Generic;

namespace ValheimQoLCM.Core;

/// <summary>Which saved panel bindings are the old defaults that clash with another plugin.</summary>
public static class PanelHotkey
{
    /// <summary>Unity key name for the + key. A later rebind is left alone.</summary>
    public const string DefaultKey = "Plus";

    /// <summary>True for an unbound-style backtick or the older Ctrl+Tab default.</summary>
    public static bool IsLegacyDefault(string mainKey, IReadOnlyCollection<string> modifiers)
    {
        var count = modifiers == null ? 0 : modifiers.Count;
        if (count == 0)
        {
            return mainKey == "BackQuote";
        }

        if (modifiers == null || count != 1 || mainKey != "Tab")
        {
            return false;
        }

        foreach (var modifier in modifiers)
        {
            return modifier == "LeftControl";
        }

        return false;
    }
}
