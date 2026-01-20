using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Infrastructure.Persistence;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db, bool reset)
    {
        if (reset)
            await ResetOperationalDataAsync(db);

        if (await db.ProductionDays.AnyAsync())
            return;

        var wc = await db.WorkCenters.OrderBy(x => x.Name).FirstAsync();
        var products = await db.Products.OrderBy(x => x.Code).ToListAsync();
        var productA = products.First();
        var productB = products.Skip(1).FirstOrDefault() ?? productA;

        var users = await db.Users.ToListAsync();
        var operatorUser = users.First(x => x.Role == UserRole.Operator);
        var masterUser = users.First(x => x.Role == UserRole.Master);

        var reasons = await db.DowntimeReasons.ToListAsync();
        var rBrk = reasons.First(x => x.Code == "BRKDWN");
        var rTool = reasons.First(x => x.Code == "TOOL");
        var rSetup = reasons.First(x => x.Code == "SETUP");
        var rMaint = reasons.First(x => x.Code == "MAINT");
        var rMat = reasons.First(x => x.Code == "MAT");
        var rQc = reasons.First(x => x.Code == "QC");
        var rQual = reasons.First(x => x.Code == "QUAL");

        var now = DateTime.UtcNow;

        var shiftStart = new TimeOnly(8, 0);
        var shiftEnd = new TimeOnly(16, 0);

        var today = DateOnly.FromDateTime(now);

        var dayToday = BuildDay(
            date: today,
            workCenter: wc,
            product: productA,
            taktSec: 300,
            planPerHour: 12,
            status: ProductionDayStatus.Active,
            createdAt: now.AddMinutes(-40),
            createdBy: operatorUser
        );

        var dayYesterday = BuildDay(
            date: today.AddDays(-1),
            workCenter: wc,
            product: productB,
            taktSec: 600,
            planPerHour: 6,
            status: ProductionDayStatus.Closed,
            createdAt: now.AddDays(-1).AddMinutes(-40),
            createdBy: operatorUser
        );

        var dayTwoDaysAgo = BuildDay(
            date: today.AddDays(-2),
            workCenter: wc,
            product: productA,
            taktSec: 300,
            planPerHour: 12,
            status: ProductionDayStatus.Active,
            createdAt: now.AddDays(-2).AddMinutes(-40),
            createdBy: operatorUser
        );

        FillHours(dayToday, shiftStart, 12, new[] { 12, 12, 6, 12, 12, 12, 12, 12 }, operatorUser, now);
        FillHours(dayYesterday, shiftStart, 6, new[] { 6, 6, 6, 6, 6, 6, 6, 6 }, operatorUser, now.AddDays(-1));
        FillHours(dayTwoDaysAgo, shiftStart, 12, new[] { 12, 12, 12, 12, 8, 12, 12, 12 }, operatorUser, now.AddDays(-2));

        AddDowntime(dayToday, 2, rBrk, 20, "Machine stop", operatorUser, now);
        AddDowntime(dayToday, 2, rTool, 10, "Tool replacement", operatorUser, now);
        AddDowntime(dayToday, 5, rSetup, 15, "Setup adjustment", operatorUser, now);

        AddDowntime(dayYesterday, 1, rMaint, 20, "Planned maintenance", operatorUser, now.AddDays(-1));
        AddDowntime(dayYesterday, 6, rMat, 10, "Material waiting", operatorUser, now.AddDays(-1));
        AddDowntime(dayYesterday, 6, rQc, 5, "QC waiting", operatorUser, now.AddDays(-1));

        AddDowntime(dayTwoDaysAgo, 4, rQual, 12, "Rework / quality", operatorUser, now.AddDays(-2));

        db.ProductionDays.AddRange(dayToday, dayYesterday, dayTwoDaysAgo);

        var evOpen = BuildDeviation(
            productionDay: dayToday,
            hourlyRecord: dayToday.HourlyRecords.First(x => x.HourIndex == 2),
            status: DeviationEventStatus.Open,
            createdAt: now.AddMinutes(-10),
            createdBy: operatorUser,
            acknowledgedBy: null,
            acknowledgedAt: null,
            currentLevel: 1,
            note: null
        );

        var evAck = BuildDeviation(
            productionDay: dayTwoDaysAgo,
            hourlyRecord: dayTwoDaysAgo.HourlyRecords.First(x => x.HourIndex == 4),
            status: DeviationEventStatus.Acknowledged,
            createdAt: now.AddDays(-2).AddMinutes(-15),
            createdBy: operatorUser,
            acknowledgedBy: masterUser,
            acknowledgedAt: now.AddDays(-2).AddMinutes(-10),
            currentLevel: 1,
            note: "Investigating cause and corrective action."
        );

        db.DeviationEvents.AddRange(evOpen, evAck);

        db.EscalationLogs.Add(new EscalationLog
        {
            Id = Guid.NewGuid(),
            DeviationEventId = evOpen.Id,
            Level = 1,
            CreatedAt = evOpen.CreatedAt,
            Message = "Deviation detected (L1: Master)."
        });

        db.EscalationLogs.Add(new EscalationLog
        {
            Id = Guid.NewGuid(),
            DeviationEventId = evAck.Id,
            Level = 1,
            CreatedAt = evAck.CreatedAt,
            Message = "Deviation detected (L1: Master)."
        });

        db.EscalationLogs.Add(new EscalationLog
        {
            Id = Guid.NewGuid(),
            DeviationEventId = evAck.Id,
            Level = 1,
            CreatedAt = evAck.AcknowledgedAt ?? now,
            Message = "ACK received."
        });

        await db.SaveChangesAsync();
    }

    private static async Task ResetOperationalDataAsync(AppDbContext db)
    {
        await db.EscalationLogs.ExecuteDeleteAsync();
        await db.DeviationEvents.ExecuteDeleteAsync();
        await db.HourlyDowntimes.ExecuteDeleteAsync();
        await db.HourlyRecords.ExecuteDeleteAsync();
        await db.ProductionDays.ExecuteDeleteAsync();
    }

    private static ProductionDay BuildDay(
        DateOnly date,
        WorkCenter workCenter,
        Product product,
        int taktSec,
        int planPerHour,
        ProductionDayStatus status,
        DateTime createdAt,
        AppUser createdBy
    )
    {
        return new ProductionDay
        {
            Id = Guid.NewGuid(),
            Date = date,
            ShiftStart = new TimeOnly(8, 0),
            ShiftEnd = new TimeOnly(16, 0),
            WorkCenterId = workCenter.Id,
            WorkCenter = workCenter,
            ProductId = product.Id,
            Product = product,
            TaktSec = taktSec,
            PlanPerHour = planPerHour,
            Status = status,
            CreatedAt = createdAt,
            CreatedByUserId = createdBy.Id,
            CreatedByUser = createdBy
        };
    }

    private static void FillHours(ProductionDay day, TimeOnly shiftStart, int planQty, int[] actualByHour, AppUser updatedBy, DateTime updatedAt)
    {
        for (var i = 0; i < 8; i++)
        {
            var actual = actualByHour.Length > i ? actualByHour[i] : planQty;

            day.HourlyRecords.Add(new HourlyRecord
            {
                Id = Guid.NewGuid(),
                ProductionDayId = day.Id,
                ProductionDay = day,
                HourIndex = i,
                HourStart = shiftStart.AddHours(i),
                PlanQty = planQty,
                ActualQty = actual,
                Comment = actual < planQty ? "Delay / under plan" : null,
                UpdatedAt = updatedAt,
                UpdatedByUserId = updatedBy.Id,
                UpdatedByUser = updatedBy
            });
        }
    }

    private static void AddDowntime(ProductionDay day, int hourIndex, DowntimeReason reason, int minutes, string? comment, AppUser updatedBy, DateTime updatedAt)
    {
        var hr = day.HourlyRecords.First(x => x.HourIndex == hourIndex);

        hr.HourlyDowntimes.Add(new HourlyDowntime
        {
            Id = Guid.NewGuid(),
            HourlyRecordId = hr.Id,
            HourlyRecord = hr,
            DowntimeReasonId = reason.Id,
            DowntimeReason = reason,
            Minutes = minutes,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedBy.Id,
            UpdatedByUser = updatedBy
        });
    }

    private static DeviationEvent BuildDeviation(
        ProductionDay productionDay,
        HourlyRecord hourlyRecord,
        DeviationEventStatus status,
        DateTime createdAt,
        AppUser createdBy,
        AppUser? acknowledgedBy,
        DateTime? acknowledgedAt,
        int currentLevel,
        string? note
    )
    {
        var actual = hourlyRecord.ActualQty ?? 0;
        var plan = hourlyRecord.PlanQty;

        return new DeviationEvent
        {
            Id = Guid.NewGuid(),
            ProductionDayId = productionDay.Id,
            ProductionDay = productionDay,
            HourlyRecordId = hourlyRecord.Id,
            HourlyRecord = hourlyRecord,
            WorkCenterId = productionDay.WorkCenterId,
            ProductId = productionDay.ProductId,
            ProductionDate = productionDay.Date,
            HourIndex = hourlyRecord.HourIndex,
            HourStart = hourlyRecord.HourStart,
            PlanQty = plan,
            ActualQty = actual,
            DeviationQty = actual - plan,
            Status = status,
            CurrentEscalationLevel = currentLevel,
            CreatedAt = createdAt,
            CreatedByUserId = createdBy.Id,
            CreatedByUser = createdBy,
            AcknowledgedAt = acknowledgedAt,
            AcknowledgedByUserId = acknowledgedBy?.Id,
            AcknowledgedByUser = acknowledgedBy,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };
    }
}
