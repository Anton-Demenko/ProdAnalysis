using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.ProductionDays;

public sealed record ProductionDayListItemDto(
    Guid Id,
    DateOnly Date,
    string WorkCenterName,
    string ProductName,
    int TaktSec,
    int PlanPerHour,
    ProductionDayStatus Status,
    int CumDeviationNow
);