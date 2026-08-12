using WoofBnB.Application.Auth.DTOs;

namespace WoofBnB.Application.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.service.js.</summary>
public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequest request);

    Task<UserDto> GetCurrentUserAsync(Guid userId);
}
