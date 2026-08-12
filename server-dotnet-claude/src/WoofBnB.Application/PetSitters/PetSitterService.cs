using NetTopologySuite.Geometries;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Application.PetSitters.Mappers;
using WoofBnB.Domain.Entities;
using WoofBnB.Domain.ValueObjects;

namespace WoofBnB.Application.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.service.js.</summary>
public class PetSitterService : IPetSitterService
{
    // Mirrors server/src/modules/petsitter/petsitter.validation.js:nearbyPetSitterSchema's
    // z.coerce.number().positive().default(5000). Applied here rather than in the
    // validator: Node's default resolves at the validation layer, but FluentValidation
    // rules only produce pass/fail, so the equivalent default is resolved at the one
    // remaining place that actually needs a concrete radius value — before it's handed
    // to the repository's geography query.
    private const double DefaultRadiusInMeters = 5000;

    private readonly IPetSitterRepository _repository;
    private readonly IClock _clock;

    public PetSitterService(IPetSitterRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<PetSitterDto> RegisterAsync(CreatePetSitterRequest request)
    {
        // Node's pre-check uses the trimmed-but-not-lowercased email (see
        // server/src/modules/petsitter/petsitter.service.js:8-10), so a mixed-case
        // duplicate slips past it and only fails later at Mongo's unique index — as an
        // unhandled 500 (decision D-8). Normalizing here, plus the repository's own
        // unique-violation guard for the remaining race window, means every duplicate
        // gets the same clean 409 regardless of casing or timing.
        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _repository.GetByEmailAsync(email);

        if (existing is not null)
        {
            throw AppException.Conflict("A pet sitter with this email already exists");
        }

        var now = _clock.UtcNow;

        var petSitter = new PetSitter
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Bio = request.Bio.Trim(),
            Address = request.Address.Trim(),
            Location = new Point(request.Location!.Coordinates[0], request.Location.Coordinates[1]) { SRID = 4326 },
            WorkingHours = new WorkingHours
            {
                Start = request.WorkingHours!.Start,
                End = request.WorkingHours.End,
            },
            Amenities = request.Amenities!
                .Select((amenity, index) => new PetSitterAmenity { Amenity = amenity, SortOrder = index })
                .ToList(),
            ProfileImage = request.ProfileImage,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var created = await _repository.CreateAsync(petSitter);

        return PetSitterMapper.ToDto(created);
    }

    public async Task<List<PetSitterDto>> GetAllAsync()
    {
        var petSitters = await _repository.GetAllAsync();

        return petSitters.Select(PetSitterMapper.ToDto).ToList();
    }

    public async Task<List<PetSitterDto>> GetNearbyAsync(NearbyPetSitterQuery query)
    {
        var radius = query.Radius ?? DefaultRadiusInMeters;

        var petSitters = await _repository.GetNearbyAsync(query.Lat!.Value, query.Lng!.Value, radius);

        return petSitters.Select(PetSitterMapper.ToDto).ToList();
    }
}
