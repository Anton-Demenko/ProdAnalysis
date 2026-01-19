using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Hourly;
using ProdAnalysis.Application.Dtos.ProductionDays;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

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
        if (request.TaktSec < 1 || request.TaktSec > 3600)
            throw new InvalidOperationException("TaktSec must be in range 1..3600.");

        var planPerHour = 3600 / request.TaktSec;
        if (planPerHour < 1)
            throw new InvalidOperationException("PlanPerHour must be >= 1.");

        var shiftStart = new TimeOnly(8, 0);
        var shiftEnd = new TimeOnly(16, 0);
        var now = DateTime.UtcNow;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var exists = await db.ProductionDays
            .AsNoTracking()
            .AnyAsync(x => x.Date == request.Date && x.ShiftStart == shiftStart && x.WorkCenterId == request.WorkCenterId);

        if (exists)
            throw new InvalidOperationException("ProductionDay already exists for the given date/workcenter/shift start.");

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

        for (var i = 0; i < 8; i++)
        {
            var hr = new HourlyRecord
            {
                Id = Guid.NewGuid(),
                ProductionDayId = pd.Id,
                HourIndex = i,
                HourStart = shiftStart.AddHours(i),
                PlanQty = planPerHour,
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
            var cumPlan = 0;
            var cumActual = 0;

            foreach (var hr in ordered)
            {
                cumPlan += hr.PlanQty;
                cumActual += hr.ActualQty ?? 0;
            }

            var cumDev = cumActual - cumPlan;

            result.Add(new ProductionDayListItemDto(
                pd.Id,
                pd.Date,
                pd.WorkCenter.Name,
                pd.Product.Name,
                pd.TaktSec,
                pd.PlanPerHour,
                pd.Status,
                cumDev
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
}