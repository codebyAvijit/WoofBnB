using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Application.PetSitters.Mappers;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.PetSitters;

public class PetSitterService : IPetSitterService
{
    private readonly IPetSitterRepository _repository;

    public PetSitterService(IPetSitterRepository repository)
    {
        _repository = repository;
    }

    public async Task<PetSitterDto> RegisterAsync(
        CreatePetSitterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingPetSitter =
            await _repository.GetByEmailAsync(email);

        if (existingPetSitter is not null)
        {
            throw AppException.Conflict(
                "A pet sitter with this email already exists");
        }

        var petSitter = new PetSitter
        {
            Name = request.Name.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Bio = request.Bio.Trim(),
            Address = request.Address.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            WorkingHoursStart = request.WorkingHoursStart,
            WorkingHoursEnd = request.WorkingHoursEnd,
            Amenities = request.Amenities,
            ProfileImage = request.ProfileImage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdPetSitter =
            await _repository.CreateAsync(petSitter);

        return PetSitterMapper.ToDto(createdPetSitter);
    }

    public async Task<List<PetSitterDto>> GetAllAsync()
    {
        var petSitters =
            await _repository.GetAllAsync();

        return petSitters
            .Select(PetSitterMapper.ToDto)
            .ToList();
    }

    public async Task<PetSitterDto> GetByIdAsync(int id)
    {
        var petSitter =
            await _repository.GetByIdAsync(id);

        if (petSitter is null)
        {
            throw AppException.NotFound(
                "Pet sitter not found");
        }

        return PetSitterMapper.ToDto(petSitter);
    }

    public async Task<List<PetSitterDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters)
    {
        var petSitters =
            await _repository.GetNearbyAsync(
                latitude,
                longitude,
                radiusInMeters);

        return petSitters
            .Select(PetSitterMapper.ToDto)
            .ToList();
    }
}