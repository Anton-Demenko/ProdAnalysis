namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record EscalationLogDto(
    int Level,
    DateTime CreatedAt,
    string Message
);