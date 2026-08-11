using Microsoft.EntityFrameworkCore;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Persistence;

public class WoofBnBDbContext : DbContext
{
    public WoofBnBDbContext(
        DbContextOptions<WoofBnBDbContext> options)
        : base(options)
    {
    }

    public DbSet<PetSitter> PetSitters => Set<PetSitter>();
}