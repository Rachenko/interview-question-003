using Example.Domain.Enums;

namespace Example.Domain.Entities;

public class ApprovalDocument
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
