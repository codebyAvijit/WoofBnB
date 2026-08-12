namespace WoofBnB.Domain.Entities;

/// <summary>
/// One row of a PetSitter's amenities list. Node stores amenities as a plain ordered
/// string array (server/src/modules/petsitter/petsitter.model.js:65-70); SortOrder
/// preserves that order across the relational child table (decision D-12).
/// </summary>
public class PetSitterAmenity
{
    public Guid PetSitterId { get; set; }

    public int SortOrder { get; set; }

    public required string Amenity { get; set; }
}
