using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.Auth.Mappers;

/// <summary>Mirrors server/src/modules/auth/auth.mapper.js:toUserDto.</summary>
public static class UserMapper
{
    public static UserDto ToDto(User user) => new()
    {
        Id = user.Id.ToString(),
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        LastLogin = user.LastLogin,
        CreatedAt = user.CreatedAt,
    };
}
