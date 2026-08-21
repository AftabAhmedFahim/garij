using Garij.Domain.Entities;

namespace Garij.Infrastructure.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlateNumber);
}
