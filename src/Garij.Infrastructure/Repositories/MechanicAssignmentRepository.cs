using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class MechanicAssignmentRepository : Repository<MechanicAssignment>, IMechanicAssignmentRepository
{
    public MechanicAssignmentRepository(GarijDbContext context) : base(context)
    {
    }
}
