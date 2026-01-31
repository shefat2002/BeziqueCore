namespace BeziqueCore.Core.API.Results;

public class ActionResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }

    public static ActionResult Ok(object? data = null) => new() { Success = true, Data = data };
    public static ActionResult Fail(string message) => new() { Success = false, Message = message };
    public static ActionResult Fail(string message, object? data) => new() { Success = false, Message = message, Data = data };

    public void Match(Action onSuccess, Action<string> onFailure)
    {
        if (Success)
            onSuccess();
        else
            onFailure(Message ?? "Action failed");
    }

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return Success ? onSuccess() : onFailure(Message ?? "Action failed");
    }
}

public class ActionResult<T> : ActionResult
{
    public new T? Data { get; init; }

    public static ActionResult<T> Ok(T? data = default) => new() { Success = true, Data = data };
    public static new ActionResult<T> Fail(string message) => new() { Success = false, Message = message };
    public static ActionResult<T> Fail(string message, T? data) => new() { Success = false, Message = message, Data = data };

    public void Match(Action<T> onSuccess, Action<string> onFailure)
    {
        if (Success)
            onSuccess(Data!);
        else
            onFailure(Message ?? "Action failed");
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return Success ? onSuccess(Data!) : onFailure(Message ?? "Action failed");
    }
}
