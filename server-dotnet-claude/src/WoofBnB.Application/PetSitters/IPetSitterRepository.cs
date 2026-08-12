using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.repository.js.</summary>
public interface IPetSitterRepository
{
    Task<PetSitter?> GetByEmailAsync(string email);

    Task<PetSitter> CreateAsync(PetSitter petSitter);

    Task<List<PetSitter>> GetAllAsync();

    Task<List<PetSitter>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters);
}
