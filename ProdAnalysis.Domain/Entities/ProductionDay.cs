using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Domain.Entities;

public sealed class ProductionDay
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly ShiftStart { get; set; }
    public TimeOnly ShiftEnd { get; set; }

    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int TaktSec { get; set; }
    public int PlanPerHour { get; set; }

    public ProductionDayStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;

    public List<HourlyRecord> HourlyRecords { get; set; } = new();
}
