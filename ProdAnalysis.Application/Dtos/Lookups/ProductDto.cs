namespace ProdAnalysis.Application.Dtos.Lookups;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Code
);