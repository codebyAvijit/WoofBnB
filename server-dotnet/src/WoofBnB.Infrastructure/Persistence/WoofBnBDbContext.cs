using Microsoft.EntityFrameworkCore;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Persistence;

public class WoofBnBDbContext : DbContext
{
    public WoofBnBDbContext(DbContextOptions<WoofBnBDbContext> options)
        : base(options)
    {
    }

    public DbSet<PetSitter> PetSitters => Set<PetSitter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PetSitter>(entity =>
        {
            entity.ToTable("PetSitters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Bio)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Latitude)
                .IsRequired();

            entity.Property(x => x.Longitude)
                .IsRequired();

            entity.Property(x => x.WorkingHoursStart)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.WorkingHoursEnd)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Amenities)
                .HasConversion(
                    amenities => string.Join(",", amenities),
                    value => value.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries
                    ).ToList()
                );

            entity.Property(x => x.ProfileImage)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();
        });
    }
}