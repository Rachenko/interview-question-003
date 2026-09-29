namespace Example.Domain.Entities;

public class ApprovalDecisionItem
{
    public int DecisionId { get; set; }
    public int DocumentId { get; set; }

    public ApprovalDecision Decision { get; set; } = null!;
    public ApprovalDocument Document { get; set; } = null!;
}
