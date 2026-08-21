using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Enums;

namespace Garij.Application.Services;

public class ServiceJobService : IServiceJobService
{
    public Task<IEnumerable<ServiceJobDto>> GetAllServiceJobsAsync() => throw new NotImplementedException();

    public Task<ServiceJobDto?> GetServiceJobByIdAsync(int id) => throw new NotImplementedException();

    public Task<ServiceJobDto?> GetServiceJobByBookingReferenceAsync(string bookingReference) => throw new NotImplementedException();

    public Task<ServiceJobDto> CreateServiceJobAsync(ServiceJobDto serviceJob) => throw new NotImplementedException();

    public Task<ServiceJobDto> UpdateServiceJobAsync(ServiceJobDto serviceJob) => throw new NotImplementedException();

    public Task<ServiceJobDto> UpdateServiceJobStatusAsync(int id, JobStatus status) => throw new NotImplementedException();

    public Task DeleteServiceJobAsync(int id) => throw new NotImplementedException();

    public Task<MechanicAssignmentDto> AssignMechanicAsync(int serviceJobId, int userId, RoleInJob roleInJob) => throw new NotImplementedException();

    public Task RemoveMechanicAssignmentAsync(int assignmentId) => throw new NotImplementedException();

    public Task<IEnumerable<MechanicAssignmentDto>> GetAssignmentsByServiceJobAsync(int serviceJobId) => throw new NotImplementedException();
}
