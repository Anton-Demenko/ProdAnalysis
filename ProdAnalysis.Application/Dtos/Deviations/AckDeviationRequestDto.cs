namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record AckDeviationRequestDto(
    Guid DeviationEventId
);