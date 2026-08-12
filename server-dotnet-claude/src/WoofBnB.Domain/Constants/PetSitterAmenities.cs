namespace WoofBnB.Domain.Constants;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.constants.js:PET_SITTER_AMENITIES exactly.
/// CLAUDE.md §11: "These values are part of the API contract. Do not rename them casually."
/// </summary>
public static class PetSitterAmenities
{
    public static readonly IReadOnlyList<string> All =
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
        "Birds",
    ];
}
