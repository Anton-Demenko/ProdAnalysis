namespace ProdAnalysis.Application.Dtos.ProductionDays;

public sealed record ProductionTotalsDto(
    int PlanShift,
    int ActualShift,
    int CumDeviation
);