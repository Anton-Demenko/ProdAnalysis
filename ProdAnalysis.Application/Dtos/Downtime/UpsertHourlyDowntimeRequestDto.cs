namespace ProdAnalysis.Application.Dtos.Downtime;

public sealed record UpsertHourlyDowntimeRequestDto(
    Guid HourlyRecordId,
    Guid DowntimeReasonId,
    int Minutes,
    string? Comment
);