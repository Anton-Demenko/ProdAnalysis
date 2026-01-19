using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public static readonly ValueConverter<DateOnly, string> DateOnlyConverter =
        new(
            d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            s => DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture)
        );

    public static readonly ValueConverter<TimeOnly, string> TimeOnlyConverter =
        new(
            t => t.ToString("HH:mm", CultureInfo.InvariantCulture),
            s => TimeOnly.ParseExact(s, "HH:mm", CultureInfo.InvariantCulture)
        );

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ProductionDay> ProductionDays => Set<ProductionDay>();
    public DbSet<HourlyRecord> HourlyRecords => Set<HourlyRecord>();

    public DbSet<DowntimeReason> DowntimeReasons => Set<DowntimeReason>();
    public DbSet<HourlyDowntime> HourlyDowntimes => Set<HourlyDowntime>();

    public DbSet<DeviationEvent> DeviationEvents => Set<DeviationEvent>();
    public DbSet<EscalationLog> EscalationLogs => Set<EscalationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}