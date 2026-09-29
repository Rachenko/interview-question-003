using Example.Domain.Enums;

namespace Example.Application.Approvals;

public record ApprovalDocumentDto(
    int Id,
    string Title,
    ApprovalStatus Status,
    string? Reason,
    string? DecidedBy,
    DateTime CreatedAt,
    DateTime? DecidedAt);

public record ApprovalDecisionRequest(
    IReadOnlyList<int> DocumentIds,
    string Reason);
