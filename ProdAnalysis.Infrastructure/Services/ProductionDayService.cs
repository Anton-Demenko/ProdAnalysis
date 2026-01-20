using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Hourly;
using ProdAnalysis.Application.Dtos.ProductionDays;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;
using ProdAnalysis.Infrastructure.Services.Planning;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class ProductionDayService : IProductionDayService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProductionDayService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CreateAsync(CreateProductionDayRequestDto request, Guid createdByUserId)
    {
        if (request.TaktSec < 1 || request.TaktSec > 86400)
            throw new InvalidOperationException("TaktSec must be in range 1..86400.");

        var shiftStart = new TimeOnly(8, 0);
        var shiftEnd = new TimeOnly(16, 0);
        var now = DateTime.UtcNow;

        const int hours = 8;

        var hourlyPlan = BuildHourlyPlan(request, hours);
        var planShift = hourlyPlan.Sum();
        var planPerHour = (int)Math.Round(hours == 0 ? 0d : (double)planShift / hours);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var exists = await db.ProductionDays
            .AsNoTracking()
            .AnyAsync(x =>
                x.Date == request.Date &&
                x.ShiftStart == shiftStart &&
                x.WorkCenterId == request.WorkCenterId &&
                x.ProductId == request.ProductId);

        if (exists)
            throw new InvalidOperationException("ProductionDay already exists for the given date/workcenter/shift/product.");

        var pd = new ProductionDay
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            ShiftStart = shiftStart,
            ShiftEnd = shiftEnd,
            WorkCenterId = request.WorkCenterId,
            ProductId = request.ProductId,
            TaktSec = request.TaktSec,
            PlanPerHour = planPerHour,
            Status = ProductionDayStatus.Active,
            CreatedAt = now,
            CreatedByUserId = createdByUserId
        };

        for (var i = 0; i < hours; i++)
        {
            var hr = new HourlyRecord
            {
                Id = Guid.NewGuid(),
                ProductionDayId = pd.Id,
                HourIndex = i,
                HourStart = shiftStart.AddHours(i),
                PlanQty = hourlyPlan[i],
                ActualQty = 0,
                Comment = null,
                UpdatedAt = now,
                UpdatedByUserId = createdByUserId
            };
            pd.HourlyRecords.Add(hr);
        }

        db.ProductionDays.Add(pd);
        await db.SaveChangesAsync();
        return pd.Id;
    }

    public async Task<IReadOnlyList<ProductionDayListItemDto>> ListAsync(DateOnly? date, Guid? workCenterId, ProductionDayStatus? status)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var query = db.ProductionDays
            .AsNoTracking()
            .Include(x => x.WorkCenter)
            .Include(x => x.Product)
            .Include(x => x.HourlyRecords)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(x => x.Date == date.Value);

        if (workCenterId.HasValue)
            query = query.Where(x => x.WorkCenterId == workCenterId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var list = await query
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.WorkCenter.Name)
            .ToListAsync();

        var result = new List<ProductionDayListItemDto>(list.Count);

        foreach (var pd in list)
        {
            var ordered = pd.HourlyRecords.OrderBy(x => x.HourIndex).ToList();

            var planShift = 0;
            var actualShift = 0;

            foreach (var hr in ordered)
            {
                planShift += hr.PlanQty;
                actualShift += hr.ActualQty ?? 0;
            }

            var cumDeviationNow = actualShift - planShift;
            var avgPlanPerHour = ordered.Count <= 0 ? 0d : (double)planShift / ordered.Count;

            result.Add(new ProductionDayListItemDto(
                pd.Id,
                pd.Date,
                pd.WorkCenter.Name,
                pd.Product.Name,
                pd.TaktSec,
                pd.PlanPerHour,
                pd.Status,
                cumDeviationNow,
                planShift,
                avgPlanPerHour
            ));
        }

        return result;
    }

    public async Task<ProductionDayDetailsDto?> GetDetailsAsync(Guid productionDayId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var pd = await db.ProductionDays
            .AsNoTracking()
            .Include(x => x.WorkCenter)
            .Include(x => x.Product)
            .Include(x => x.HourlyRecords)
                .ThenInclude(x => x.HourlyDowntimes)
            .FirstOrDefaultAsync(x => x.Id == productionDayId);

        if (pd == null)
            return null;

        var hours = pd.HourlyRecords.OrderBy(x => x.HourIndex).ToList();

        var dtos = new List<HourlyRecordDto>(hours.Count);
        var cumPlan = 0;
        var cumActual = 0;

        foreach (var hr in hours)
        {
            var actualVal = hr.ActualQty ?? 0;
            var dtMin = hr.HourlyDowntimes.Sum(x => x.Minutes);

            cumPlan += hr.PlanQty;
            cumActual += actualVal;

            var hasDev = actualVal < hr.PlanQty;

            dtos.Add(new HourlyRecordDto(
                hr.Id,
                hr.HourIndex,
                hr.HourStart,
                hr.PlanQty,
                actualVal,
                hr.Comment,
                dtMin,
                cumPlan,
                cumActual,
                cumActual - cumPlan,
                hasDev
            ));
        }

        var planShift = hours.Sum(x => x.PlanQty);
        var actualShift = hours.Sum(x => x.ActualQty ?? 0);
        var totals = new ProductionTotalsDto(planShift, actualShift, actualShift - planShift);

        return new ProductionDayDetailsDto(
            pd.Id,
            pd.Date,
            pd.ShiftStart,
            pd.ShiftEnd,
            pd.WorkCenterId,
            pd.WorkCenter.Name,
            pd.ProductId,
            pd.Product.Name,
            pd.TaktSec,
            pd.PlanPerHour,
            pd.Status,
            dtos,
            totals
        );
    }

    public async Task SetStatusAsync(Guid productionDayId, ProductionDayStatus status)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var pd = await db.ProductionDays
            .FirstOrDefaultAsync(x => x.Id == productionDayId);

        if (pd == null)
            throw new InvalidOperationException("ProductionDay not found.");

        if (pd.Status == status)
            return;

        pd.Status = status;
        await db.SaveChangesAsync();
    }

    private static int[] BuildHourlyPlan(CreateProductionDayRequestDto request, int hours)
    {
        if (hours < 1)
            throw new InvalidOperationException("Hours must be >= 1.");

        if (request.HourlyPlan != null)
        {
            if (request.HourlyPlan.Length != hours)
                throw new InvalidOperationException($"HourlyPlan must have exactly {hours} values.");

            var res = new int[hours];
            for (var i = 0; i < hours; i++)
            {
                var v = request.HourlyPlan[i];
                if (v < 0)
                    throw new InvalidOperationException("HourlyPlan values cannot be negative.");
                res[i] = v;
            }

            return res;
        }

        return ShiftPlanCalculator.CalculateCumulativeHourlyPlan(request.TaktSec, hours);
    }
}
