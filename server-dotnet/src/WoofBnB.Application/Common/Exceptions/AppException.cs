namespace WoofBnB.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    public AppException(
        string message,
        int statusCode,
        string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public static AppException Conflict(string message)
    {
        return new AppException(
            message,
            409,
            "CONFLICT");
    }

    public static AppException NotFound(string message)
    {
        return new AppException(
            message,
            404,
            "NOT_FOUND");
    }
}