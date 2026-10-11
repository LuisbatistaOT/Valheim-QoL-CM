using System;
using System.Collections.Generic;
using System.Globalization;

namespace ValheimQoLCM.Core;

/// <summary>
/// The Log tab's session buffer: the newest <see cref="Capacity"/> actions, oldest first,
/// each stamped with the local time the caller passes. Not persisted.
/// </summary>
public static class ActionLog
{
	/// <summary>How many lines the panel holds.</summary>
	public const int Capacity = 40;

	/// <summary>The line Clear log records after emptying the buffer.</summary>
	public const string ClearedMessage = "Log cleared.";

	private static readonly List<string> Buffer = new List<string>();

	/// <summary>Lines held right now, oldest first, already formatted.</summary>
	public static IReadOnlyList<string> Entries => Buffer;

	/// <summary>How many lines are held.</summary>
	public static int Count => Buffer.Count;

	/// <summary><c>[HH:mm:ss] message</c>, 24-hour, zero-padded, invariant culture.</summary>
	public static string Format(DateTime localTime, string message)
	{
		return "[" + localTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message;
	}

	/// <summary>Record one action. A blank message is ignored and returns false.</summary>
	public static bool Add(DateTime localTime, string? message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return false;
		}
		Buffer.Add(Format(localTime, message.Trim()));
		while (Buffer.Count > Capacity)
		{
			Buffer.RemoveAt(0);
		}
		return true;
	}

	/// <summary>Drop every line.</summary>
	public static void Clear()
	{
		Buffer.Clear();
	}
}
