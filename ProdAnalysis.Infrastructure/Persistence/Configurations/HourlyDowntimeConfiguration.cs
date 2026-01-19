using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class HourlyDowntimeConfiguration : IEntityTypeConfiguration<HourlyDowntime>
{
    public void Configure(EntityTypeBuilder<HourlyDowntime> builder)
    {
        builder.ToTable("HourlyDowntimes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Minutes)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.HourlyRecordId, x.DowntimeReasonId })
            .IsUnique();

        builder.HasOne(x => x.HourlyRecord)
            .WithMany(x => x.HourlyDowntimes)
            .HasForeignKey(x => x.HourlyRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DowntimeReason)
            .WithMany()
            .HasForeignKey(x => x.DowntimeReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}