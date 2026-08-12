namespace WoofBnB.Application.PetSitters.DTOs;

/// <summary>
/// Mirrors the GeoJSON Point shape server/src/modules/petsitter/petsitter.model.js
/// stores and server/src/modules/petsitter/petsitter.mapper.js echoes verbatim:
/// { type: "Point", coordinates: [longitude, latitude] }. Used for BOTH the create
/// request and the response DTO — do not flatten to Latitude/Longitude (decision D-4).
/// </summary>
public class LocationDto
{
    public string Type { get; set; } = "Point";

    public List<double> Coordinates { get; set; } = [];
}
