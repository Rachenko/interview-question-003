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

        var decidedAt = DateTime.UtcNow;
        _db.ApprovalDocuments.AddRange(
            new ApprovalDocument { Id = 1, Title = "รายการที่ 1", Status = ApprovalStatus.Pending, CreatedAt = DateTime.UtcNow },
            new ApprovalDocument { Id = 2, Title = "รายการที่ 2", Status = ApprovalStatus.Pending, CreatedAt = DateTime.UtcNow },
            new ApprovalDocument { Id = 3, Title = "รายการที่ 3", Status = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow });
        _db.ApprovalDecisions.Add(new ApprovalDecision
        {
            Id = 1,
            Action = ApprovalAction.Approved,
            Reason = "xxxxx",
            DecidedBy = "admin",
            DecidedAt = decidedAt,
            Items = [new ApprovalDecisionItem { DecisionId = 1, DocumentId = 3 }]
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsAllDocuments_OrderedById()
    {
        var docs = await _service.GetAllAsync();

        docs.Should().HaveCount(3);
        docs.Select(d => d.Id).Should().BeInAscendingOrder();
        docs.Single(d => d.Id == 3).Should().Match<ApprovalDocumentDto>(d =>
            d.Reason == "xxxxx" && d.DecidedBy == "admin" && d.DecidedAt != null);
    }

    [Fact]
    public async Task Approve_PendingDocuments_CreatesOneDecisionAndItems()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([1, 2], "ok"));

        result.IsSuccess.Should().BeTrue();
        result.UpdatedCount.Should().Be(2);
        _db.ApprovalDocuments.Where(d => d.Id != 3)
            .Should().OnlyContain(d => d.Status == ApprovalStatus.Approved);
        var decision = _db.ApprovalDecisions.Single(d => d.Id != 1);
        decision.Action.Should().Be(ApprovalAction.Approved);
        decision.Reason.Should().Be("ok");
        decision.DecidedBy.Should().Be("admin");
        _db.ApprovalDecisionItems.Where(item => item.DecisionId == decision.Id)
            .Select(item => item.DocumentId)
            .Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task Reject_PendingDocument_CreatesRejectedDecision()
    {
        var result = await _service.RejectAsync(new ApprovalDecisionRequest([1], "not valid"));

        result.IsSuccess.Should().BeTrue();
        _db.ApprovalDocuments.Find(1)!.Status.Should().Be(ApprovalStatus.Rejected);
        var decision = _db.ApprovalDecisions.Single(d => d.Id != 1);
        decision.Action.Should().Be(ApprovalAction.Rejected);
        decision.Reason.Should().Be("not valid");
        _db.ApprovalDecisionItems.Single(item => item.DecisionId == decision.Id).DocumentId.Should().Be(1);
    }

    [Fact]
    public async Task Approve_AlreadyDecided_ReturnsConflict()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([3], "again"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
        _db.ApprovalDecisions.Single().Reason.Should().Be("xxxxx");
    }

    [Fact]
    public async Task Approve_MixedWithDecided_FailsWholeBatch()
    {
        var result = await _service.ApproveAsync(new ApprovalDecisionRequest([1, 3], "ok"));

        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
        _db.ApprovalDocuments.Find(1)!.Status.Should().Be(ApprovalStatus.Pending);
        _db.ApprovalDecisions.Should().HaveCount(1);
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
        _db.ApprovalDecisions.Should().HaveCount(1);
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

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ApprovalErrorCode.AlreadyDecided);
    }

    public void Dispose() => _db.Dispose();
}
