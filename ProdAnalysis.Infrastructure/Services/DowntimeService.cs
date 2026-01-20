using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Downtime;
using ProdAnalysis.Application.Dtos.Reports;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class DowntimeService : IDowntimeService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DowntimeService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<DowntimeReasonDto>> GetReasonsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.DowntimeReasons
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DowntimeReasonDto(x.Id, x.Code, x.Name))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HourlyDowntimeDto>> GetHourlyDowntimesAsync(Guid hourlyRecordId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.HourlyDowntimes
            .AsNoTracking()
            .Include(x => x.DowntimeReason)
            .Where(x => x.HourlyRecordId == hourlyRecordId)
            .OrderByDescending(x => x.Minutes)
            .ThenBy(x => x.DowntimeReason.Name)
            .Select(x => new HourlyDowntimeDto(
                x.Id,
                x.DowntimeReasonId,
                x.DowntimeReason.Code,
                x.DowntimeReason.Name,
                x.Minutes,
                x.Comment
            ))
            .ToListAsync();
    }

    public async Task UpsertHourlyDowntimeAsync(UpsertHourlyDowntimeRequestDto request, Guid userId)
    {
        if (request.Minutes < 0 || request.Minutes > 60)
            throw new InvalidOperationException("Minutes must be in range 0..60.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var hr = await db.HourlyRecords
            .Include(x => x.ProductionDay)
            .FirstOrDefaultAsync(x => x.Id == request.HourlyRecordId);

        if (hr == null)
            throw new InvalidOperationException("HourlyRecord not found.");

        if (hr.ProductionDay.Status == ProductionDayStatus.Closed)
            throw new InvalidOperationException("ProductionDay is closed. Editing is not allowed.");

        var reasonExists = await db.DowntimeReasons
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.DowntimeReasonId && x.IsActive);

        if (!reasonExists)
            throw new InvalidOperationException("DowntimeReason not found or inactive.");

        var now = DateTime.UtcNow;

        var entity = await db.HourlyDowntimes
            .FirstOrDefaultAsync(x => x.HourlyRecordId == request.HourlyRecordId && x.DowntimeReasonId == request.DowntimeReasonId);

        if (request.Minutes == 0 && entity != null)
        {
            db.HourlyDowntimes.Remove(entity);
            await db.SaveChangesAsync();
            return;
        }

        if (request.Minutes == 0)
            return;

        if (entity == null)
        {
            entity = new Domain.Entities.HourlyDowntime
            {
                Id = Guid.NewGuid(),
                HourlyRecordId = request.HourlyRecordId,
                DowntimeReasonId = request.DowntimeReasonId,
                Minutes = request.Minutes,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
                UpdatedAt = now,
                UpdatedByUserId = userId
            };
            db.HourlyDowntimes.Add(entity);
        }
        else
        {
            entity.Minutes = request.Minutes;
            entity.Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
            entity.UpdatedAt = now;
            entity.UpdatedByUserId = userId;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteHourlyDowntimeAsync(DeleteHourlyDowntimeRequestDto request)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var hr = await db.HourlyRecords
            .Include(x => x.ProductionDay)
            .FirstOrDefaultAsync(x => x.Id == request.HourlyRecordId);

        if (hr == null)
            return;

        if (hr.ProductionDay.Status == ProductionDayStatus.Closed)
            throw new InvalidOperationException("ProductionDay is closed. Editing is not allowed.");

        var entity = await db.HourlyDowntimes
            .FirstOrDefaultAsync(x => x.HourlyRecordId == request.HourlyRecordId && x.DowntimeReasonId == request.DowntimeReasonId);

        if (entity == null)
            return;

        db.HourlyDowntimes.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ParetoItemDto>> GetParetoAsync(DateOnly dateFrom, DateOnly dateTo, Guid? workCenterId)
    {
        if (dateTo < dateFrom)
            throw new InvalidOperationException("dateTo must be >= dateFrom.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var q = db.HourlyDowntimes
            .AsNoTracking()
            .Include(x => x.DowntimeReason)
            .Include(x => x.HourlyRecord)
            .ThenInclude(x => x.ProductionDay)
            .Where(x => x.HourlyRecord.ProductionDay.Date >= dateFrom && x.HourlyRecord.ProductionDay.Date <= dateTo);

        if (workCenterId.HasValue)
            q = q.Where(x => x.HourlyRecord.ProductionDay.WorkCenterId == workCenterId.Value);

        var grouped = await q
            .GroupBy(x => new { x.DowntimeReasonId, x.DowntimeReason.Code, x.DowntimeReason.Name })
            .Select(g => new
            {
                g.Key.DowntimeReasonId,
                g.Key.Code,
                g.Key.Name,
                TotalMinutes = g.Sum(x => x.Minutes)
            })
            .ToListAsync();

        var ordered = grouped
            .OrderByDescending(x => x.TotalMinutes)
            .ThenBy(x => x.Name)
            .ToList();

        var total = ordered.Sum(x => x.TotalMinutes);
        if (total <= 0)
            return Array.Empty<ParetoItemDto>();

        var result = new List<ParetoItemDto>(ordered.Count);
        double cum = 0;

        foreach (var it in ordered)
        {
            var pct = (double)it.TotalMinutes / total * 100.0;
            cum += pct;

            result.Add(new ParetoItemDto(
                it.DowntimeReasonId,
                it.Code,
                it.Name,
                it.TotalMinutes,
                Math.Round(pct, 2),
                Math.Round(cum, 2)
            ));
        }

        return result;
    }
}
