namespace SmartWatch4G.Domain.Common;

public sealed class ServiceResult<T>
{
    public bool IsSuccess  { get; }
    public bool IsFailure  => !IsSuccess;
    public T?   Value      { get; }
    public string? Error   { get; }
    public int  ErrorCode  { get; }

    private ServiceResult(bool ok, T? value, string? error, int errorCode)
    {
        IsSuccess = ok;
        Value     = value;
        Error     = error;
        ErrorCode = errorCode;
    }

    public static ServiceResult<T> Ok(T? value) =>
        new(true, value, null, 0);

    public static ServiceResult<T> Fail(string error, int errorCode = 0) =>
        new(false, default, error, errorCode);
}
