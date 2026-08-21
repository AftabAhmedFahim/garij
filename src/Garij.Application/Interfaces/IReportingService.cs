using Garij.Application.DTOs;

namespace Garij.Application.Interfaces;

public interface IReportingService
{
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime periodStart, DateTime periodEnd);

    Task<IEnumerable<MechanicWorkloadDto>> GetMechanicWorkloadReportAsync();

    Task<IEnumerable<PartDto>> GetLowStockReportAsync();

    Task<IEnumerable<ServiceJobDto>> GetCompletedJobsReportAsync(DateTime periodStart, DateTime periodEnd);
}
