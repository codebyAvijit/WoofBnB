namespace WoofBnB.Application.PetSitters.DTOs;

public class PetSitterDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string WorkingHoursStart { get; set; } = string.Empty;

    public string WorkingHoursEnd { get; set; } = string.Empty;

    public List<string> Amenities { get; set; } = [];

    public string? ProfileImage { get; set; }
}