using FluentValidation;
using WoofBnB.Application.Auth.DTOs;

namespace WoofBnB.Application.Auth.Validators;

/// <summary>
/// Mirrors server/src/modules/auth/auth.validation.js:loginSchema exactly, including a
/// distinction confirmed by a live differential run (parity-tests/PARITY_REPORT.md) that
/// the original implementation missed: Zod fails in two different stages with two
/// different messages —
///   1. A wholly MISSING key fails the base string-type check with Zod's own default
///      "Invalid input: expected string, received undefined", before .email()/.min(8)
///      ever run.
///   2. A PRESENT value (even "") that fails the format/length refinement gets the
///      schema's custom message.
/// Each field is therefore two cascading rules: NotNull (stage 1's message) then the
/// format/length check (stage 2's message, only reached once a value is present).
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Invalid input: expected string, received undefined")
            .EmailAddress()
                .WithMessage("Please provide a valid email address");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Invalid input: expected string, received undefined")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters");
    }
}
