namespace ProdAnalysis.Domain.Entities;

public sealed class HourlyDowntime
{
    public Guid Id { get; set; }

    public Guid HourlyRecordId { get; set; }
    public HourlyRecord HourlyRecord { get; set; } = null!;

    public Guid DowntimeReasonId { get; set; }
    public DowntimeReason DowntimeReason { get; set; } = null!;

    public int Minutes { get; set; }
    public string? Comment { get; set; }

    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public AppUser? UpdatedByUser { get; set; }
}