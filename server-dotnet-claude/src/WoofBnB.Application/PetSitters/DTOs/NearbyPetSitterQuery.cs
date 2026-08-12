namespace WoofBnB.Application.PetSitters.DTOs;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.validation.js:nearbyPetSitterSchema's
/// query parameters. Lat/Lng/Radius are nullable so "missing from the query string" is
/// distinguishable from "present with value 0" — required to reproduce Node's exact
/// validation behaviour (see NearbyPetSitterQueryValidator).
/// </summary>
public class NearbyPetSitterQuery
{
    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public double? Radius { get; set; }
}
