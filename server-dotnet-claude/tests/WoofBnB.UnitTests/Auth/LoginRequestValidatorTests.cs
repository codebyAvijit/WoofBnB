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

    [Fact]
    public void Validate_MissingEmailKey_FailsWithZodsTypeStageMessage_NotTheCustomOne()
    {
        // Confirmed by a live differential run (parity-tests/PARITY_REPORT.md): a wholly
        // MISSING key fails Zod's base string-type check with its own default message,
        // before .email() ever runs — distinct from an empty-string/malformed value,
        // which reaches .email() and gets the custom message (see
        // Validate_InvalidEmail_FailsWithNodesExactMessage above).
        var result = _validator.Validate(new LoginRequest { Email = null, Password = "password123" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Email" && e.ErrorMessage == "Invalid input: expected string, received undefined");
    }

    [Fact]
    public void Validate_MissingPasswordKey_FailsWithZodsTypeStageMessage_NotTheCustomOne()
    {
        var result = _validator.Validate(new LoginRequest { Email = "admin@example.com", Password = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Password" && e.ErrorMessage == "Invalid input: expected string, received undefined");
    }

    [Fact]
    public void Validate_BothKeysMissing_ProducesExactlyOneErrorPerField_NotTwo()
    {
        // Guards against the NotNull + format/length rules both firing for the same
        // null field (Cascade(Stop) is what prevents that) — Node's Zod schema also
        // never produces two issues for one missing key.
        var result = _validator.Validate(new LoginRequest { Email = null, Password = null });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == "Email");
        Assert.Single(result.Errors, e => e.PropertyName == "Password");
    }
}
