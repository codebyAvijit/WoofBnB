namespace WoofBnB.Application.Auth.DTOs;

/// <summary>Mirrors server/src/modules/auth/auth.service.js:login's returned { user, accessToken }.</summary>
public class LoginResultDto
{
    public required UserDto User { get; set; }

    public required string AccessToken { get; set; }
}
