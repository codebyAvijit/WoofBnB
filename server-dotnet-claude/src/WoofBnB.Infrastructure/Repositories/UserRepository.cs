using Microsoft.EntityFrameworkCore;
using WoofBnB.Application.Auth;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence;

namespace WoofBnB.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WoofBnBDbContext _context;

    public UserRepository(WoofBnBDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
