namespace ValheimQoLCM.Core;

/// <summary>
/// Outcome of a panel action. Either <see cref="Data"/> is set and <see cref="Error"/> is null,
/// or <see cref="Error"/> carries the message the console shows. This is the <c>{ data, error }</c>
/// shape every result keeps when it crosses the host boundary.
/// </summary>
/// <typeparam name="T">Payload type on success.</typeparam>
public sealed class ActionResult<T>
{
	/// <summary>Payload on success. Default when <see cref="Ok"/> is false.</summary>
	public T Data { get; }

	/// <summary>Console message on failure. Null on success.</summary>
	public string? Error { get; }

	/// <summary>True when the action was accepted.</summary>
	public bool Ok => Error == null;

	private ActionResult(T data, string? error)
	{
		Data = data;
		Error = error;
	}

	/// <summary>Accepted result carrying <paramref name="data"/>.</summary>
	public static ActionResult<T> Success(T data)
	{
		return new ActionResult<T>(data, null);
	}

	/// <summary>Rejected result carrying the console message <paramref name="error"/>.</summary>
	public static ActionResult<T> Fail(string error)
	{
		return new ActionResult<T>(default!, error);
	}
}
