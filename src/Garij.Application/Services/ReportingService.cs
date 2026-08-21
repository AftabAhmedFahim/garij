using Garij.Application.DTOs;
using Garij.Application.Interfaces;

namespace Garij.Application.Services;

public class ReportingService : IReportingService
{
    public Task<RevenueReportDto> GetRevenueReportAsync(DateTime periodStart, DateTime periodEnd) => throw new NotImplementedException();

    public Task<IEnumerable<MechanicWorkloadDto>> GetMechanicWorkloadReportAsync() => throw new NotImplementedException();

    public Task<IEnumerable<PartDto>> GetLowStockReportAsync() => throw new NotImplementedException();

    public Task<IEnumerable<ServiceJobDto>> GetCompletedJobsReportAsync(DateTime periodStart, DateTime periodEnd) => throw new NotImplementedException();
}
