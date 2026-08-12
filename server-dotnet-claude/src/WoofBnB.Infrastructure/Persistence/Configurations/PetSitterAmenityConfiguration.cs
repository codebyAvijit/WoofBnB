using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Persistence.Configurations;

public class PetSitterAmenityConfiguration : IEntityTypeConfiguration<PetSitterAmenity>
{
    public void Configure(EntityTypeBuilder<PetSitterAmenity> builder)
    {
        builder.ToTable("PetSitterAmenities", table => table.HasCheckConstraint(
            "CK_PetSitterAmenities_Amenity",
            BuildAllowedAmenitiesCheck()));

        builder.HasKey(x => new { x.PetSitterId, x.SortOrder });

        builder.Property(x => x.Amenity)
            .IsRequired()
            .HasMaxLength(50);
    }

    private static string BuildAllowedAmenitiesCheck()
    {
        var values = string.Join(
            ", ",
            PetSitterAmenities.All.Select(amenity => $"'{amenity.Replace("'", "''")}'"));

        return $"[Amenity] IN ({values})";
    }
}
