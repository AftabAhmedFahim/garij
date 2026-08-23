namespace Garij.Domain.Entities;

public class AiRequestLog
{
    public int Id { get; set; }

    public string FeatureName { get; set; } = string.Empty; // e.g. "PredictiveMaintenance", "EstimateJobDuration"

    public string PromptText { get; set; } = string.Empty;

    public string ResponseText { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public long ResponseTimeMs { get; set; }
}
