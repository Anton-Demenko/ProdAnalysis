using ProdAnalysis.Application.Dtos.Admin;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IProductAdminService
{
    Task<IReadOnlyList<ProductAdminDto>> ListAsync();
    Task<Guid> CreateAsync(string name, string code);
    Task UpdateAsync(Guid id, string name, string code);
    Task SetActiveAsync(Guid id, bool isActive);
}
