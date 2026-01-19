using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record DeviationEventListItemDto(
    Guid Id,
    DateOnly ProductionDate,
    string WorkCenterName,
    string ProductName,
    int HourIndex,
    TimeOnly HourStart,
    int PlanQty,
    int ActualQty,
    int DeviationQty,
    DeviationEventStatus Status,
    int CurrentEscalationLevel,
    DateTime CreatedAt,
    int AgeMinutes
);