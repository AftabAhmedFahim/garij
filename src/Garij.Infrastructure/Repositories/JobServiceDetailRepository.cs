using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class JobServiceDetailRepository : Repository<JobServiceDetail>, IJobServiceDetailRepository
{
    public JobServiceDetailRepository(GarijDbContext context) : base(context)
    {
    }
}
