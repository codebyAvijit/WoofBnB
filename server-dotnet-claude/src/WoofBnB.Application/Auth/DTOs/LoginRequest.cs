namespace WoofBnB.Application.Auth.DTOs;

/// <summary>
/// Mirrors server/src/modules/auth/auth.validation.js:loginSchema's input shape.
/// Email/Password are nullable — not defaulted to string.Empty — so a JSON body that
/// omits the key entirely (binds to null) is distinguishable from one that supplies an
/// explicit empty string (binds to ""), exactly mirroring Zod's own two distinct
/// failure paths for "key missing" vs "value present but invalid"
/// (see LoginRequestValidator).
/// </summary>
public class LoginRequest
{
    public string? Email { get; set; }

    public string? Password { get; set; }
}
