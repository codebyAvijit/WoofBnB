using WoofBnB.Application.Common.Abstractions;

namespace WoofBnB.Infrastructure.Security;

/// <summary>
/// Mirrors server/src/utils/crypto/password.js. BCrypt.Net-Next (not
/// Microsoft.AspNetCore.Identity's PasswordHasher&lt;T&gt;, which is PBKDF2 and cannot
/// read Node's existing $2b$ hashes) reads and writes the exact bcrypt format Node's
/// npm `bcrypt` package uses, so existing admin credentials keep working unchanged
/// across the migration cutover.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public BCryptPasswordHasher(int workFactor)
    {
        _workFactor = workFactor;
    }

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
