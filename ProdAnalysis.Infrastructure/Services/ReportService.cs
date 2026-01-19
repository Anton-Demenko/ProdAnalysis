using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Reports;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ReportService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ProductionSummaryItemDto>> GetProductionSummaryAsync(DateOnly from, DateOnly to, Guid? workCenterId)
    {
        if (to < from)
            (from, to) = (to, from);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var query = db.ProductionDays
            .AsNoTracking()
            .Include(x => x.WorkCenter)
            .Include(x => x.Product)
            .Include(x => x.HourlyRecords)
            .Where(x => x.Date >= from && x.Date <= to)
            .AsQueryable();

        if (workCenterId.HasValue)
            query = query.Where(x => x.WorkCenterId == workCenterId.Value);

        var list = await query
            .OrderBy(x => x.Date)
            .ThenBy(x => x.WorkCenter.Name)
            .ThenBy(x => x.Product.Name)
            .ToListAsync();

        var result = new List<ProductionSummaryItemDto>(list.Count);

        foreach (var pd in list)
        {
            var planShift = pd.HourlyRecords.Sum(x => x.PlanQty);
            var actualShift = pd.HourlyRecords.Sum(x => x.ActualQty ?? 0);
            var deviation = actualShift - planShift;

            result.Add(new ProductionSummaryItemDto(
                pd.Id,
                pd.Date,
                pd.WorkCenter.Name,
                pd.Product.Name,
                pd.TaktSec,
                pd.PlanPerHour,
                planShift,
                actualShift,
                deviation,
                pd.Status
            ));
        }

        return result;
    }
}
