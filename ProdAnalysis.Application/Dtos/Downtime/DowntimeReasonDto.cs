namespace ProdAnalysis.Application.Dtos.Downtime;

public sealed record DowntimeReasonDto(
    Guid Id,
    string Code,
    string Name
);