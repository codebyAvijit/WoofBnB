namespace WoofBnB.Application.PetSitters.DTOs;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.mapper.js:toPetSitterDto exactly.</summary>
public class PetSitterDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }

    public required string Bio { get; set; }

    public required string Address { get; set; }

    public required LocationDto Location { get; set; }

    public required WorkingHoursDto WorkingHours { get; set; }

    public required List<string> Amenities { get; set; }

    public string? ProfileImage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
