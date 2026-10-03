namespace ValheimQoLCM.Core;

/// <summary>Result envelope for a panel action. <c>Error</c> is null when the action is accepted.</summary>
public sealed class ActionResult<T>
{
    private ActionResult(T data, string error)
    {
        Data = data;
        Error = error;
    }

    /// <summary>Payload when <see cref="Ok"/> is true.</summary>
    public T Data { get; }

    /// <summary>Failure reason. Null when the action is accepted.</summary>
    public string Error { get; }

    /// <summary>True when <see cref="Error"/> is null.</summary>
    public bool Ok => Error == null;

    /// <summary>Creates a successful result.</summary>
    public static ActionResult<T> Success(T data) => new ActionResult<T>(data, null);

    /// <summary>Creates a failed result and carries no data.</summary>
    public static ActionResult<T> Fail(string error) => new ActionResult<T>(default(T), error);
}
