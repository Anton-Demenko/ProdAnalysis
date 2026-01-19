using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class DowntimeReasonConfiguration : IEntityTypeConfiguration<DowntimeReason>
{
    public void Configure(EntityTypeBuilder<DowntimeReason> builder)
    {
        builder.ToTable("DowntimeReasons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}