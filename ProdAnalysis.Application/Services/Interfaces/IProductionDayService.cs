using ProdAnalysis.Application.Dtos.ProductionDays;
using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IProductionDayService
{
    Task<Guid> CreateAsync(CreateProductionDayRequestDto request, Guid createdByUserId);
    Task<IReadOnlyList<ProductionDayListItemDto>> ListAsync(DateOnly? date, Guid? workCenterId, ProductionDayStatus? status);
    Task<ProductionDayDetailsDto?> GetDetailsAsync(Guid productionDayId);
    Task SetStatusAsync(Guid productionDayId, ProductionDayStatus status);
}
