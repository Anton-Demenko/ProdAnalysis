using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProdAnalysis.Application.Dtos.Deviations;
using ProdAnalysis.Application.Options;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class DeviationEventService : IDeviationEventService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly DeviationOptions _options;

    public DeviationEventService(IDbContextFactory<AppDbContext> dbFactory, IOptions<DeviationOptions> options)
    {
        _dbFactory = dbFactory;
        _options = options.Value ?? new DeviationOptions();
    }

    public async Task<IReadOnlyList<DeviationEventListItemDto>> ListAsync(DateOnly? date, Guid? workCenterId, bool includeClosed)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        await EvaluateEscalationsAsync(db, _options.EscalationMinutes);

        var q = db.DeviationEvents
            .AsNoTracking()
            .Include(x => x.ProductionDay)
                .ThenInclude(x => x.WorkCenter)
            .Include(x => x.ProductionDay)
                .ThenInclude(x => x.Product)
            .AsQueryable();

        if (!includeClosed)
            q = q.Where(x => x.Status != DeviationEventStatus.Closed);

        if (date.HasValue)
            q = q.Where(x => x.ProductionDate == date.Value);

        if (workCenterId.HasValue)
            q = q.Where(x => x.WorkCenterId == workCenterId.Value);

        var now = DateTime.UtcNow;

        var list = await q
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return list.Select(x => new DeviationEventListItemDto(
            x.Id,
            x.ProductionDate,
            x.ProductionDay.WorkCenter.Name,
            x.ProductionDay.Product.Name,
            x.HourIndex,
            x.HourStart,
            x.PlanQty,
            x.ActualQty,
            x.DeviationQty,
            x.Status,
            x.CurrentEscalationLevel,
            x.CreatedAt,
            (int)Math.Max(0, Math.Floor((now - x.CreatedAt).TotalMinutes))
        )).ToList();
    }

    public async Task<DeviationEventDetailsDto?> GetAsync(Guid deviationEventId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        await EvaluateEscalationsAsync(db, _options.EscalationMinutes);

        var ev = await db.DeviationEvents
            .AsNoTracking()
            .Include(x => x.ProductionDay)
                .ThenInclude(x => x.WorkCenter)
            .Include(x => x.ProductionDay)
                .ThenInclude(x => x.Product)
            .Include(x => x.AcknowledgedByUser)
            .Include(x => x.ClosedByUser)
            .Include(x => x.EscalationLogs)
            .FirstOrDefaultAsync(x => x.Id == deviationEventId);

        if (ev == null)
            return null;

        var esc = ev.EscalationLogs
            .OrderBy(x => x.CreatedAt)
            .Select(x => new EscalationLogDto(x.Level, x.CreatedAt, x.Message))
            .ToList();

        return new DeviationEventDetailsDto(
            ev.Id,
            ev.ProductionDate,
            ev.WorkCenterId,
            ev.ProductionDay.WorkCenter.Name,
            ev.ProductId,
            ev.ProductionDay.Product.Name,
            ev.HourIndex,
            ev.HourStart,
            ev.PlanQty,
            ev.ActualQty,
            ev.DeviationQty,
            ev.Status,
            ev.CurrentEscalationLevel,
            ev.CreatedAt,
            ev.Note,
            ev.AcknowledgedAt,
            ev.AcknowledgedByUser?.DisplayName,
            ev.ClosedAt,
            ev.ClosedByUser?.DisplayName,
            esc
        );
    }

    public async Task AckAsync(AckDeviationRequestDto request, Guid userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ev = await db.DeviationEvents.FirstOrDefaultAsync(x => x.Id == request.DeviationEventId);
        if (ev == null)
            throw new InvalidOperationException("DeviationEvent not found.");

        if (ev.AcknowledgedAt.HasValue)
            return;

        var now = DateTime.UtcNow;

        ev.AcknowledgedAt = now;
        ev.AcknowledgedByUserId = userId;

        if (ev.Status == DeviationEventStatus.Open)
            ev.Status = DeviationEventStatus.Acknowledged;

        db.EscalationLogs.Add(new EscalationLog
        {
            Id = Guid.NewGuid(),
            DeviationEventId = ev.Id,
            Level = Math.Max(1, ev.CurrentEscalationLevel),
            CreatedAt = now,
            Message = "Подтверждение получено (ACK)."
        });

        await db.SaveChangesAsync();
    }

    public async Task CloseAsync(CloseDeviationRequestDto request, Guid userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ev = await db.DeviationEvents.FirstOrDefaultAsync(x => x.Id == request.DeviationEventId);
        if (ev == null)
            throw new InvalidOperationException("DeviationEvent not found.");

        if (ev.Status == DeviationEventStatus.Closed)
            return;

        var now = DateTime.UtcNow;

        ev.Status = DeviationEventStatus.Closed;
        ev.ClosedAt = now;
        ev.ClosedByUserId = userId;

        if (!string.IsNullOrWhiteSpace(request.Note))
            ev.Note = request.Note.Trim();

        db.EscalationLogs.Add(new EscalationLog
        {
            Id = Guid.NewGuid(),
            DeviationEventId = ev.Id,
            Level = Math.Max(1, ev.CurrentEscalationLevel),
            CreatedAt = now,
            Message = "Закрыто пользователем."
        });

        await db.SaveChangesAsync();
    }

    internal static async Task EvaluateEscalationsAsync(AppDbContext db, int escalationMinutes)
    {
        if (escalationMinutes <= 0)
            escalationMinutes = 30;

        var now = DateTime.UtcNow;
        var threshold = now.AddMinutes(-escalationMinutes);

        var candidates = await db.DeviationEvents
            .Include(x => x.EscalationLogs)
            .Where(x => x.Status == DeviationEventStatus.Open)
            .Where(x => x.AcknowledgedAt == null)
            .Where(x => x.CreatedAt <= threshold)
            .ToListAsync();

        var changed = false;

        foreach (var ev in candidates)
        {
            if (ev.CurrentEscalationLevel >= 2)
                continue;

            var already = ev.EscalationLogs.Any(x => x.Level == 2);
            ev.CurrentEscalationLevel = 2;

            if (!already)
            {
                db.EscalationLogs.Add(new EscalationLog
                {
                    Id = Guid.NewGuid(),
                    DeviationEventId = ev.Id,
                    Level = 2,
                    CreatedAt = now,
                    Message = "Эскалация: уровень 2 (руководитель). Нет ACK в установленный срок."
                });
            }

            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync();
    }
}
