using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class ServiceCatalogRepository : Repository<ServiceCatalog>, IServiceCatalogRepository
{
    public ServiceCatalogRepository(GarijDbContext context) : base(context)
    {
    }
}
