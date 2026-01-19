using ProdAnalysis.Application.Dtos.Hourly;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IHourlyRecordService
{
    Task<bool> UpdateActualAsync(UpdateHourlyActualRequestDto request, Guid userId);
}