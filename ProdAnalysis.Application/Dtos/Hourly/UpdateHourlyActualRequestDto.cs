namespace ProdAnalysis.Application.Dtos.Hourly;

public sealed record UpdateHourlyActualRequestDto(
    Guid HourlyRecordId,
    int? ActualQty,
    string? Comment
);