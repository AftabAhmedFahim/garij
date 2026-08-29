using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Enums;
using Garij.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garij.Web.Controllers;

[AllowAnonymous]
public class StatusLookupController : Controller
{
    private readonly ICustomerVehicleService _customerVehicleService;
    private readonly IServiceJobService _serviceJobService;

    public StatusLookupController(
        ICustomerVehicleService customerVehicleService,
        IServiceJobService serviceJobService)
    {
        _customerVehicleService = customerVehicleService;
        _serviceJobService = serviceJobService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new StatusLookupViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Result(string? query, string? plateNumber, string? bookingReference)
    {
        var lookupValue = FirstProvided(query, bookingReference, plateNumber);
        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new StatusLookupViewModel
        {
            Query = lookupValue,
            WasSearched = true
        };

        var job = await _serviceJobService.GetServiceJobByBookingReferenceAsync(lookupValue);
        if (job is not null)
        {
            model.MatchedByBookingReference = true;
            model.CurrentJob = job;
            model.Vehicle = await _customerVehicleService.GetVehicleByIdAsync(job.VehicleId);
            model.ServiceHistory = (await _customerVehicleService.GetServiceHistoryByVehicleAsync(job.VehicleId)).ToList();
            return View(model);
        }

        var vehicle = await _customerVehicleService.GetVehicleByLicensePlateAsync(lookupValue);
        if (vehicle is null)
        {
            model.Message = $"No booking or vehicle was found for \"{lookupValue}\".";
            return View(model);
        }

        var history = (await _customerVehicleService.GetServiceHistoryByVehicleAsync(vehicle.Id)).ToList();
        model.Vehicle = vehicle;
        model.ServiceHistory = history;
        model.CurrentJob = SelectCurrentJob(history);

        return View(model);
    }

    private static string FirstProvided(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static ServiceJobDto? SelectCurrentJob(IReadOnlyList<ServiceHistoryDto> history)
    {
        var current = history.FirstOrDefault(job => job.Status is not JobStatus.Completed and not JobStatus.Cancelled)
            ?? history.FirstOrDefault();

        if (current is null)
        {
            return null;
        }

        return new ServiceJobDto
        {
            Id = current.ServiceJobId,
            VehiclePlateNumber = current.VehiclePlate,
            VehicleDescription = current.VehicleDescription,
            BookingReference = current.BookingReference,
            JobType = current.JobType,
            Status = current.Status,
            CreatedAt = current.CreatedAt,
            CompletedAt = current.CompletedAt
        };
    }
}
