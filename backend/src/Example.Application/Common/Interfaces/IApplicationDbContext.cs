using Example.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApprovalDocument> ApprovalDocuments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
