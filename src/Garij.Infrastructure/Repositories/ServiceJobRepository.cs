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

    public async Task<IEnumerable<ServiceJob>> GetServiceHistoryByVehicleAsync(int vehicleId) =>
        await DbSet.Include(sj => sj.Vehicle)
            .Where(sj => sj.VehicleId == vehicleId)
            .OrderByDescending(sj => sj.CompletedAt ?? sj.CreatedAt)
            .ThenByDescending(sj => sj.CreatedAt)
            .ToListAsync();
}
