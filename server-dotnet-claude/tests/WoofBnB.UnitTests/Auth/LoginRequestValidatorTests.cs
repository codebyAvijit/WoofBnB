using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Application.Auth.Validators;

namespace WoofBnB.UnitTests.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.validation.js:loginSchema.</summary>
public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidLoginRequest_HasNoErrors()
    {
        var result = _validator.Validate(new LoginRequest { Email = "admin@example.com", Password = "password123" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Validate_InvalidEmail_FailsWithNodesExactMessage(string email)
    {
        var result = _validator.Validate(new LoginRequest { Email = email, Password = "password123" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Please provide a valid email address");
    }

    [Fact]
    public void Validate_PasswordShorterThanEightCharacters_FailsWithNodesExactMessage()
    {
        var result = _validator.Validate(new LoginRequest { Email = "admin@example.com", Password = "short1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == "Password must be at least 8 characters");
    }

    [Fact]
    public void Validate_PasswordExactlyEightCharacters_Passes()
    {
        var result = _validator.Validate(new LoginRequest { Email = "admin@example.com", Password = "12345678" });

        Assert.True(result.IsValid);
    }
}
