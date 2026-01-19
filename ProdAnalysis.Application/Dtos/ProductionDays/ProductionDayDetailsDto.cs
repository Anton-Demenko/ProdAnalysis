using ProdAnalysis.Application.Dtos.Hourly;
using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.ProductionDays;

public sealed record ProductionDayDetailsDto(
    Guid Id,
    DateOnly Date,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    Guid WorkCenterId,
    string WorkCenterName,
    Guid ProductId,
    string ProductName,
    int TaktSec,
    int PlanPerHour,
    ProductionDayStatus Status,
    IReadOnlyList<HourlyRecordDto> HourlyRecords,
    ProductionTotalsDto Totals
);