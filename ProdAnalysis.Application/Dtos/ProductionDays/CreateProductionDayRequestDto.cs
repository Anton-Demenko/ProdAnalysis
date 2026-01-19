namespace ProdAnalysis.Application.Dtos.ProductionDays;

public sealed record CreateProductionDayRequestDto(
    DateOnly Date,
    Guid WorkCenterId,
    Guid ProductId,
    int TaktSec
);