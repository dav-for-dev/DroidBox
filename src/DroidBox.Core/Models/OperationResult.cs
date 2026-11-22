namespace DroidBox.Core.Models;

public record OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }

    public static OperationResult Ok(string message = "") =>
        new() { Success = true, Message = message };

    public static OperationResult Fail(string message, string? errorCode = null) =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}

public record OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Ok(T data, string message = "") =>
        new() { Success = true, Data = data, Message = message };

    public new static OperationResult<T> Fail(string message, string? errorCode = null) =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}
