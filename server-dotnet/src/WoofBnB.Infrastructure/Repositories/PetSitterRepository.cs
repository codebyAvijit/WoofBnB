using Microsoft.EntityFrameworkCore;
using WoofBnB.Application.PetSitters;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence;

namespace WoofBnB.Infrastructure.Repositories;

public class PetSitterRepository : IPetSitterRepository
{
    private readonly WoofBnBDbContext _context;

    public PetSitterRepository(WoofBnBDbContext context)
    {
        _context = context;
    }

    public async Task<PetSitter> CreateAsync(PetSitter petSitter)
    {
        _context.PetSitters.Add(petSitter);
        await _context.SaveChangesAsync();

        return petSitter;
    }

    public async Task<PetSitter?> GetByEmailAsync(string email)
    {
        return await _context.PetSitters
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<List<PetSitter>> GetAllAsync()
    {
        return await _context.PetSitters
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<PetSitter?> GetByIdAsync(int id)
    {
        return await _context.PetSitters
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PetSitter>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters)
    {
        // Temporary implementation.
        // We will replace this with SQL Server spatial querying
        // after the basic CRUD flow is working.

        return await _context.PetSitters
            .ToListAsync();
    }
}