using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class HourlyRecordConfiguration : IEntityTypeConfiguration<HourlyRecord>
{
    public void Configure(EntityTypeBuilder<HourlyRecord> builder)
    {
        builder.ToTable("HourlyRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.HourIndex)
            .IsRequired();

        builder.Property(x => x.HourStart)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.TimeOnlyConverter)
            .HasMaxLength(5);

        builder.Property(x => x.PlanQty)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductionDayId, x.HourIndex })
            .IsUnique();

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.HourlyDowntimes)
            .WithOne(x => x.HourlyRecord)
            .HasForeignKey(x => x.HourlyRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}