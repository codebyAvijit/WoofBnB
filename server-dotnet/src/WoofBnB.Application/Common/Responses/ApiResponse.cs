namespace WoofBnB.Application.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public string? ErrorCode { get; init; }

    public ApiResponse(
        bool success,
        string message,
        T? data = default,
        string? errorCode = null)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorCode = errorCode;
    }

    public static ApiResponse<T> Ok(
        string message,
        T data)
    {
        return new ApiResponse<T>(
            true,
            message,
            data);
    }

    public static ApiResponse<T> Fail(
        string message,
        string errorCode)
    {
        return new ApiResponse<T>(
            false,
            message,
            default,
            errorCode);
    }
}