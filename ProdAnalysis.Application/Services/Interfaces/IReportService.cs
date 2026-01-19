using ProdAnalysis.Application.Dtos.Reports;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<ProductionSummaryItemDto>> GetProductionSummaryAsync(DateOnly from, DateOnly to, Guid? workCenterId);
}
