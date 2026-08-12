using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Persistence.Configurations;

/// <summary>See server/src/modules/auth/auth.model.js for the source shape.</summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var allowedRoles = string.Join(", ", UserRoles.All.Select(role => $"'{role}'"));

        builder.ToTable("Users", table => table.HasCheckConstraint("CK_Users_Role", $"[Role] IN ({allowedRoles})"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // bcrypt hashes are ASCII and always 60 characters; varchar(72) leaves headroom
        // for any future hash-scheme migration without another schema change.
        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasColumnType("varchar(72)");

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(UserRoles.Admin);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
