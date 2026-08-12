namespace WoofBnB.Application.Common.Responses;

/// <summary>
/// Mirrors the shape produced by server/src/middlewares/validate.middleware.js:
/// result.error.issues.map(issue => ({ field: issue.path.join("."), message: issue.message })).
/// </summary>
public sealed class ValidationErrorItem
{
    public required string Field { get; init; }

    public required string Message { get; init; }
}
