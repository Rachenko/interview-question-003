using Example.Application.Common.Interfaces;
using Example.Domain.Entities;
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
    private const string CurrentUser = "admin";

    public async Task<IReadOnlyList<ApprovalDocumentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.ApprovalDocuments
            .OrderBy(d => d.Id)
            .Select(d => new ApprovalDocumentDto(
                d.Id,
                d.Title,
                d.Status,
                d.DecisionItems
                    .OrderByDescending(item => item.Decision.DecidedAt)
                    .Select(item => item.Decision.Reason)
                    .FirstOrDefault(),
                d.DecisionItems
                    .OrderByDescending(item => item.Decision.DecidedAt)
                    .Select(item => item.Decision.DecidedBy)
                    .FirstOrDefault(),
                d.CreatedAt,
                d.DecisionItems
                    .OrderByDescending(item => item.Decision.DecidedAt)
                    .Select(item => (DateTime?)item.Decision.DecidedAt)
                    .FirstOrDefault()))
            .ToListAsync(ct);
    }

    public Task<ApprovalResult> ApproveAsync(ApprovalDecisionRequest request, CancellationToken ct = default)
        => DecideAsync(request, ApprovalStatus.Approved, ApprovalAction.Approved, ct);

    public Task<ApprovalResult> RejectAsync(ApprovalDecisionRequest request, CancellationToken ct = default)
        => DecideAsync(request, ApprovalStatus.Rejected, ApprovalAction.Rejected, ct);

    private async Task<ApprovalResult> DecideAsync(
        ApprovalDecisionRequest request,
        ApprovalStatus target,
        ApprovalAction action,
        CancellationToken ct)
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

        var decision = new ApprovalDecision
        {
            Action = action,
            Reason = request.Reason.Trim(),
            DecidedBy = CurrentUser,
            DecidedAt = DateTime.UtcNow
        };

        foreach (var document in documents)
        {
            document.Status = target;
            decision.Items.Add(new ApprovalDecisionItem { DocumentId = document.Id });
        }

        db.ApprovalDecisions.Add(decision);
        await db.SaveChangesAsync(ct);
        return ApprovalResult.Success(documents.Count);
    }
}
