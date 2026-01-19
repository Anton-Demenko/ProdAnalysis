namespace ProdAnalysis.Application.Dtos.Reports;

public sealed record DailySummaryItemDto(
    Guid ProductionDayId,
    DateOnly Date,
    string WorkCenter,
    string Product,
    int TaktSec,
    int PlanShift,
    int ActualShift,
    int DeviationShift,
    int DowntimeMinutes,
    int DeviationEventsTotal,
    int DeviationEventsOpen,
    int MaxEscalationLevel
);