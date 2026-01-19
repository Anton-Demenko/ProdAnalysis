namespace ProdAnalysis.Domain.Entities;

public sealed class HourlyRecord
{
    public Guid Id { get; set; }

    public Guid ProductionDayId { get; set; }
    public ProductionDay ProductionDay { get; set; } = null!;

    public int HourIndex { get; set; }
    public TimeOnly HourStart { get; set; }

    public int PlanQty { get; set; }
    public int? ActualQty { get; set; }

    public string? Comment { get; set; }

    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public AppUser? UpdatedByUser { get; set; }

    public List<HourlyDowntime> HourlyDowntimes { get; set; } = new();
}