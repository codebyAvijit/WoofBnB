using FluentValidation;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Application.PetSitters.Validators;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.validation.js:nearbyPetSitterSchema.
///
/// Node's schema uses z.coerce.number(): Number(undefined) is NaN. Confirmed by a live
/// differential run (parity-tests/PARITY_REPORT.md), this is NOT one uniform failure —
/// Zod actually fails in two distinct stages, each with its own message:
///   1. The coercion/type stage: a MISSING (or non-numeric) value produces
///      "Invalid input: expected number, received NaN", before .min()/.max() ever runs.
///   2. The range-refinement stage: a value that DID coerce to a real number, but is
///      out of range, produces the custom "must be between ..." message.
/// Lat/Lng are therefore two cascading rules each — NotNull (stage 1's message) then a
/// range Must() (stage 2's message, only reached once a value is actually present).
///
/// Radius is different: Node's z.coerce.number().positive().default(5000) short-circuits
/// to the default when the input is undefined, without ever running .positive() — so a
/// missing radius always passes validation (the 5000 default is applied downstream, in
/// PetSitterService, not here); only a *present* non-positive value fails. This is
/// unchanged by the Phase 8 fix — a malformed (non-numeric) radius string is a separate,
/// already-documented gap (see PARITY_REPORT.md's "Phase-5-known-gap" entry) involving a
/// genuine status-code difference, not a message-wording one, and is out of scope here.
/// </summary>
public class NearbyPetSitterQueryValidator : AbstractValidator<NearbyPetSitterQuery>
{
    public NearbyPetSitterQueryValidator()
    {
        RuleFor(x => x.Lat)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Invalid input: expected number, received NaN")
            .Must(lat => lat!.Value is >= -90 and <= 90)
                .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Lng)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Invalid input: expected number, received NaN")
            .Must(lng => lng!.Value is >= -180 and <= 180)
                .WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Radius)
            .Must(radius => !radius.HasValue || radius.Value > 0)
            .WithMessage("Radius must be greater than 0");
    }
}
