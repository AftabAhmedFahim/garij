namespace Garij.Application.DTOs;

public class JobServiceDetailDto
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public int ServiceCatalogId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtBooking { get; set; }
}
