using FluentValidation;
using WoofBnB.Application.Auth.DTOs;

namespace WoofBnB.Application.Auth.Validators;

/// <summary>Mirrors server/src/modules/auth/auth.validation.js:loginSchema exactly.</summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Please provide a valid email address");

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters");
    }
}
