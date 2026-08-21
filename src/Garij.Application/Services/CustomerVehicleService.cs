using Garij.Application.DTOs;
using Garij.Application.Interfaces;

namespace Garij.Application.Services;

public class CustomerVehicleService : ICustomerVehicleService
{
    public Task<IEnumerable<CustomerDto>> GetAllCustomersAsync() => throw new NotImplementedException();

    public Task<CustomerDto?> GetCustomerByIdAsync(int id) => throw new NotImplementedException();

    public Task<CustomerDto> CreateCustomerAsync(CustomerDto customer) => throw new NotImplementedException();

    public Task<CustomerDto> UpdateCustomerAsync(CustomerDto customer) => throw new NotImplementedException();

    public Task DeleteCustomerAsync(int id) => throw new NotImplementedException();

    public Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerAsync(int customerId) => throw new NotImplementedException();

    public Task<VehicleDto?> GetVehicleByIdAsync(int id) => throw new NotImplementedException();

    public Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlateNumber) => throw new NotImplementedException();

    public Task<VehicleDto> AddVehicleAsync(VehicleDto vehicle) => throw new NotImplementedException();

    public Task<VehicleDto> UpdateVehicleAsync(VehicleDto vehicle) => throw new NotImplementedException();

    public Task DeleteVehicleAsync(int id) => throw new NotImplementedException();
}
