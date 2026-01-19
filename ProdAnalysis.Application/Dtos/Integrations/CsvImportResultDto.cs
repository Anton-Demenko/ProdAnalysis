namespace ProdAnalysis.Application.Dtos.Integrations;

public sealed record CsvImportResultDto(
    int UpdatedRows,
    int SkippedRows,
    IReadOnlyList<string> Errors
);