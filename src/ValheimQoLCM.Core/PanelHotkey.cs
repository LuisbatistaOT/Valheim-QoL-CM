using System.Collections.Generic;

namespace ValheimQoLCM.Core;

/// <summary>Which saved panel bindings are the old defaults that clash with another plugin.</summary>
public static class PanelHotkey
{
    /// <summary>Unity key name stored for the + key. Numpad + is a real key. KeyCode.Plus is not.</summary>
    public const string DefaultKey = "KeypadPlus";

    /// <summary>True for an unbound-style backtick or the older Ctrl+Tab default.</summary>
    public static bool IsLegacyDefault(string mainKey, IReadOnlyCollection<string> modifiers)
    {
        var count = modifiers == null ? 0 : modifiers.Count;
        if (count == 0)
        {
            return mainKey == "BackQuote" || mainKey == "Plus";
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

    /// <summary>True when the binding is the + default, which must listen for the keys Valheim actually reports.</summary>
    public static bool IsPlusBinding(string mainKey, IReadOnlyCollection<string> modifiers)
    {
        var count = modifiers == null ? 0 : modifiers.Count;
        return count == 0 && (mainKey == DefaultKey || mainKey == "Plus");
    }
}
