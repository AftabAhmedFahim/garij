namespace Garij.Application.DTOs;

public class JobPartUsedDto
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public int PartId { get; set; }

    public int QuantityUsed { get; set; }

    public decimal PriceAtUsage { get; set; }
}
