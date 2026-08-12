namespace WoofBnB.Application.Auth.DTOs;

/// <summary>
/// Mirrors server/src/modules/auth/auth.mapper.js:toUserDto exactly — id, name, email,
/// role, lastLogin, createdAt. Deliberately no updatedAt, no isActive, no password.
/// </summary>
public class UserDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Role { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; }
}
