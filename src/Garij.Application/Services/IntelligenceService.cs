using Garij.Application.DTOs;
using Garij.Application.Interfaces;

namespace Garij.Application.Services;

public class IntelligenceService : IIntelligenceService
{
    public Task<IEnumerable<VehicleDto>> PredictMaintenanceDueAsync() => throw new NotImplementedException();

    public Task<TimeSpan> EstimateJobDurationAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<IEnumerable<PartDto>> PredictPartsShortageAsync() => throw new NotImplementedException();

    public Task<IEnumerable<ServiceCatalogDto>> SuggestServicesForVehicleAsync(int vehicleId) => throw new NotImplementedException();
}
