using ProdAnalysis.Application.Dtos.Deviations;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IDeviationEventService
{
    Task<IReadOnlyList<DeviationEventListItemDto>> ListAsync(DateOnly? date, Guid? workCenterId, bool includeClosed);
    Task<DeviationEventDetailsDto?> GetAsync(Guid deviationEventId);
    Task AckAsync(AckDeviationRequestDto request, Guid userId);
    Task CloseAsync(CloseDeviationRequestDto request, Guid userId);
    Task NotifyAsync(NotifyDeviationRequestDto request, Guid userId);
}
