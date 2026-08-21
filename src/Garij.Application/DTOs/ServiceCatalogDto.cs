namespace Garij.Application.DTOs;

public class ServiceCatalogDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int EstimatedDurationMinutes { get; set; }

    public decimal BasePrice { get; set; }
}
