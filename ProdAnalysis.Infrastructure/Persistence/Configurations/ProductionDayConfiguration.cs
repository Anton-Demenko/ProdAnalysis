using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdAnalysis.Domain.Entities;

namespace ProdAnalysis.Infrastructure.Persistence.Configurations;

public sealed class ProductionDayConfiguration : IEntityTypeConfiguration<ProductionDay>
{
    public void Configure(EntityTypeBuilder<ProductionDay> builder)
    {
        builder.ToTable("ProductionDays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.DateOnlyConverter)
            .HasMaxLength(10);

        builder.Property(x => x.ShiftStart)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.TimeOnlyConverter)
            .HasMaxLength(5);

        builder.Property(x => x.ShiftEnd)
            .IsRequired()
            .HasConversion(Persistence.AppDbContext.TimeOnlyConverter)
            .HasMaxLength(5);

        builder.Property(x => x.TaktSec)
            .IsRequired();

        builder.Property(x => x.PlanPerHour)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.HasIndex(x => new { x.Date, x.ShiftStart, x.WorkCenterId })
            .IsUnique();

        builder.HasOne(x => x.WorkCenter)
            .WithMany()
            .HasForeignKey(x => x.WorkCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.HourlyRecords)
            .WithOne(x => x.ProductionDay)
            .HasForeignKey(x => x.ProductionDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}