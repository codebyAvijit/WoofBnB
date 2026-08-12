using NetTopologySuite.Geometries;
using WoofBnB.Domain.ValueObjects;

namespace WoofBnB.Domain.Entities;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.model.js. Location is stored as a
/// NetTopologySuite Point (X = longitude, Y = latitude — see decision D-4) rather than
/// flattened Latitude/Longitude columns: SQL Server's geography type already stores each
/// coordinate as a CLR double, so Point.X/Point.Y round-trip exactly for the response DTO,
/// and a second denormalized pair would just be a duplicate write path with no benefit.
/// </summary>
public class PetSitter
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }

    public required string Bio { get; set; }

    public required string Address { get; set; }

    public required Point Location { get; set; }

    public required WorkingHours WorkingHours { get; set; }

    public List<PetSitterAmenity> Amenities { get; set; } = [];

    public string? ProfileImage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
