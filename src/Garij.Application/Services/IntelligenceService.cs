using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Enums;
using Garij.Infrastructure.Repositories;

namespace Garij.Application.Services;

public class IntelligenceService : IIntelligenceService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IServiceJobRepository _serviceJobRepository;

    public IntelligenceService(
        IVehicleRepository vehicleRepository,
        IServiceJobRepository serviceJobRepository)
    {
        _vehicleRepository = vehicleRepository;
        _serviceJobRepository = serviceJobRepository;
    }

    public async Task<IEnumerable<VehicleDto>> PredictMaintenanceDueAsync()
    {
        var predictions = await FlagVehiclesDueForServiceAsync();
        return predictions.Cast<VehicleDto>();
    }

    public async Task<IEnumerable<VehicleMaintenancePredictionDto>> FlagVehiclesDueForServiceAsync()
    {
        var vehicles = await _vehicleRepository.GetAllWithCustomersAsync();
        var allJobs = await _serviceJobRepository.GetAllWithDetailsAsync();

        var jobLookup = allJobs.GroupBy(j => j.VehicleId)
                               .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<VehicleMaintenancePredictionDto>();
        var now = DateTime.UtcNow;

        foreach (var vehicle in vehicles)
        {
            jobLookup.TryGetValue(vehicle.Id, out var vehicleJobs);
            vehicleJobs ??= new List<Domain.Entities.ServiceJob>();

            // Check if vehicle currently has an active, open job in progress
            bool hasActiveJob = vehicleJobs.Any(j => j.Status != JobStatus.Completed && j.Status != JobStatus.Cancelled);

            // If the vehicle is currently in the workshop for an active job, we can exclude it or flag it
            if (hasActiveJob)
            {
                continue;
            }

            // Completed jobs sorted chronologically descending
            var completedJobs = vehicleJobs
                .Where(j => j.Status == JobStatus.Completed)
                .OrderByDescending(j => j.CompletedAt ?? j.CreatedAt)
                .ToList();

            DateTime? lastServiceDate = null;
            int? daysSinceLastService = null;
            DateTime predictedDueDate;
            int? averageIntervalDays = null;
            string recommendedService;
            string predictionReason;
            string urgencyLevel;
            int daysOverdue = 0;
            int estimatedDaysUntilDue = 0;

            if (completedJobs.Count == 0)
            {
                // No completed service recorded yet: vehicle needs initial checkup
                lastServiceDate = null;
                daysSinceLastService = null;
                predictedDueDate = now.Date;
                daysOverdue = 14;
                estimatedDaysUntilDue = -daysOverdue;
                urgencyLevel = "Overdue";
                recommendedService = "Initial Multi-Point Inspection & Routine Service";
                predictionReason = "No prior service history recorded. Initial baseline maintenance and safety inspection required.";
            }
            else
            {
                var latestJob = completedJobs.First();
                lastServiceDate = latestJob.CompletedAt ?? latestJob.CreatedAt;
                daysSinceLastService = Math.Max(0, (int)(now.Date - lastServiceDate.Value.Date).TotalDays);

                if (completedJobs.Count >= 2)
                {
                    // Compute average interval between historical services
                    var intervals = new List<double>();
                    for (int i = 0; i < completedJobs.Count - 1; i++)
                    {
                        var current = completedJobs[i].CompletedAt ?? completedJobs[i].CreatedAt;
                        var previous = completedJobs[i + 1].CompletedAt ?? completedJobs[i + 1].CreatedAt;
                        var diffDays = (current - previous).TotalDays;
                        if (diffDays > 0)
                        {
                            intervals.Add(diffDays);
                        }
                    }

                    if (intervals.Count > 0)
                    {
                        var avg = (int)Math.Round(intervals.Average());
                        averageIntervalDays = Math.Clamp(avg, 30, 180);
                    }
                    else
                    {
                        averageIntervalDays = 90;
                    }
                }
                else
                {
                    // Single completed service: standard 90-day maintenance interval
                    averageIntervalDays = 90;
                }

                predictedDueDate = lastServiceDate.Value.Date.AddDays(averageIntervalDays.Value);
                estimatedDaysUntilDue = (int)(predictedDueDate.Date - now.Date).TotalDays;

                if (estimatedDaysUntilDue < 0)
                {
                    urgencyLevel = "Overdue";
                    daysOverdue = Math.Abs(estimatedDaysUntilDue);
                    predictionReason = $"Service is overdue by {daysOverdue} day{(daysOverdue == 1 ? "" : "s")} based on vehicle's {averageIntervalDays}-day service interval.";
                }
                else if (estimatedDaysUntilDue <= 14)
                {
                    urgencyLevel = "Due Soon";
                    daysOverdue = 0;
                    predictionReason = $"Service is due in {estimatedDaysUntilDue} day{(estimatedDaysUntilDue == 1 ? "" : "s")} based on regular {averageIntervalDays}-day maintenance cadence.";
                }
                else
                {
                    urgencyLevel = "Upcoming";
                    daysOverdue = 0;
                    predictionReason = $"Next routine maintenance forecasted in {estimatedDaysUntilDue} days.";
                }

                // Determine recommended service based on history
                if (daysSinceLastService > 180)
                {
                    recommendedService = "Major Periodic Service & Multi-Point Inspection";
                }
                else if (latestJob.JobType == JobType.RoutineService)
                {
                    recommendedService = "Routine Service, Fluid Top-Up & Oil Change";
                }
                else if (latestJob.JobType == JobType.Repair)
                {
                    recommendedService = "Post-Repair Safety Follow-up & Routine Service";
                }
                else
                {
                    recommendedService = "Scheduled Diagnostic & Preventative Maintenance";
                }
            }

            var dto = new VehicleMaintenancePredictionDto
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                CustomerName = vehicle.Customer?.FullName ?? "Unknown",
                CustomerPhoneNumber = vehicle.Customer?.PhoneNumber ?? string.Empty,
                CustomerEmail = vehicle.Customer?.Email ?? string.Empty,
                LicensePlateNumber = vehicle.LicensePlateNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Vin = vehicle.Vin,
                Color = vehicle.Color,
                LastServiceDate = lastServiceDate,
                DaysSinceLastService = daysSinceLastService,
                PredictedDueDate = predictedDueDate,
                DaysOverdue = daysOverdue,
                EstimatedDaysUntilDue = estimatedDaysUntilDue,
                AverageIntervalDays = averageIntervalDays,
                TotalServicesCompleted = completedJobs.Count,
                UrgencyLevel = urgencyLevel,
                RecommendedService = recommendedService,
                PredictionReason = predictionReason,
                HasActiveJob = hasActiveJob,
                LastJobType = completedJobs.FirstOrDefault()?.JobType.ToString()
            };

            result.Add(dto);
        }

        // Return vehicles that require attention (Overdue or Due Soon), ordered by urgency
        return result
            .Where(r => r.UrgencyLevel == "Overdue" || r.UrgencyLevel == "Due Soon")
            .OrderBy(r => r.UrgencyLevel == "Overdue" ? 0 : 1)
            .ThenBy(r => r.EstimatedDaysUntilDue)
            .ToList();
    }

    public Task<TimeSpan> EstimateJobDurationAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<IEnumerable<PartDto>> PredictPartsShortageAsync() => throw new NotImplementedException();

    public Task<IEnumerable<ServiceCatalogDto>> SuggestServicesForVehicleAsync(int vehicleId) => throw new NotImplementedException();
}
