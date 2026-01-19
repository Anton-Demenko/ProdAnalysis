using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Application.Dtos.Lookups;

public sealed record UserDto(
    Guid Id,
    string DisplayName,
    UserRole Role
);