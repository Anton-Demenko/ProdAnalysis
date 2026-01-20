using ProdAnalysis.Application.Dtos.Admin;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface IDowntimeReasonAdminService
{
    Task<IReadOnlyList<DowntimeReasonAdminDto>> ListAsync();
    Task<Guid> CreateAsync(string code, string name);
    Task UpdateAsync(Guid id, string code, string name);
    Task SetActiveAsync(Guid id, bool isActive);
}
