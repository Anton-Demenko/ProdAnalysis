namespace ProdAnalysis.Domain.Entities;

public sealed class WorkCenter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}