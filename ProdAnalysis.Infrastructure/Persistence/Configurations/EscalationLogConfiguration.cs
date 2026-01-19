using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class EscalationLogConfiguration : IEntityTypeConfiguration<EscalationLog>
{
    public void Configure(EntityTypeBuilder<EscalationLog> builder)
    {
        builder.ToTable("EscalationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Level)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.DeviationEventId, x.Level, x.CreatedAt });

        builder.HasOne(x => x.DeviationEvent)
            .WithMany(x => x.EscalationLogs)
            .HasForeignKey(x => x.DeviationEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}