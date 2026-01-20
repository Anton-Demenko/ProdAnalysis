namespace ProdAnalysis.Application.Dtos.Hourly;

public sealed record UpdateHourlyPlanRequestDto(
    Guid HourlyRecordId,
    int PlanQty
);
