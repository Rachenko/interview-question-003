using Example.Domain.Entities;
using Example.Domain.Enums;

namespace Example.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);

        if (db.ApprovalDocuments.Any())
            return;

        var now = DateTime.UtcNow;
        var documents = new[]
        {
            Pending(1), Decided(2, ApprovalStatus.Approved), Decided(3, ApprovalStatus.Rejected),
            Pending(4), Decided(5, ApprovalStatus.Approved), Decided(6, ApprovalStatus.Rejected),
            Pending(7), Pending(8), Pending(9), Pending(10)
        };

        db.ApprovalDocuments.AddRange(documents);
        await db.SaveChangesAsync(ct);

        return;

        ApprovalDocument Pending(int n) => new()
        {
            Title = $"รายการที่ {n}",
            Status = ApprovalStatus.Pending,
            Reason = "xxxxx",
            CreatedAt = now
        };

        ApprovalDocument Decided(int n, ApprovalStatus status) => new()
        {
            Title = $"รายการที่ {n}",
            Status = status,
            Reason = "xxxxx",
            CreatedAt = now,
            DecidedAt = now
        };
    }
}
