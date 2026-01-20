using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services.Deviations;

public static class DeviationEventUpserter
{
    public static async Task UpsertAsync(AppDbContext db, HourlyRecord hr, Guid userId)
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
                var now = DateTime.UtcNow;

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
                    CreatedAt = now,
                    CreatedByUserId = userId,
                    Note = null
                };

                db.DeviationEvents.Add(ev);

                db.EscalationLogs.Add(new EscalationLog
                {
                    Id = Guid.NewGuid(),
                    DeviationEventId = ev.Id,
                    Level = 1,
                    CreatedAt = now,
                    Message = "Отклонение зафиксировано. Уровень 1 (мастер)."
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
            var now = DateTime.UtcNow;

            open.Status = DeviationEventStatus.Closed;
            open.ClosedAt = now;
            open.ClosedByUserId = userId;

            db.EscalationLogs.Add(new EscalationLog
            {
                Id = Guid.NewGuid(),
                DeviationEventId = open.Id,
                Level = Math.Max(1, open.CurrentEscalationLevel),
                CreatedAt = now,
                Message = "Отклонение закрыто автоматически: план достигнут."
            });
        }
    }
}
