using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.PetSitters.Mappers;

public static class PetSitterMapper
{
    public static PetSitterDto ToDto(PetSitter petSitter)
    {
        return new PetSitterDto
        {
            Id = petSitter.Id,
            Name = petSitter.Name,
            Email = petSitter.Email,
            Phone = petSitter.Phone,
            Bio = petSitter.Bio,
            Address = petSitter.Address,
            Latitude = petSitter.Latitude,
            Longitude = petSitter.Longitude,
            WorkingHoursStart = petSitter.WorkingHoursStart,
            WorkingHoursEnd = petSitter.WorkingHoursEnd,
            Amenities = petSitter.Amenities.ToList(),
            ProfileImage = petSitter.ProfileImage
        };
    }
}