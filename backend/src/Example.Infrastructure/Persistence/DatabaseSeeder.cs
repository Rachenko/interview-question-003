using Example.Domain.Entities;
using Example.Domain.Enums;

namespace Example.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);

        if (db.ApprovalDocuments.Any())
            return;

        var now = DateTime.UtcNow;
        var documents = new[]
        {
            Doc(1, "ใบขออนุมัติเบิกจ่ายค่าเดินทางเข้าพบลูกค้า จ.เชียงใหม่", ApprovalStatus.Pending, null),
            Doc(2, "ใบสั่งซื้อโน้ตบุ๊กสำหรับพนักงานใหม่ 3 เครื่อง", ApprovalStatus.Approved, "อยู่ในงบประมาณที่อนุมัติไว้ของไตรมาสนี้"),
            Doc(3, "แบบขอลาพักร้อนล่วงหน้า 10 วัน เดือนตุลาคม", ApprovalStatus.Rejected, "ช่วงดังกล่าวมีงานประจำปี กรุณาเลื่อนเป็นเดือนพฤศจิกายน"),
            Doc(4, "ใบขออนุมัติจ้างพนักงานประจำตำแหน่ง Developer 1 อัตรา", ApprovalStatus.Pending, null),
            Doc(5, "ใบเบิกค่าอบรมออนไลน์หลักสูตร Azure Fundamentals", ApprovalStatus.Approved, "เป็นไปตามแผนพัฒนาบุคลากรประจำปี"),
            Doc(6, "แบบขอเปลี่ยนเวลาทำงานเป็น WFH 3 วันต่อสัปดาห์", ApprovalStatus.Rejected, "ตำแหน่งงานต้องประจำที่ออฟฟิศตามนโยบายบริษัท"),
            Doc(7, "ใบขออนุมัติงบจัดกิจกรรมส่งท้ายปีเก่าแผนก IT", ApprovalStatus.Pending, null),
            Doc(8, "ใบสั่งซื้อ license ซอฟต์แวร์ Figma รายปี 5 ที่นั่ง", ApprovalStatus.Pending, null),
            Doc(9, "ใบขออนุมัติเบิกค่าโทรศัพท์ติดต่องานราชการต่างจังหวัด", ApprovalStatus.Pending, null),
            Doc(10, "แบบขอซ่อมแซมเครื่องปรับอากาศห้อง Server ชั้น 3", ApprovalStatus.Pending, null),
        };

        db.ApprovalDocuments.AddRange(documents);
        await db.SaveChangesAsync(ct);

        return;

        ApprovalDocument Doc(int id, string title, ApprovalStatus status, string? reason) => new()
        {
            Title = title,
            Status = status,
            Reason = reason ?? "-",
            CreatedAt = now.AddDays(-id),
            DecidedAt = status == ApprovalStatus.Pending ? null : now.AddDays(-id).AddHours(3)
        };
    }
}
