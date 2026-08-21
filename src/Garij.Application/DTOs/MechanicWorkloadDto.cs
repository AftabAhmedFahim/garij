namespace Garij.Application.DTOs;

public class MechanicWorkloadDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int ActiveJobCount { get; set; }

    public int CompletedJobCount { get; set; }
}
