namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record NotifyDeviationRequestDto(
    Guid DeviationEventId,
    string? Note
);
