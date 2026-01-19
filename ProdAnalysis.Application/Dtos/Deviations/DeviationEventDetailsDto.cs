using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.Deviations;

public sealed record DeviationEventDetailsDto(
    Guid Id,
    DateOnly ProductionDate,
    Guid WorkCenterId,
    string WorkCenterName,
    Guid ProductId,
    string ProductName,
    int HourIndex,
    TimeOnly HourStart,
    int PlanQty,
    int ActualQty,
    int DeviationQty,
    DeviationEventStatus Status,
    int CurrentEscalationLevel,
    DateTime CreatedAt,
    string? Note,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTime? ClosedAt,
    string? ClosedBy,
    IReadOnlyList<EscalationLogDto> Escalations
);