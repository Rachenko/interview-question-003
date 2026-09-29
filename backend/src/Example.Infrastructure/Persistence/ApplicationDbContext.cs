using Example.Application.Common.Interfaces;
using Example.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<ApprovalDocument> ApprovalDocuments => Set<ApprovalDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalDocument>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Reason).HasMaxLength(1000);
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(d => d.Status);
        });
    }
}
