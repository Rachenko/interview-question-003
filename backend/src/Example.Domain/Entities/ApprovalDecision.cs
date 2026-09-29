using Example.Domain.Enums;

namespace Example.Domain.Entities;

public class ApprovalDecision
{
    public int Id { get; set; }
    public ApprovalAction Action { get; set; }
    public required string Reason { get; set; }
    public required string DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; }

    public ICollection<ApprovalDecisionItem> Items { get; set; } = [];
}
