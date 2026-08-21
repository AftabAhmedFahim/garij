using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class JobPartUsedRepository : Repository<JobPartUsed>, IJobPartUsedRepository
{
    public JobPartUsedRepository(GarijDbContext context) : base(context)
    {
    }
}
