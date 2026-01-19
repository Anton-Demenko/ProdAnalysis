namespace ProdAnalysis.Application.Dtos.Downtime;

public sealed record DeleteHourlyDowntimeRequestDto(
    Guid HourlyRecordId,
    Guid DowntimeReasonId
);