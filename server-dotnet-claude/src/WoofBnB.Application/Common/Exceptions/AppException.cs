using WoofBnB.Application.Common.Responses;

namespace WoofBnB.Application.Common.Exceptions;

/// <summary>
/// Application/business failure that must produce a controlled API response, mirroring
/// server/src/utils/AppError.js (message, statusCode). ErrorCode is an additive property
/// (decision D-2) and Errors carries field-level validation failures for the 400 case
/// that server/src/middlewares/validate.middleware.js produces directly.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    public IReadOnlyList<ValidationErrorItem>? Errors { get; }

    protected AppException(
        string message,
        int statusCode,
        string errorCode,
        IReadOnlyList<ValidationErrorItem>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public static AppException BadRequest(string message) =>
        new(message, HttpStatusCodes.BadRequest, Responses.ErrorCodes.BadRequest);

    public static AppException Validation(string message, IReadOnlyList<ValidationErrorItem> errors) =>
        new(message, HttpStatusCodes.BadRequest, Responses.ErrorCodes.ValidationError, errors);

    public static AppException Unauthorized(string message) =>
        new(message, HttpStatusCodes.Unauthorized, Responses.ErrorCodes.Unauthorized);

    public static AppException Forbidden(string message) =>
        new(message, HttpStatusCodes.Forbidden, Responses.ErrorCodes.Forbidden);

    public static AppException NotFound(string message) =>
        new(message, HttpStatusCodes.NotFound, Responses.ErrorCodes.NotFound);

    public static AppException Conflict(string message) =>
        new(message, HttpStatusCodes.Conflict, Responses.ErrorCodes.Conflict);
}
