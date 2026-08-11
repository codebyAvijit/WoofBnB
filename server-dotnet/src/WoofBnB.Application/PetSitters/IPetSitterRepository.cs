using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.PetSitters;

public interface IPetSitterRepository
{
    Task<PetSitter> CreateAsync(PetSitter petSitter);

    Task<PetSitter?> GetByEmailAsync(string email);

    Task<List<PetSitter>> GetAllAsync();

    Task<PetSitter?> GetByIdAsync(int id);

    Task<List<PetSitter>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters);
}