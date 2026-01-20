using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Admin;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class ProductAdminService : IProductAdminService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProductAdminService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ProductAdminDto>> ListAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductAdminDto(x.Id, x.Name, x.Code, x.IsActive))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(string name, string code)
    {
        name = (name ?? "").Trim();
        code = (code ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var nameExists = await db.Products.AsNoTracking().AnyAsync(x => x.Name == name);
        if (nameExists)
            throw new InvalidOperationException("Product with the same name already exists.");

        var codeExists = await db.Products.AsNoTracking().AnyAsync(x => x.Code == code);
        if (codeExists)
            throw new InvalidOperationException("Product with the same code already exists.");

        var p = new ProdAnalysis.Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            IsActive = true
        };

        db.Products.Add(p);
        await db.SaveChangesAsync();

        return p.Id;
    }

    public async Task UpdateAsync(Guid id, string name, string code)
    {
        name = (name ?? "").Trim();
        code = (code ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null)
            throw new InvalidOperationException("Product not found.");

        var nameExists = await db.Products.AsNoTracking().AnyAsync(x => x.Id != id && x.Name == name);
        if (nameExists)
            throw new InvalidOperationException("Product with the same name already exists.");

        var codeExists = await db.Products.AsNoTracking().AnyAsync(x => x.Id != id && x.Code == code);
        if (codeExists)
            throw new InvalidOperationException("Product with the same code already exists.");

        p.Name = name;
        p.Code = code;

        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null)
            throw new InvalidOperationException("Product not found.");

        p.IsActive = isActive;
        await db.SaveChangesAsync();
    }
}
