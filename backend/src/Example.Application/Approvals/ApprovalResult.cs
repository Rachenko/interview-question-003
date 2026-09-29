namespace Example.Application.Approvals;

public enum ApprovalErrorCode
{
    None,
    EmptySelection,
    ReasonRequired,
    NotFound,
    AlreadyDecided
}

public record ApprovalResult(ApprovalErrorCode ErrorCode, string? Error, int UpdatedCount)
{
    public bool IsSuccess => ErrorCode == ApprovalErrorCode.None;

    public static ApprovalResult Success(int updated) => new(ApprovalErrorCode.None, null, updated);

    public static ApprovalResult Failure(ApprovalErrorCode code, string error) => new(code, error, 0);
}
