namespace ProdAnalysis.Application.Dtos.Admin;

public sealed record ProductAdminDto(Guid Id, string Name, string Code, bool IsActive);
