using Microsoft.EntityFrameworkCore;

namespace Example.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
        => db.Database.EnsureCreatedAsync(ct);
}
