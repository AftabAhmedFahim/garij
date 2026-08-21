namespace Garij.Application.DTOs;

public class PartDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PartNumber { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int QuantityInStock { get; set; }

    public int ReorderLevel { get; set; }
}
