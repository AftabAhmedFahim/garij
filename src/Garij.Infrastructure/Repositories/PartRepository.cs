using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Garij.Infrastructure.Repositories;

public class PartRepository : Repository<Part>, IPartRepository
{
    public PartRepository(GarijDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Part>> GetLowStockAsync() =>
        await DbSet.Where(p => p.QuantityInStock <= p.ReorderLevel).ToListAsync();
}
