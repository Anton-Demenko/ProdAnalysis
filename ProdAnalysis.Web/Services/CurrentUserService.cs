using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Web.Services;

public sealed class CurrentUserService
{
    public event Action? Changed;

    public Guid? UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public UserRole? Role { get; private set; }

    public bool IsSet => UserId.HasValue;

    public void Set(Guid userId, string displayName, UserRole role)
    {
        UserId = userId;
        DisplayName = displayName;
        Role = role;
        Changed?.Invoke();
    }

    public void Clear()
    {
        UserId = null;
        DisplayName = null;
        Role = null;
        Changed?.Invoke();
    }
}