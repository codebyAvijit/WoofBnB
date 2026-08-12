using NSubstitute;
using WoofBnB.Application.Auth;
using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;
using WoofBnB.UnitTests.TestSupport;

namespace WoofBnB.UnitTests.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.service.js.</summary>
public class AuthServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_userRepository, _passwordHasher, _tokenService, new FakeClock(FixedNow));
    }

    private static User NewUser(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Admin",
        Email = "admin@example.com",
        PasswordHash = "hashed",
        Role = UserRoles.Admin,
        IsActive = isActive,
        LastLogin = null,
        CreatedAt = FixedNow.AddDays(-30),
        UpdatedAt = FixedNow.AddDays(-30),
    };

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUserAndAccessToken_AndUpdatesLastLogin()
    {
        var user = NewUser();
        _userRepository.GetByEmailAsync("admin@example.com").Returns(user);
        _passwordHasher.Verify("password123", user.PasswordHash).Returns(true);
        _tokenService.GenerateAccessToken(user).Returns("signed.jwt.token");

        var result = await _service.LoginAsync(new LoginRequest { Email = "admin@example.com", Password = "password123" });

        Assert.Equal("signed.jwt.token", result.AccessToken);
        Assert.Equal(user.Id.ToString(), result.User.Id);
        Assert.Equal(FixedNow, user.LastLogin);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmailToLowercaseBeforeLookup()
    {
        // Decision D-7: Node's own lookup is case-sensitive against lowercased storage
        // and silently fails for "Admin@Example.com" — fixed here.
        var user = NewUser();
        _userRepository.GetByEmailAsync("admin@example.com").Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _service.LoginAsync(new LoginRequest { Email = "Admin@Example.com", Password = "password123" });

        await _userRepository.Received(1).GetByEmailAsync("admin@example.com");
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized_WithNodesExactMessage()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = "password123" }));

        Assert.Equal(401, exception.StatusCode);
        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized_WithNodesExactMessage()
    {
        var user = NewUser();
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "admin@example.com", Password = "wrong" }));

        Assert.Equal(401, exception.StatusCode);
        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_DisabledAccount_ThrowsForbidden_BeforeCheckingPassword()
    {
        // server/src/modules/auth/auth.service.js:17-19 checks isActive BEFORE comparing
        // the password — preserved in the same order, verified here by asserting the
        // password hasher is never even called.
        var user = NewUser(isActive: false);
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "admin@example.com", Password = "correct-password" }));

        Assert.Equal(403, exception.StatusCode);
        Assert.Equal("Your account has been disabled", exception.Message);
        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetCurrentUserAsync_ExistingUser_ReturnsMappedDto()
    {
        var user = NewUser();
        _userRepository.GetByIdAsync(user.Id).Returns(user);

        var dto = await _service.GetCurrentUserAsync(user.Id);

        Assert.Equal(user.Id.ToString(), dto.Id);
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal(user.Role, dto.Role);
    }

    [Fact]
    public async Task GetCurrentUserAsync_UnknownUser_ThrowsNotFound_WithNodesExactMessage()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var exception = await Assert.ThrowsAsync<AppException>(() => _service.GetCurrentUserAsync(Guid.NewGuid()));

        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("User not found", exception.Message);
    }
}
