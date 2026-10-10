namespace ValheimQoLCM.Core;

public sealed class ActionResult<T>
{
	public T Data { get; }

	public string? Error { get; }

	public bool Ok => Error == null;

	private ActionResult(T data, string? error)
	{
		Data = data;
		Error = error;
	}

	public static ActionResult<T> Success(T data)
	{
		return new ActionResult<T>(data, null);
	}

	public static ActionResult<T> Fail(string error)
	{
		return new ActionResult<T>(default(T), error);
	}
}
