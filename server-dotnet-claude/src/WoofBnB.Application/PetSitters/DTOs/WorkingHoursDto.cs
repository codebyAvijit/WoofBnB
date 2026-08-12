namespace WoofBnB.Application.PetSitters.DTOs;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.model.js's workingHours sub-document.</summary>
public class WorkingHoursDto
{
    public string Start { get; set; } = string.Empty;

    public string End { get; set; } = string.Empty;
}
