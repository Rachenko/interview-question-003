using Example.Application.Approvals;
using Example.Domain.Entities;
using Example.Domain.Enums;
using Example.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Example.UnitTests;

public class ApprovalServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ApprovalService _service;

    public ApprovalServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _service = new ApprovalService(_db);

        _db.ApprovalDocuments.AddRange(
            new ApprovalDocument { Id = 1, Title = "รายการที่ 1", Status = ApprovalStatus.Pending, Reason = "xxxxx", CreatedAt = DateTime.UtcNow },
            new ApprovalDocument { Id = 2, Title = "รายการที่ 2", Status = ApprovalStatus.Pending, Reason = "xxxxx", CreatedAt = DateTime.UtcNow },
            new ApprovalDocument { Id = 3, Title = "รายการที่ 3", Status = ApprovalStatus.Approved, Reason = "xxxxx", CreatedAt = DateTime.UtcNow, DecidedAt = DateTime.UtcNow });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsAllDocuments_OrderedById()
    {
        var docs = await _service.GetAllAsync();
        docs.Should().HaveCount(3);
        docs.Select(d => d.Id).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Approve_PendingDocuments_UpdatesStatusAndReason()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([1, 2], "ok"));

        result.IsSuccess.Should().BeTrue();
        result.UpdatedCount.Should().Be(2);
        var docs = _db.ApprovalDocuments.Where(d => d.Id != 3).ToList();
        docs.Should().OnlyContain(d => d.Status == ApprovalStatus.Approved && d.Reason == "ok" && d.DecidedAt != null);
    }

    [Fact]
    public async Task Reject_PendingDocument_UpdatesStatusToRejected()
    {
        var result = await _service.RejectAsync(new ApprovalDecisionRequest([1], "not valid"));

        result.IsSuccess.Should().BeTrue();
        var doc = _db.ApprovalDocuments.Find(1)!;
        doc.Status.Should().Be(ApprovalStatus.Rejected);
        doc.Reason.Should().Be("not valid");
    }

    [Fact]
    public async Task Approve_AlreadyDecided_ReturnsConflict()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([3], "again"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
        _db.ApprovalDocuments.Find(3)!.Reason.Should().Be("xxxxx");
    }

    [Fact]
    public async Task Approve_MixedWithDecided_FailsWholeBatch()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([1, 3], "ok"));

        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
        _db.ApprovalDocuments.Find(1)!.Status.Should().Be(ApprovalStatus.Pending);
    }

    [Fact]
    public async Task Approve_EmptySelection_ReturnsFailure()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([], "ok"));
        result.ErrorCode.Should().Be(ApprovalErrorCode.EmptySelection);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Approve_BlankReason_ReturnsFailure(string reason)
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([1], reason));
        result.ErrorCode.Should().Be(ApprovalErrorCode.ReasonRequired);
        _db.ApprovalDocuments.Find(1)!.Status.Should().Be(ApprovalStatus.Pending);
    }

    [Fact]
    public async Task Reject_MissingDocument_ReturnsNotFound()
    {
        var result = await _service.RejectAsync(new ApprovalDecisionRequest([999], "ok"));
        result.ErrorCode.Should().Be(ApprovalErrorCode.NotFound);
    }

    [Fact]
    public async Task Reject_AlreadyApproved_ReturnsConflict()
    {
        var result = await _service.RejectAsync(new ApprovalDecisionRequest([3], "no"));
        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
    }

    public void Dispose() => _db.Dispose();
}
