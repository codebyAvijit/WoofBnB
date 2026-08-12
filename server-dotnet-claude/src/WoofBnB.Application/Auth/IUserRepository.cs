using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.Auth;

/// <summary>Mirrors server/src/modules/auth/auth.repository.js.</summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task<User> CreateAsync(User user);

    /// <summary>
    /// Persists in-place mutations (e.g. LastLogin) made to an already-loaded User.
    /// Node's authRepository.updateUserLastLogin issues its own findByIdAndUpdate; here
    /// the equivalent is mutate-then-save, which is the idiomatic EF Core pattern for the
    /// same outcome — the caller already holds the tracked entity.
    /// </summary>
    Task SaveChangesAsync();
}
