using WoofBnB.Domain.Constants;

namespace WoofBnB.Domain.Entities;

/// <summary>Mirrors server/src/modules/auth/auth.model.js.</summary>
public class User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public string Role { get; set; } = UserRoles.Admin;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
