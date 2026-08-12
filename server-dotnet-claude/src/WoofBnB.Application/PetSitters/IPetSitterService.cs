using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Application.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.service.js.</summary>
public interface IPetSitterService
{
    Task<PetSitterDto> RegisterAsync(CreatePetSitterRequest request);

    Task<List<PetSitterDto>> GetAllAsync();

    Task<List<PetSitterDto>> GetNearbyAsync(NearbyPetSitterQuery query);
}
