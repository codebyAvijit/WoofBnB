using FluentValidation;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Constants;

namespace WoofBnB.Application.PetSitters.Validators;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.validation.js:createPetSitterSchema.
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
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .EmailAddress();

        RuleFor(x => x.Phone)
            .Matches(PhoneRegex)
            .WithMessage("Enter a valid phone number");

        RuleFor(x => x.Bio)
            .MinimumLength(20)
            .MaximumLength(1000);

        RuleFor(x => x.Address)
            .MinimumLength(5)
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
            .WithMessage("Amenities is required");

        RuleForEach(x => x.Amenities)
            .Must(amenity => PetSitterAmenities.All.Contains(amenity))
            .WithMessage("Invalid pet sitter amenity")
            .When(x => x.Amenities is not null);

        RuleFor(x => x.ProfileImage)
            .MaximumLength(500)
            .When(x => x.ProfileImage is not null);
    }
}
