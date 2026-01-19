namespace ProdAnalysis.Application.Dtos.Downtime;

public sealed record HourlyDowntimeDto(
    Guid Id,
    Guid DowntimeReasonId,
    string DowntimeReasonCode,
    string DowntimeReasonName,
    int Minutes,
    string? Comment
);