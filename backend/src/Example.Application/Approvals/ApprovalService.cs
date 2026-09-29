using Example.Application.Common.Interfaces;
using Example.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Example.Application.Approvals;

public interface IApprovalService
{
    Task<IReadOnlyList<ApprovalDocumentDto>> GetAllAsync(CancellationToken ct = default);
    Task<ApprovalResult> ApproveAsync(ApprovalDecisionRequest request, CancellationToken ct = default);
    Task<ApprovalResult> RejectAsync(ApprovalDecisionRequest request, CancellationToken ct = default);
}

public class ApprovalService(IApplicationDbContext db) : IApprovalService
{
    public async Task<IReadOnlyList<ApprovalDocumentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.ApprovalDocuments
            .OrderBy(d => d.Id)
            .Select(d => new ApprovalDocumentDto(d.Id, d.Title, d.Status, d.Reason, d.CreatedAt, d.DecidedAt))
            .ToListAsync(ct);
    }

    public Task<ApprovalResult> ApproveAsync(ApprovalDecisionRequest request, CancellationToken ct = default)
        => DecideAsync(request, ApprovalStatus.Approved, ct);

    public Task<ApprovalResult> RejectAsync(ApprovalDecisionRequest request, CancellationToken ct = default)
        => DecideAsync(request, ApprovalStatus.Rejected, ct);

    private async Task<ApprovalResult> DecideAsync(
        ApprovalDecisionRequest request, ApprovalStatus target, CancellationToken ct)
    {
        if (request.DocumentIds is null || request.DocumentIds.Count == 0)
            return ApprovalResult.Failure(ApprovalErrorCode.EmptySelection, "No documents selected.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return ApprovalResult.Failure(ApprovalErrorCode.ReasonRequired, "Reason is required.");

        var ids = request.DocumentIds.Distinct().ToList();
        var documents = await db.ApprovalDocuments
            .Where(d => ids.Contains(d.Id))
            .ToListAsync(ct);

        if (documents.Count != ids.Count)
            return ApprovalResult.Failure(ApprovalErrorCode.NotFound, "One or more documents were not found.");

        if (documents.Any(d => d.Status != ApprovalStatus.Pending))
            return ApprovalResult.Failure(ApprovalErrorCode.AlreadyDecided, "One or more documents have already been decided.");

        var now = DateTime.UtcNow;
        foreach (var doc in documents)
        {
            doc.Status = target;
            doc.Reason = request.Reason.Trim();
            doc.DecidedAt = now;
        }

        await db.SaveChangesAsync(ct);
        return ApprovalResult.Success(documents.Count);
    }
}
