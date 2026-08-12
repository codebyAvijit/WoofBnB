using Microsoft.EntityFrameworkCore;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence.Configurations;

namespace WoofBnB.Infrastructure.Persistence;

public class WoofBnBDbContext : DbContext
{
    public WoofBnBDbContext(DbContextOptions<WoofBnBDbContext> options)
        : base(options)
    {
    }

    public DbSet<PetSitter> PetSitters => Set<PetSitter>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PetSitterConfiguration());
        modelBuilder.ApplyConfiguration(new PetSitterAmenityConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
