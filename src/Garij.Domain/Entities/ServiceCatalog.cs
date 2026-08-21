namespace Garij.Domain.Entities;

public class ServiceCatalog
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int EstimatedDurationMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public ICollection<JobServiceDetail> JobServiceDetails { get; set; } = new List<JobServiceDetail>();
}
