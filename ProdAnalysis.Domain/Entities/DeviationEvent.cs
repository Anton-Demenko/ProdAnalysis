using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Domain.Entities;

public sealed class DeviationEvent
{
    public Guid Id { get; set; }

    public Guid ProductionDayId { get; set; }
    public ProductionDay ProductionDay { get; set; } = null!;

    public Guid HourlyRecordId { get; set; }
    public HourlyRecord HourlyRecord { get; set; } = null!;

    public Guid WorkCenterId { get; set; }
    public Guid ProductId { get; set; }

    public DateOnly ProductionDate { get; set; }
    public int HourIndex { get; set; }
    public TimeOnly HourStart { get; set; }

    public int PlanQty { get; set; }
    public int ActualQty { get; set; }
    public int DeviationQty { get; set; }

    public DeviationEventStatus Status { get; set; }
    public int CurrentEscalationLevel { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;

    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public AppUser? AcknowledgedByUser { get; set; }

    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public AppUser? ClosedByUser { get; set; }

    public string? Note { get; set; }

    public List<EscalationLog> EscalationLogs { get; set; } = new();
}