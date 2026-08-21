namespace Garij.Domain.Entities;

/// <summary>Line item linking a ServiceJob to a catalog service performed on it.</summary>
public class JobServiceDetail
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public ServiceJob ServiceJob { get; set; } = null!;

    public int ServiceCatalogId { get; set; }

    public ServiceCatalog ServiceCatalog { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal PriceAtBooking { get; set; }
}
