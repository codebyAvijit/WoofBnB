using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.PetSitters.Mappers;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.mapper.js:toPetSitterDto. Location.X is
/// longitude, Location.Y is latitude (decision D-4) — coordinates are re-emitted as
/// [longitude, latitude], matching the GeoJSON order Node stores and returns.
/// </summary>
public static class PetSitterMapper
{
    public static PetSitterDto ToDto(PetSitter petSitter) => new()
    {
        Id = petSitter.Id.ToString(),
        Name = petSitter.Name,
        Email = petSitter.Email,
        Phone = petSitter.Phone,
        Bio = petSitter.Bio,
        Address = petSitter.Address,
        Location = new LocationDto
        {
            Type = "Point",
            Coordinates = [petSitter.Location.X, petSitter.Location.Y],
        },
        WorkingHours = new WorkingHoursDto
        {
            Start = petSitter.WorkingHours.Start,
            End = petSitter.WorkingHours.End,
        },
        Amenities = petSitter.Amenities
            .OrderBy(a => a.SortOrder)
            .Select(a => a.Amenity)
            .ToList(),
        ProfileImage = petSitter.ProfileImage,
        CreatedAt = petSitter.CreatedAt,
        UpdatedAt = petSitter.UpdatedAt,
    };
}
