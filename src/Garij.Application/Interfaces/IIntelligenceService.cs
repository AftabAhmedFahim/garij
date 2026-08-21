using Garij.Application.DTOs;

namespace Garij.Application.Interfaces;

/// <summary>Predictive / decision-support features (maintenance prediction, duration estimation, parts shortage forecasting).</summary>
public interface IIntelligenceService
{
    Task<IEnumerable<VehicleDto>> PredictMaintenanceDueAsync();

    Task<TimeSpan> EstimateJobDurationAsync(int serviceJobId);

    Task<IEnumerable<PartDto>> PredictPartsShortageAsync();

    Task<IEnumerable<ServiceCatalogDto>> SuggestServicesForVehicleAsync(int vehicleId);
}
