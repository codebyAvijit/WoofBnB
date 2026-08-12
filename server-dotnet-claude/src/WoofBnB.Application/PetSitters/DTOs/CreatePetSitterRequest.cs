namespace WoofBnB.Application.PetSitters.DTOs;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.validation.js:createPetSitterSchema's input shape.</summary>
public class CreatePetSitterRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public LocationDto? Location { get; set; }

    public WorkingHoursDto? WorkingHours { get; set; }

    public List<string>? Amenities { get; set; }

    public string? ProfileImage { get; set; }
}
