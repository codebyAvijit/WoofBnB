using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence;
using WoofBnB.Infrastructure.Security;

// Mirrors server/src/scripts/seedAdmin.js: if an admin with the configured email already
// exists, log and exit 0 (idempotent — safe to run repeatedly); otherwise hash the
// configured password and insert a new admin.

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets(typeof(Program).Assembly)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Missing required configuration: ConnectionStrings:DefaultConnection.");
    return 1;
}

var adminName = configuration["Seed:Admin:Name"];
var adminEmail = configuration["Seed:Admin:Email"]?.Trim().ToLowerInvariant();
var adminPassword = configuration["Seed:Admin:Password"];
var bcryptWorkFactor = configuration.GetValue("Security:BcryptWorkFactor", 10);

if (string.IsNullOrWhiteSpace(adminName) || string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
{
    Console.Error.WriteLine("Missing required configuration: Seed:Admin:Name, Seed:Admin:Email, and Seed:Admin:Password must all be set.");
    return 1;
}

var options = new DbContextOptionsBuilder<WoofBnBDbContext>()
    .UseSqlServer(connectionString, sql => sql.UseNetTopologySuite())
    .Options;

try
{
    using var db = new WoofBnBDbContext(options);

    var existingAdmin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

    if (existingAdmin is not null)
    {
        Console.WriteLine("Admin already exists.");
        return 0;
    }

    var hasher = new BCryptPasswordHasher(bcryptWorkFactor);
    var now = DateTime.UtcNow;

    db.Users.Add(new User
    {
        Id = Guid.CreateVersion7(),
        Name = adminName,
        Email = adminEmail,
        PasswordHash = hasher.Hash(adminPassword),
        Role = UserRoles.Admin,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now,
    });

    await db.SaveChangesAsync();

    Console.WriteLine("Admin seeded successfully.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to seed admin: {ex.Message}");
    return 1;
}
