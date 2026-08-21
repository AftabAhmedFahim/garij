using Garij.Domain.Entities;

namespace Garij.Infrastructure.Repositories;

public interface IServiceJobRepository : IRepository<ServiceJob>
{
    Task<ServiceJob?> GetByBookingReferenceAsync(string bookingReference);
}
