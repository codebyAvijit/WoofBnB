namespace WoofBnB.Application.Common.Abstractions;

/// <summary>
/// Mirrors server/src/utils/crypto/password.js. The implementation (BCrypt.Net-Next in
/// Infrastructure) must produce and verify hashes compatible with the Node backend's
/// bcrypt hashes ($2b$ prefix) — existing admin credentials must keep working across
/// the migration cutover without a forced password reset.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
