using Example.Application.Common.Interfaces;
using Example.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<ApprovalDocument> ApprovalDocuments => Set<ApprovalDocument>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<ApprovalDecisionItem> ApprovalDecisionItems => Set<ApprovalDecisionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalDocument>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(d => d.Status);
        });

        modelBuilder.Entity<ApprovalDecision>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Action).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(d => d.DecidedBy).HasMaxLength(100).IsRequired();
            entity.Property(d => d.DecidedAt).IsRequired();
            entity.HasIndex(d => d.DecidedAt);
        });

        modelBuilder.Entity<ApprovalDecisionItem>(entity =>
        {
            entity.HasKey(item => new { item.DecisionId, item.DocumentId });
            entity.HasIndex(item => item.DocumentId).IsUnique();
            entity.HasOne(item => item.Decision)
                .WithMany(decision => decision.Items)
                .HasForeignKey(item => item.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Document)
                .WithMany(document => document.DecisionItems)
                .HasForeignKey(item => item.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
