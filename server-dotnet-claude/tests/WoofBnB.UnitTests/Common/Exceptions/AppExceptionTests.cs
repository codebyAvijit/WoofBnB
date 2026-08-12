using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.UnitTests.Common.Exceptions;

public class AppExceptionTests
{
    [Fact]
    public void Conflict_SetsStatus409AndConflictErrorCode()
    {
        var exception = AppException.Conflict("A pet sitter with this email already exists");

        Assert.Equal(HttpStatusCodes.Conflict, exception.StatusCode);
        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal("A pet sitter with this email already exists", exception.Message);
        Assert.Null(exception.Errors);
    }

    [Fact]
    public void NotFound_SetsStatus404AndNotFoundErrorCode()
    {
        var exception = AppException.NotFound("Pet sitter not found");

        Assert.Equal(HttpStatusCodes.NotFound, exception.StatusCode);
        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public void Unauthorized_SetsStatus401AndUnauthorizedErrorCode()
    {
        var exception = AppException.Unauthorized("Invalid email or password");

        Assert.Equal(HttpStatusCodes.Unauthorized, exception.StatusCode);
        Assert.Equal(ErrorCodes.Unauthorized, exception.ErrorCode);
    }

    [Fact]
    public void Forbidden_SetsStatus403AndForbiddenErrorCode()
    {
        var exception = AppException.Forbidden("Your account has been disabled");

        Assert.Equal(HttpStatusCodes.Forbidden, exception.StatusCode);
        Assert.Equal(ErrorCodes.Forbidden, exception.ErrorCode);
    }

    [Fact]
    public void Validation_SetsStatus400AndCarriesFieldErrors()
    {
        var errors = new List<ValidationErrorItem>
        {
            new() { Field = "email", Message = "Invalid email address" },
        };

        var exception = AppException.Validation("Validation Failed", errors);

        Assert.Equal(HttpStatusCodes.BadRequest, exception.StatusCode);
        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
        Assert.Same(errors, exception.Errors);
    }
}
