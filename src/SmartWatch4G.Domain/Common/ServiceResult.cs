namespace SmartWatch4G.Domain.Common;

/// <summary>
/// Represents the outcome of a service operation — either a success carrying a value,
/// or a failure carrying an error message and optional numeric error code.
/// Use the static factory methods <see cref="Ok"/> and <see cref="Fail"/> to create instances.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class ServiceResult<T>
{
    private ServiceResult(T? value, bool isSuccess, string? error, int errorCode)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>The success value. Only meaningful when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    public T? Value { get; }

    /// <summary><c>true</c> when the operation completed successfully.</summary>
    public bool IsSuccess { get; }

    /// <summary><c>true</c> when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Human-readable error description. Only set on failure.</summary>
    public string? Error { get; }

    /// <summary>
    /// Application-defined error code (e.g. HTTP status or domain code).
    /// 0 when not set.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>Creates a successful result wrapping <paramref name="value"/>.</summary>
    public static ServiceResult<T> Ok(T value) =>
        new(value, true, null, 0);

    /// <summary>Creates a failure result with the given error message and optional code.</summary>
    public static ServiceResult<T> Fail(string error, int errorCode = 0) =>
        new(default, false, error, errorCode);
}
