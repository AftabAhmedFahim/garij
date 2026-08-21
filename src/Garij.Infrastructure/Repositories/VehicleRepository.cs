using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Garij.Infrastructure.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(GarijDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlateNumber) =>
        await DbSet.FirstOrDefaultAsync(v => v.LicensePlateNumber == licensePlateNumber);
}
