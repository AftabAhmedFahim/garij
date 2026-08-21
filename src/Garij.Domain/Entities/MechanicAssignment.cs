using Garij.Domain.Enums;

namespace Garij.Domain.Entities;

/// <summary>Assigns a staff member (mechanic) to a ServiceJob. RoleInJob.Lead marks the lead mechanic for that job.</summary>
public class MechanicAssignment
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public ServiceJob ServiceJob { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public RoleInJob RoleInJob { get; set; }

    public DateTime AssignedAt { get; set; }
}
