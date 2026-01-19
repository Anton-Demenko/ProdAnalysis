namespace ProdAnalysis.Application.Dtos.Reports;

public sealed record ParetoItemDto(
    Guid DowntimeReasonId,
    string Code,
    string Name,
    int TotalMinutes,
    double Percent,
    double CumPercent
);