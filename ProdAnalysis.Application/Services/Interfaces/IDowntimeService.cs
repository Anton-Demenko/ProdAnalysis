using ProdAnalysis.Application.Dtos.Downtime;
using ProdAnalysis.Application.Dtos.Reports;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IDowntimeService
{
    Task<IReadOnlyList<DowntimeReasonDto>> GetReasonsAsync();
    Task<IReadOnlyList<HourlyDowntimeDto>> GetHourlyDowntimesAsync(Guid hourlyRecordId);
    Task UpsertHourlyDowntimeAsync(UpsertHourlyDowntimeRequestDto request, Guid userId);
    Task DeleteHourlyDowntimeAsync(DeleteHourlyDowntimeRequestDto request);
    Task<IReadOnlyList<ParetoItemDto>> GetParetoAsync(DateOnly dateFrom, DateOnly dateTo, Guid? workCenterId);
}