using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Application.PetSitters;

public interface IPetSitterService
{
    Task<PetSitterDto> RegisterAsync(
        CreatePetSitterRequest request);

    Task<List<PetSitterDto>> GetAllAsync();

    Task<List<PetSitterDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters);
}