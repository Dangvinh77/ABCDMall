namespace ABCDMall.Modules.Events.Application.Common;

public enum ApplicationResultStatus
{
    Ok,
    BadRequest,
    NotFound,
    Unauthorized
}

public sealed class ApplicationResult<T>
{
    public ApplicationResultStatus Status { get; init; }
    public string? Error { get; init; }
    public T? Value { get; init; }

    public static ApplicationResult<T> Ok(T value) => new() { Status = ApplicationResultStatus.Ok, Value = value };
    public static ApplicationResult<T> BadRequest(string error) => new() { Status = ApplicationResultStatus.BadRequest, Error = error };
    public static ApplicationResult<T> NotFound(string error) => new() { Status = ApplicationResultStatus.NotFound, Error = error };
    public static ApplicationResult<T> Unauthorized(string error) => new() { Status = ApplicationResultStatus.Unauthorized, Error = error };
}
