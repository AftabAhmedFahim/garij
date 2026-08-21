using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Garij.Infrastructure.Repositories;

public class ServiceJobRepository : Repository<ServiceJob>, IServiceJobRepository
{
    public ServiceJobRepository(GarijDbContext context) : base(context)
    {
    }

    public async Task<ServiceJob?> GetByBookingReferenceAsync(string bookingReference) =>
        await DbSet.FirstOrDefaultAsync(sj => sj.BookingReference == bookingReference);
}
