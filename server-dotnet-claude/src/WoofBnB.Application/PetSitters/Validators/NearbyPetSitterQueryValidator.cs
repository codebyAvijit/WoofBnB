using FluentValidation;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Application.PetSitters.Validators;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.validation.js:nearbyPetSitterSchema.
///
/// Node's schema uses z.coerce.number(): Number(undefined) is NaN, and NaN fails every
/// numeric comparison — so a MISSING lat/lng produces the exact same
/// "must be between ..." message as an out-of-range one, never a separate "required"
/// message. Lat/Lng are therefore validated with a single Must() covering both "missing"
/// and "out of range" as one failure, replicating that.
///
/// Radius is different: Node's z.coerce.number().positive().default(5000) short-circuits
/// to the default when the input is undefined, without ever running .positive() — so a
/// missing radius always passes validation (the 5000 default is applied downstream, in
/// PetSitterService, not here); only a *present* non-positive value fails.
/// </summary>
public class NearbyPetSitterQueryValidator : AbstractValidator<NearbyPetSitterQuery>
{
    public NearbyPetSitterQueryValidator()
    {
        RuleFor(x => x.Lat)
            .Must(lat => lat.HasValue && lat.Value is >= -90 and <= 90)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Lng)
            .Must(lng => lng.HasValue && lng.Value is >= -180 and <= 180)
            .WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Radius)
            .Must(radius => !radius.HasValue || radius.Value > 0)
            .WithMessage("Radius must be greater than 0");
    }
}
