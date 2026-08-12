namespace WoofBnB.Application.Common;

/// <summary>
/// Mirrors server/src/constants/httpStatus.js so Application stays framework-independent
/// (no Microsoft.AspNetCore.Http reference) while using the same status values as the Node backend.
/// </summary>
public static class HttpStatusCodes
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int NoContent = 204;

    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;

    public const int InternalServerError = 500;
}
