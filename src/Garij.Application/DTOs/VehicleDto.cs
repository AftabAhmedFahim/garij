namespace Garij.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string LicensePlateNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string Vin { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
}
