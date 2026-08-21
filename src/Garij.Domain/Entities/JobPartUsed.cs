namespace Garij.Domain.Entities;

/// <summary>Line item linking a ServiceJob to a Part consumed on it.</summary>
public class JobPartUsed
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public ServiceJob ServiceJob { get; set; } = null!;

    public int PartId { get; set; }

    public Part Part { get; set; } = null!;

    public int QuantityUsed { get; set; }

    public decimal PriceAtUsage { get; set; }
}
