using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(GarijDbContext context) : base(context)
    {
    }
}
