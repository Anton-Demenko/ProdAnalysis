using ProdAnalysis.Application.Dtos.Integrations;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface ICsvIntegrationService
{
    Task<byte[]> ExportProductionDayHourlyCsvAsync(Guid productionDayId);
    Task<CsvImportResultDto> ImportProductionDayHourlyCsvAsync(Guid productionDayId, Stream csvStream, Guid userId);
    Task<byte[]> ExportParetoCsvAsync(DateOnly from, DateOnly to, Guid? workCenterId);
    Task<byte[]> ExportProductionSummaryCsvAsync(DateOnly from, DateOnly to, Guid? workCenterId);
}
