namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record CloseDeviationRequestDto(
    Guid DeviationEventId,
    string? Note
);