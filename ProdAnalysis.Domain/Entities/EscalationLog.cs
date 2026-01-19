namespace ProdAnalysis.Domain.Entities;

public sealed class EscalationLog
{
    public Guid Id { get; set; }

    public Guid DeviationEventId { get; set; }
    public DeviationEvent DeviationEvent { get; set; } = null!;

    public int Level { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Message { get; set; } = "";
}