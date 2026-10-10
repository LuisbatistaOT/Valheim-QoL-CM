using System.Collections.Generic;

namespace ValheimQoLCM.Core;

/// <summary>
/// Which saved panel bindings are rewritten to numpad +. <c>KeyCode.Plus</c> never arrives from
/// the keyboard, so a saved <c>Plus</c> binding would leave the panel unopenable.
/// </summary>
public static class PanelHotkey
{
	/// <summary>Key name the binding is rewritten to.</summary>
	public const string DefaultKey = "KeypadPlus";

	/// <summary>
	/// True for the three bindings earlier versions shipped or that cannot fire:
	/// backtick, the dead <c>Plus</c> code, and Left Ctrl with Tab. Anything else is a player's choice.
	/// </summary>
	public static bool IsLegacyDefault(string mainKey, IReadOnlyCollection<string> modifiers)
	{
		int num = modifiers?.Count ?? 0;
		if (num == 0)
		{
			return mainKey == "BackQuote" || mainKey == "Plus";
		}
		if (modifiers == null || num != 1 || mainKey != "Tab")
		{
			return false;
		}
		using (IEnumerator<string> enumerator = modifiers.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				return current == "LeftControl";
			}
		}
		return false;
	}

	/// <summary>True when the binding is an unmodified + key, so Shift with =/+ should also open the panel.</summary>
	public static bool IsPlusBinding(string mainKey, IReadOnlyCollection<string> modifiers)
	{
		return (modifiers == null || modifiers.Count == 0) && (mainKey == "KeypadPlus" || mainKey == "Plus");
	}
}
