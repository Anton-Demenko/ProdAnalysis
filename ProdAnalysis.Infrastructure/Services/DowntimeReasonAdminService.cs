using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Admin;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class DowntimeReasonAdminService : IDowntimeReasonAdminService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DowntimeReasonAdminService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<DowntimeReasonAdminDto>> ListAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.DowntimeReasons
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new DowntimeReasonAdminDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(string code, string name)
    {
        code = (code ?? "").Trim();
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var codeExists = await db.DowntimeReasons.AsNoTracking().AnyAsync(x => x.Code == code);
        if (codeExists)
            throw new InvalidOperationException("Code already exists.");

        var nameExists = await db.DowntimeReasons.AsNoTracking().AnyAsync(x => x.Name == name);
        if (nameExists)
            throw new InvalidOperationException("Name already exists.");

        var entity = new DowntimeReason
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsActive = true
        };

        db.DowntimeReasons.Add(entity);
        await db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, string code, string name)
    {
        code = (code ?? "").Trim();
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var entity = await db.DowntimeReasons.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            throw new InvalidOperationException("DowntimeReason not found.");

        var codeExists = await db.DowntimeReasons.AsNoTracking().AnyAsync(x => x.Id != id && x.Code == code);
        if (codeExists)
            throw new InvalidOperationException("Code already exists.");

        var nameExists = await db.DowntimeReasons.AsNoTracking().AnyAsync(x => x.Id != id && x.Name == name);
        if (nameExists)
            throw new InvalidOperationException("Name already exists.");

        entity.Code = code;
        entity.Name = name;

        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var entity = await db.DowntimeReasons.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            throw new InvalidOperationException("DowntimeReason not found.");

        entity.IsActive = isActive;
        await db.SaveChangesAsync();
    }
}
