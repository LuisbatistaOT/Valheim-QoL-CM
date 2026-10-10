using System.Collections.Generic;

namespace ValheimQoLCM.Core;

public static class PanelHotkey
{
	public const string DefaultKey = "KeypadPlus";

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

	public static bool IsPlusBinding(string mainKey, IReadOnlyCollection<string> modifiers)
	{
		return (modifiers == null || modifiers.Count == 0) && (mainKey == "KeypadPlus" || mainKey == "Plus");
	}
}
