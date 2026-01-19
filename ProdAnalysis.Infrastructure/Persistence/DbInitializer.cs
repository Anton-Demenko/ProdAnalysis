using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.WorkCenters.AnyAsync())
        {
            db.WorkCenters.AddRange(
                new WorkCenter { Id = Guid.NewGuid(), Name = "Lathe-1", IsActive = true },
                new WorkCenter { Id = Guid.NewGuid(), Name = "Lathe-2", IsActive = true }
            );
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product { Id = Guid.NewGuid(), Name = "Part-A", Code = "A", IsActive = true },
                new Product { Id = Guid.NewGuid(), Name = "Part-B", Code = "B", IsActive = true }
            );
        }

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new AppUser { Id = Guid.NewGuid(), DisplayName = "Operator Ivan", Role = UserRole.Operator, IsActive = true },
                new AppUser { Id = Guid.NewGuid(), DisplayName = "Master Petr", Role = UserRole.Master, IsActive = true },
                new AppUser { Id = Guid.NewGuid(), DisplayName = "Manager Olga", Role = UserRole.Manager, IsActive = true },
                new AppUser { Id = Guid.NewGuid(), DisplayName = "Admin Admin", Role = UserRole.Admin, IsActive = true }
            );
        }

        if (!await db.DowntimeReasons.AnyAsync())
        {
            db.DowntimeReasons.AddRange(
                new DowntimeReason { Id = Guid.NewGuid(), Code = "BRKDWN", Name = "Breakdown", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "SETUP", Name = "Setup", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "TOOL", Name = "Tool change", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "MAT", Name = "Material shortage", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "QUAL", Name = "Quality issue", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "NOOP", Name = "No operator", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "MAINT", Name = "Maintenance", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "QC", Name = "Waiting for QC", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "POWER", Name = "Power outage", IsActive = true },
                new DowntimeReason { Id = Guid.NewGuid(), Code = "OTHER", Name = "Other", IsActive = true }
            );
        }

        await db.SaveChangesAsync();
    }
}