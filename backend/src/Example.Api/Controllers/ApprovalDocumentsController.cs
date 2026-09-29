using Example.Application.Approvals;
using Microsoft.AspNetCore.Mvc;

namespace Example.Api.Controllers;

[ApiController]
[Route("api/approval-documents")]
public class ApprovalDocumentsController(IApprovalService service) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ApprovalDocumentDto>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] ApprovalDecisionRequest request, CancellationToken ct)
        => ToActionResult(await service.ApproveAsync(request, ct));

    [HttpPost("reject")]
    public async Task<IActionResult> Reject([FromBody] ApprovalDecisionRequest request, CancellationToken ct)
        => ToActionResult(await service.RejectAsync(request, ct));

    private IActionResult ToActionResult(ApprovalResult result)
    {
        if (result.IsSuccess)
            return Ok(new { updated = result.UpdatedCount });

        return result.ErrorCode switch
        {
            ApprovalErrorCode.NotFound => NotFound(new { error = result.Error }),
            ApprovalErrorCode.AlreadyDecided => Conflict(new { error = result.Error }),
            _ => BadRequest(new { error = result.Error })
        };
    }
}
