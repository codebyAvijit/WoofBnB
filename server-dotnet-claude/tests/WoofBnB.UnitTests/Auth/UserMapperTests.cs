using WoofBnB.Application.Auth.Mappers;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;

namespace WoofBnB.UnitTests.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.mapper.js:toUserDto exactly.</summary>
public class UserMapperTests
{
    [Fact]
    public void ToDto_MapsAllSixFields_AndNoOthers()
    {
        var user = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Admin",
            Email = "admin@example.com",
            PasswordHash = "should-never-appear-in-the-dto",
            Role = UserRoles.SuperAdmin,
            IsActive = false,
            LastLogin = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc),
        };

        var dto = UserMapper.ToDto(user);

        Assert.Equal("22222222-2222-2222-2222-222222222222", dto.Id);
        Assert.Equal("Admin", dto.Name);
        Assert.Equal("admin@example.com", dto.Email);
        Assert.Equal(UserRoles.SuperAdmin, dto.Role);
        Assert.Equal(user.LastLogin, dto.LastLogin);
        Assert.Equal(user.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void ToDto_NullLastLogin_MapsToNull()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@example.com",
            PasswordHash = "hashed",
            LastLogin = null,
        };

        var dto = UserMapper.ToDto(user);

        Assert.Null(dto.LastLogin);
    }
}
