using ProdAnalysis.Application.Dtos.Lookups;

namespace ProdAnalysis.Application.Services.Interfaces;

public interface ILookupService
{
    Task<IReadOnlyList<WorkCenterDto>> GetWorkCentersAsync();
    Task<IReadOnlyList<ProductDto>> GetProductsAsync();
    Task<IReadOnlyList<UserDto>> GetUsersAsync();
}