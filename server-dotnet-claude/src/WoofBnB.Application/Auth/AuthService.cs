using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Application.Auth.Mappers;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Exceptions;

namespace WoofBnB.Application.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.service.js.</summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IClock clock)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _clock = clock;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequest request)
    {
        // server/src/modules/auth/auth.model.js stores email lowercased; Node's own
        // login lookup never lowercases the incoming value before querying, so a
        // mixed-case email fails to match an existing admin (decision D-7 — fixed here,
        // strictly more permissive, no one loses access).
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            throw AppException.Unauthorized("Invalid email or password");
        }

        // Node checks isActive before comparing the password (auth.service.js:17-19) —
        // preserved in the same order: a disabled account is rejected before bcrypt
        // ever runs, regardless of whether the password would have matched.
        if (!user.IsActive)
        {
            throw AppException.Forbidden("Your account has been disabled");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw AppException.Unauthorized("Invalid email or password");
        }

        user.LastLogin = _clock.UtcNow;
        user.UpdatedAt = _clock.UtcNow;
        await _userRepository.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new LoginResultDto
        {
            User = UserMapper.ToDto(user),
            AccessToken = accessToken,
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw AppException.NotFound("User not found");
        }

        return UserMapper.ToDto(user);
    }
}
