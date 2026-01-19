using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Hourly;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class HourlyRecordService : IHourlyRecordService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public HourlyRecordService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> UpdateActualAsync(UpdateHourlyActualRequestDto request, Guid userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var hr = await db.HourlyRecords
            .Include(x => x.ProductionDay)
            .FirstOrDefaultAsync(x => x.Id == request.HourlyRecordId);

        if (hr == null)
            throw new InvalidOperationException("HourlyRecord not found.");

        if (request.ActualQty.HasValue && request.ActualQty.Value < 0)
            throw new InvalidOperationException("Actual cannot be negative.");

        var newActual = request.ActualQty ?? 0;
        var newComment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();

        var changed = (hr.ActualQty ?? 0) != newActual || hr.Comment != newComment;

        hr.ActualQty = newActual;
        hr.Comment = newComment;
        hr.UpdatedAt = DateTime.UtcNow;
        hr.UpdatedByUserId = userId;

        await UpsertDeviationEventAsync(db, hr, userId);

        await db.SaveChangesAsync();
        return changed;
    }

    private static async Task UpsertDeviationEventAsync(AppDbContext db, HourlyRecord hr, Guid userId)
    {
        var actual = hr.ActualQty ?? 0;
        var plan = hr.PlanQty;

        var open = await db.DeviationEvents
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.HourlyRecordId == hr.Id && x.Status != DeviationEventStatus.Closed);

        if (actual < plan)
        {
            if (open == null)
            {
                var ev = new DeviationEvent
                {
                    Id = Guid.NewGuid(),
                    ProductionDayId = hr.ProductionDayId,
                    HourlyRecordId = hr.Id,
                    WorkCenterId = hr.ProductionDay.WorkCenterId,
                    ProductId = hr.ProductionDay.ProductId,
                    ProductionDate = hr.ProductionDay.Date,
                    HourIndex = hr.HourIndex,
                    HourStart = hr.HourStart,
                    PlanQty = plan,
                    ActualQty = actual,
                    DeviationQty = actual - plan,
                    Status = DeviationEventStatus.Open,
                    CurrentEscalationLevel = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId,
                    Note = null
                };

                db.DeviationEvents.Add(ev);

                db.EscalationLogs.Add(new EscalationLog
                {
                    Id = Guid.NewGuid(),
                    DeviationEventId = ev.Id,
                    Level = 1,
                    CreatedAt = DateTime.UtcNow,
                    Message = "Deviation detected (L1: Master)."
                });

                return;
            }

            open.PlanQty = plan;
            open.ActualQty = actual;
            open.DeviationQty = actual - plan;

            if (open.Status == DeviationEventStatus.Closed)
                open.Status = DeviationEventStatus.Open;

            return;
        }

        if (open != null)
        {
            open.Status = DeviationEventStatus.Closed;
            open.ClosedAt = DateTime.UtcNow;
            open.ClosedByUserId = userId;

            db.EscalationLogs.Add(new EscalationLog
            {
                Id = Guid.NewGuid(),
                DeviationEventId = open.Id,
                Level = Math.Max(1, open.CurrentEscalationLevel),
                CreatedAt = DateTime.UtcNow,
                Message = "Auto-closed: plan met."
            });
        }
    }
}