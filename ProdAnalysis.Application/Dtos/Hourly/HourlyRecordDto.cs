namespace ProdAnalysis.Application.Dtos.Hourly;

public sealed record HourlyRecordDto(
    Guid Id,
    int HourIndex,
    TimeOnly HourStart,
    int PlanQty,
    int? ActualQty,
    string? Comment,
    int DowntimeMinutes,
    int CumPlanAfterThisHour,
    int CumActualAfterThisHour,
    int CumDeviationAfterThisHour,
    bool HasDeviationThisHour
);