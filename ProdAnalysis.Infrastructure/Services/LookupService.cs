using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Lookups;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Infrastructure.Persistence;
using System;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class LookupService : ILookupService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public LookupService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<WorkCenterDto>> GetWorkCentersAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.WorkCenters
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new WorkCenterDto(x.Id, x.Name))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto(x.Id, x.Name, x.Code))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayName)
            .Select(x => new UserDto(x.Id, x.DisplayName, x.Role))
            .ToListAsync();
    }
}