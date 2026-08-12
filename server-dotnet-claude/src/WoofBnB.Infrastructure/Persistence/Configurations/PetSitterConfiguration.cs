using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Persistence.Configurations;

/// <summary>
/// See server/src/modules/petsitter/petsitter.model.js for the source shape. The spatial
/// index on Location is created via raw SQL in the migration, not here — EF Core's SQL
/// Server provider has no fluent API for CREATE SPATIAL INDEX (audit decision D-4).
/// </summary>
public class PetSitterConfiguration : IEntityTypeConfiguration<PetSitter>
{
    public void Configure(EntityTypeBuilder<PetSitter> builder)
    {
        builder.ToTable("PetSitters");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Bio)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasColumnType("geography");

        builder.ComplexProperty(x => x.WorkingHours, workingHours =>
        {
            workingHours.Property(w => w.Start)
                .HasColumnName("WorkingHoursStart")
                .IsRequired()
                .HasMaxLength(10);

            workingHours.Property(w => w.End)
                .HasColumnName("WorkingHoursEnd")
                .IsRequired()
                .HasMaxLength(10);
        });

        builder.Property(x => x.ProfileImage)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasMany(x => x.Amenities)
            .WithOne()
            .HasForeignKey(a => a.PetSitterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
