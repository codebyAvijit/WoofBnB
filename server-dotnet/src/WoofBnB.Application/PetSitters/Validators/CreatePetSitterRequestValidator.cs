using FluentValidation;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Application.PetSitters.Validators;

public class CreatePetSitterRequestValidator
    : AbstractValidator<CreatePetSitterRequest>
{
    private static readonly string[] AllowedAmenities =
    [
        "Dog Walking",
        "Medication",
        "24x7 Care",
        "Training",
        "Vet Nearby",
        "Indoor Stay",
        "Outdoor Play",
        "CCTV",
        "Pickup Drop",
        "Large Yard",
        "Small Pets",
        "Cats",
        "Dogs",
        "Birds"
    ];

    public CreatePetSitterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .WithMessage("Name must be between 2 and 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Enter a valid email address");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number");

        RuleFor(x => x.Bio)
            .NotEmpty()
            .MinimumLength(20)
            .MaximumLength(1000)
            .WithMessage("Bio must be between 20 and 1000 characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MinimumLength(5)
            .WithMessage("Address must be at least 5 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.WorkingHoursStart)
            .NotEmpty();

        RuleFor(x => x.WorkingHoursEnd)
            .NotEmpty();

        RuleForEach(x => x.Amenities)
            .Must(amenity => AllowedAmenities.Contains(amenity))
            .WithMessage("Invalid pet sitter amenity");

        RuleFor(x => x.ProfileImage)
            .Must(BeValidProfileImage)
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileImage))
            .WithMessage("ProfileImage must be a valid URL");
    }

    private static bool BeValidProfileImage(string? value)
    {
        return Uri.TryCreate(
            value,
            UriKind.Absolute,
            out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps);
    }
}