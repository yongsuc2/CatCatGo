using System.Text.Json.Serialization;

namespace CatCatGo.Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
    public T? Data { get; set; }
    public StateDelta? Delta { get; set; }

    public static ApiResponse<T> Ok(T? data = default, StateDelta? delta = null) => new()
    {
        Success = true,
        Data = data,
        Delta = delta,
    };

    public static ApiResponse<T> Fail(string errorCode, string? error = null) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        Error = error ?? errorCode,
    };
}

public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse Ok(StateDelta? delta = null) => new()
    {
        Success = true,
        Delta = delta,
    };

    public new static ApiResponse Fail(string errorCode, string? error = null) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        Error = error ?? errorCode,
    };
}
