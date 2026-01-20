namespace ProdAnalysis.Application.Dtos.Admin;

public sealed record DowntimeReasonAdminDto(Guid Id, string Code, string Name, bool IsActive);
