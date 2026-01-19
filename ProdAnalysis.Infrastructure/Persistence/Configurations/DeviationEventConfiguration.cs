using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class DeviationEventConfiguration : IEntityTypeConfiguration<DeviationEvent>
{
    public void Configure(EntityTypeBuilder<DeviationEvent> builder)
    {
        builder.ToTable("DeviationEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductionDate)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.DateOnlyConverter)
            .HasMaxLength(10);

        builder.Property(x => x.HourIndex)
            .IsRequired();

        builder.Property(x => x.HourStart)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.TimeOnlyConverter)
            .HasMaxLength(5);

        builder.Property(x => x.PlanQty).IsRequired();
        builder.Property(x => x.ActualQty).IsRequired();
        builder.Property(x => x.DeviationQty).IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CurrentEscalationLevel)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.HourlyRecordId);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.ProductionDate, x.WorkCenterId });

        builder.HasOne(x => x.ProductionDay)
            .WithMany()
            .HasForeignKey(x => x.ProductionDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.HourlyRecord)
            .WithMany()
            .HasForeignKey(x => x.HourlyRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcknowledgedByUser)
            .WithMany()
            .HasForeignKey(x => x.AcknowledgedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ClosedByUser)
            .WithMany()
            .HasForeignKey(x => x.ClosedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}