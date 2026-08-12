using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.PetSitters;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence;

namespace WoofBnB.Infrastructure.Repositories;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.repository.js.</summary>
public class PetSitterRepository : IPetSitterRepository
{
    private readonly WoofBnBDbContext _context;

    public PetSitterRepository(WoofBnBDbContext context)
    {
        _context = context;
    }

    public Task<PetSitter?> GetByEmailAsync(string email) =>
        _context.PetSitters.FirstOrDefaultAsync(x => x.Email == email);

    public async Task<PetSitter> CreateAsync(PetSitter petSitter)
    {
        _context.PetSitters.Add(petSitter);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
        {
            // Closes the race between the service's pre-check and this insert — mirrors
            // decision D-8: Node's own pre-check misses a mixed-case duplicate (its
            // lookup isn't lowercased) and the resulting Mongo E11000 error is unhandled,
            // surfacing as a raw 500. Converting the unique-index violation into the same
            // AppException the pre-check throws gives a clean 409 either way.
            throw AppException.Conflict("A pet sitter with this email already exists");
        }

        return petSitter;
    }

    public Task<List<PetSitter>> GetAllAsync() =>
        _context.PetSitters
            .Include(x => x.Amenities.OrderBy(a => a.SortOrder))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<PetSitter>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters)
    {
        var origin = new Point(longitude, latitude) { SRID = 4326 };

        return _context.PetSitters
            .Include(x => x.Amenities.OrderBy(a => a.SortOrder))
            .Where(x => x.Location.Distance(origin) <= radiusInMeters)
            .OrderBy(x => x.Location.Distance(origin))
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    private static bool IsUniqueEmailViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
