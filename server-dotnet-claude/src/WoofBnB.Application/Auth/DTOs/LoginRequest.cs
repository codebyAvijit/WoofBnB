namespace WoofBnB.Application.Auth.DTOs;

/// <summary>Mirrors server/src/modules/auth/auth.validation.js:loginSchema's input shape.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
