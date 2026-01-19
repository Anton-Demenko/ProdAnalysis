namespace ProdAnalysis.Application.Options;

public sealed class DeviationOptions
{
    public int EscalationMinutes { get; set; } = 30;
    public int WorkerIntervalSeconds { get; set; } = 60;
}