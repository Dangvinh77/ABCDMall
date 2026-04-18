namespace ABCDMall.Modules.Users.Application.Common;

public enum ApplicationResultStatus
{
    Ok,
    BadRequest,
    NotFound,
    Unauthorized
}

public class ApplicationResult
{
    public bool Succeeded => Status == ApplicationResultStatus.Ok;

    public ApplicationResultStatus Status { get; init; }

    public string? Error { get; init; }

    public static ApplicationResult Ok()
        => new() { Status = ApplicationResultStatus.Ok };

    public static ApplicationResult BadRequest(string error)
        => new() { Status = ApplicationResultStatus.BadRequest, Error = error };

    public static ApplicationResult NotFound(string error)
        => new() { Status = ApplicationResultStatus.NotFound, Error = error };

    public static ApplicationResult Unauthorized(string error)
        => new() { Status = ApplicationResultStatus.Unauthorized, Error = error };
}

public sealed class ApplicationResult<T> : ApplicationResult
{
    public T? Value { get; init; }

    public static ApplicationResult<T> BadRequest(string error, T? value = default)
        => new() { Status = ApplicationResultStatus.BadRequest, Error = error, Value = value };

    public static ApplicationResult<T> NotFound(string error, T? value = default)
        => new() { Status = ApplicationResultStatus.NotFound, Error = error, Value = value };

    public static ApplicationResult<T> Unauthorized(string error, T? value = default)
        => new() { Status = ApplicationResultStatus.Unauthorized, Error = error, Value = value };

    public static ApplicationResult<T> Ok(T value)
        => new() { Status = ApplicationResultStatus.Ok, Value = value };
}
