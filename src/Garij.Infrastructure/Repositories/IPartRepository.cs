using Garij.Domain.Entities;

namespace Garij.Infrastructure.Repositories;

public interface IPartRepository : IRepository<Part>
{
    Task<IEnumerable<Part>> GetLowStockAsync();
}
