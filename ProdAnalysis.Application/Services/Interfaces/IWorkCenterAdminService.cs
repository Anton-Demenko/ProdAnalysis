using ProdAnalysis.Application.Dtos.Admin;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IWorkCenterAdminService
{
    Task<IReadOnlyList<WorkCenterAdminDto>> ListAsync();
    Task<Guid> CreateAsync(string name);
    Task UpdateNameAsync(Guid id, string name);
    Task SetActiveAsync(Guid id, bool isActive);
}
