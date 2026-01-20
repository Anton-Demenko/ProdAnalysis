using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Admin;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class WorkCenterAdminService : IWorkCenterAdminService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public WorkCenterAdminService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<WorkCenterAdminDto>> ListAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.WorkCenters
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new WorkCenterAdminDto(x.Id, x.Name, x.IsActive))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(string name)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var exists = await db.WorkCenters.AsNoTracking().AnyAsync(x => x.Name == name);
        if (exists)
            throw new InvalidOperationException("WorkCenter with the same name already exists.");

        var wc = new ProdAnalysis.Domain.Entities.WorkCenter
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        db.WorkCenters.Add(wc);
        await db.SaveChangesAsync();

        return wc.Id;
    }

    public async Task UpdateNameAsync(Guid id, string name)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var wc = await db.WorkCenters.FirstOrDefaultAsync(x => x.Id == id);
        if (wc == null)
            throw new InvalidOperationException("WorkCenter not found.");

        var exists = await db.WorkCenters.AsNoTracking().AnyAsync(x => x.Id != id && x.Name == name);
        if (exists)
            throw new InvalidOperationException("WorkCenter with the same name already exists.");

        wc.Name = name;
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var wc = await db.WorkCenters.FirstOrDefaultAsync(x => x.Id == id);
        if (wc == null)
            throw new InvalidOperationException("WorkCenter not found.");

        wc.IsActive = isActive;
        await db.SaveChangesAsync();
    }
}
