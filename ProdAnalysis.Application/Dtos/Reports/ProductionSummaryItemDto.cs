using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.Reports;

public sealed record ProductionSummaryItemDto(
    Guid ProductionDayId,
    DateOnly Date,
    string WorkCenterName,
    string ProductName,
    int TaktSec,
    int PlanPerHour,
    int PlanShift,
    int ActualShift,
    int DeviationShift,
    ProductionDayStatus Status
);