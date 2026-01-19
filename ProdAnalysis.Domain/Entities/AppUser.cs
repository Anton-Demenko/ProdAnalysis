using ProdAnalysis.Domain.Enums;

namespace ProdAnalysis.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}