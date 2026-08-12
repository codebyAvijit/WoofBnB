using FluentValidation;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Constants;

namespace WoofBnB.Application.PetSitters.Validators;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.validation.js:createPetSitterSchema.
///
/// Message text for Name/Email/Bio/Address/Amenities is pinned to Node's actual Zod v4
/// default wording (captured verbatim from a live differential run against the real
/// Node backend — see parity-tests/PARITY_REPORT.md — not guessed from the Zod docs).
/// Neither Node's schema nor this validator gave these rules a custom message before
/// Phase 8's parity harness found that both sides were silently falling back to their
/// own library's default text, which never matched. Only the specific failure mode each
/// scenario actually exercised is pinned here — e.g. Name's WithMessage covers the
/// MinimumLength(2) failure only (the tested "too short" case), not MaximumLength(50)
/// (never exercised by the parity suite, left as FluentValidation's own default,
/// per the fix's narrow scope).
///
/// Two additions beyond strict Node parity, both already called for in the approved
/// migration plan rather than invented here:
///   - Location.Coordinates range-checked to [-90,90]/[-180,180] (decision D-5). Node
///     never range-checks coordinates on create; SQL Server's geography type hard-errors
///     on an out-of-range point, so without this an invalid coordinate would surface as
///     an unhandled 500 instead of a clean 400.
///   - Address/ProfileImage/WorkingHours.Start/End get a MaximumLength matching the
///     actual DB column width chosen in Phase 3 (decision D-10 already anticipated this
///     for ProfileImage specifically). Node has no length cap on these at all; the
///     alternative is a raw SQL truncation/error for an absurdly long value the approved
///     frontend never sends, which is a worse failure mode than a clean 400.
/// </summary>
public class CreatePetSitterRequestValidator : AbstractValidator<CreatePetSitterRequest>
{
    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[1-9]\d{9,14}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public CreatePetSitterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
                .WithMessage("Too small: expected string to have >=2 characters")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .EmailAddress()
                .WithMessage("Invalid email address");

        RuleFor(x => x.Phone)
            .Matches(PhoneRegex)
            .WithMessage("Enter a valid phone number");

        RuleFor(x => x.Bio)
            .MinimumLength(20)
                .WithMessage("Too small: expected string to have >=20 characters")
            .MaximumLength(1000);

        RuleFor(x => x.Address)
            .MinimumLength(5)
                .WithMessage("Too small: expected string to have >=5 characters")
            .MaximumLength(500);

        RuleFor(x => x.Location)
            .NotNull()
            .WithMessage("Location is required");

        RuleFor(x => x.Location)
            .Custom((location, context) =>
            {
                if (location is null)
                {
                    return;
                }

                if (location.Type != "Point")
                {
                    context.AddFailure("location.type", "Invalid location type");
                }

                if (location.Coordinates.Count != 2)
                {
                    context.AddFailure("location.coordinates", "Coordinates must contain exactly 2 numbers");
                    return;
                }

                var longitude = location.Coordinates[0];
                var latitude = location.Coordinates[1];

                if (longitude is < -180 or > 180)
                {
                    context.AddFailure("location.coordinates", "Longitude must be between -180 and 180");
                }

                if (latitude is < -90 or > 90)
                {
                    context.AddFailure("location.coordinates", "Latitude must be between -90 and 90");
                }
            });

        RuleFor(x => x.WorkingHours)
            .NotNull()
            .WithMessage("Working hours are required");

        RuleFor(x => x.WorkingHours!.Start)
            .NotNull()
            .MaximumLength(10)
            .When(x => x.WorkingHours is not null)
            .OverridePropertyName("WorkingHours.Start");

        RuleFor(x => x.WorkingHours!.End)
            .NotNull()
            .MaximumLength(10)
            .When(x => x.WorkingHours is not null)
            .OverridePropertyName("WorkingHours.End");

        RuleFor(x => x.Amenities)
            .NotNull()
            .WithMessage("Invalid input: expected array, received undefined");

        RuleForEach(x => x.Amenities)
            .Must(amenity => PetSitterAmenities.All.Contains(amenity))
            .WithMessage(
                "Invalid option: expected one of " +
                string.Join("|", PetSitterAmenities.All.Select(a => $"\"{a}\"")))
            .When(x => x.Amenities is not null);

        RuleFor(x => x.ProfileImage)
            .MaximumLength(500)
            .When(x => x.ProfileImage is not null);
    }
}
