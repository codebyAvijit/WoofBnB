namespace WoofBnB.Application.Common.Responses;

public class ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;

    public ErrorResponse(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
}