using Garij.Application.DTOs;
using Garij.Application.Services;
using Garij.Domain.Entities;
using Garij.Domain.Enums;
using Garij.Domain.Exceptions;
using Garij.Infrastructure.Persistence;
using Garij.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Garij.UnitTests;

public class ServiceJobServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GarijDbContext _context;
    private readonly ServiceJobService _serviceJobService;

    public ServiceJobServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GarijDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GarijDbContext(options);
        _context.Database.EnsureCreated();

        var customerRepo = new CustomerRepository(_context);
        var vehicleRepo = new VehicleRepository(_context);
        var userRepo = new UserRepository(_context);
        var jobRepo = new ServiceJobRepository(_context);
        var assignmentRepo = new MechanicAssignmentRepository(_context);

        _serviceJobService = new ServiceJobService(jobRepo, vehicleRepo, userRepo, assignmentRepo);
    }

    [Fact]
    public async Task CreateServiceJobAsync_GeneratesUniqueBookingReference_WhenBlank()
    {
        // Arrange
        var customer = new Customer { FullName = "John Doe", Email = "john@example.com", PhoneNumber = "1234567890", Address = "123 St" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-2026", Make = "Toyota", Model = "Corolla", Year = 2022, Vin = "VIN123", Color = "White" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var jobDto = new ServiceJobDto
        {
            VehicleId = vehicle.Id,
            JobType = JobType.RoutineService,
            DiagnosticNotes = "Regular oil change requested."
        };

        // Act
        var result = await _serviceJobService.CreateServiceJobAsync(jobDto);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith($"GRJ-{DateTime.UtcNow.Year}-", result.BookingReference);
        Assert.Equal(JobStatus.Requested, result.Status);
        Assert.Equal("DHA-2026", result.VehiclePlateNumber);
    }

    [Fact]
    public async Task AssignMechanicAsync_CreatesMechanicAssignmentSuccessfully()
    {
        // Arrange
        var customer = new Customer { FullName = "Jane Smith", Email = "jane@example.com", PhoneNumber = "0987654321", Address = "456 Ave" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "CTG-5005", Make = "Honda", Model = "Civic", Year = 2021, Vin = "VIN456", Color = "Black" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var identityUser = new Microsoft.AspNetCore.Identity.IdentityUser { Id = "mech-01", UserName = "sam@garij.com", Email = "sam@garij.com" };
        _context.Users.Add(identityUser);
        await _context.SaveChangesAsync();

        var mechanic = new User { IdentityUserId = identityUser.Id, FullName = "Lead Mech Sam", Email = "sam@garij.com", PhoneNumber = "1112223333", Role = UserRole.Mechanic };
        _context.StaffUsers.Add(mechanic);
        await _context.SaveChangesAsync();

        var jobDto = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto
        {
            VehicleId = vehicle.Id,
            JobType = JobType.Repair
        });

        // Act
        var assignment = await _serviceJobService.AssignMechanicAsync(jobDto.Id, mechanic.Id, RoleInJob.Lead);

        // Assert
        Assert.NotNull(assignment);
        Assert.Equal(jobDto.Id, assignment.ServiceJobId);
        Assert.Equal(mechanic.Id, assignment.UserId);
        Assert.Equal("Lead Mech Sam", assignment.MechanicName);
        Assert.Equal(RoleInJob.Lead, assignment.RoleInJob);
    }

    [Fact]
    public async Task GetServiceJobsByStatusAsync_FiltersByStatusCorrectly()
    {
        // Arrange
        var customer = new Customer { FullName = "Test Customer", Email = "test@example.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "KHL-1234", Make = "Mazda", Model = "CX-5", Year = 2023, Vin = "VIN789", Color = "Red" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var job1 = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.RoutineService, Status = JobStatus.Requested });
        var job2 = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.Repair, Status = JobStatus.InProgress });

        // Act
        var requestedJobs = await _serviceJobService.GetServiceJobsByStatusAsync(JobStatus.Requested);
        var inProgressJobs = await _serviceJobService.GetServiceJobsByStatusAsync(JobStatus.InProgress);

        // Assert
        Assert.Single(requestedJobs);
        Assert.Equal(job1.Id, requestedJobs.First().Id);

        Assert.Single(inProgressJobs);
        Assert.Equal(job2.Id, inProgressJobs.First().Id);
    }

    [Fact]
    public async Task UpdateServiceJobStatusAsync_AllowsLegalSequentialTransitions()
    {
        // Arrange
        var customer = new Customer { FullName = "Test", Email = "legal@test.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-1010", Make = "Toyota", Model = "Axio", Year = 2020, Vin = "VIN101", Color = "Silver" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var job = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.RoutineService });

        // Act & Assert: Requested -> InspectionPending
        var step1 = await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InspectionPending);
        Assert.Equal(JobStatus.InspectionPending, step1.Status);

        // InspectionPending -> CustomerApprovalNeeded
        var step2 = await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.CustomerApprovalNeeded);
        Assert.Equal(JobStatus.CustomerApprovalNeeded, step2.Status);

        // CustomerApprovalNeeded -> InProgress
        var step3 = await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InProgress);
        Assert.Equal(JobStatus.InProgress, step3.Status);
    }

    [Fact]
    public async Task UpdateServiceJobStatusAsync_ThrowsBusinessRuleException_OnInvalidTransition()
    {
        // Arrange
        var customer = new Customer { FullName = "Test", Email = "invalid@test.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-2020", Make = "Honda", Model = "Fit", Year = 2019, Vin = "VIN202", Color = "Blue" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var job = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.RoutineService });

        // Act & Assert: Direct Requested -> InProgress is illegal
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InProgress));

        Assert.Equal("BR-007", ex.RuleCode);
    }

    [Fact]
    public async Task UpdateServiceJobStatusAsync_ThrowsBusinessRuleException_WhenCompletingWithoutParts()
    {
        // Arrange
        var customer = new Customer { FullName = "Test", Email = "noparts@test.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-3030", Make = "Nissan", Model = "Sunny", Year = 2018, Vin = "VIN303", Color = "Red" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var job = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.RoutineService });
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InspectionPending);
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.CustomerApprovalNeeded);
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InProgress);

        // Act & Assert: Attempting to complete without logged parts must fail with BR-008
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.Completed));

        Assert.Equal("BR-008", ex.RuleCode);
    }

    [Fact]
    public async Task UpdateServiceJobStatusAsync_SucceedsCompletion_WhenPartsAreLogged()
    {
        // Arrange
        var customer = new Customer { FullName = "Test", Email = "withparts@test.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-4040", Make = "Toyota", Model = "Premio", Year = 2021, Vin = "VIN404", Color = "White" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var part = new Part { Name = "Brake Oil", PartNumber = "BO-01", UnitPrice = 15.00m, QuantityInStock = 50, ReorderLevel = 10 };
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        var job = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.RoutineService });
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InspectionPending);
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.CustomerApprovalNeeded);
        await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.InProgress);

        // Log a part
        _context.JobPartsUsed.Add(new JobPartUsed { ServiceJobId = job.Id, PartId = part.Id, QuantityUsed = 1, PriceAtUsage = 15.00m });
        await _context.SaveChangesAsync();

        // Act
        var completedJob = await _serviceJobService.UpdateServiceJobStatusAsync(job.Id, JobStatus.Completed);

        // Assert
        Assert.Equal(JobStatus.Completed, completedJob.Status);
        Assert.NotNull(completedJob.CompletedAt);
    }

    [Fact]
    public async Task AssignMechanicAsync_ThrowsBusinessRuleException_WhenLeadMechanicAlreadyExists()
    {
        // Arrange
        var customer = new Customer { FullName = "Test", Email = "leadmech@test.com", PhoneNumber = "123", Address = "Test" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var vehicle = new Vehicle { CustomerId = customer.Id, LicensePlateNumber = "DHA-5050", Make = "Honda", Model = "Vezel", Year = 2022, Vin = "VIN505", Color = "Black" };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var mech1User = new Microsoft.AspNetCore.Identity.IdentityUser { Id = "m1", UserName = "m1@test.com", Email = "m1@test.com" };
        var mech2User = new Microsoft.AspNetCore.Identity.IdentityUser { Id = "m2", UserName = "m2@test.com", Email = "m2@test.com" };
        _context.Users.AddRange(mech1User, mech2User);
        await _context.SaveChangesAsync();

        var mech1 = new User { IdentityUserId = mech1User.Id, FullName = "Mechanic One", Email = "m1@test.com", Role = UserRole.Mechanic };
        var mech2 = new User { IdentityUserId = mech2User.Id, FullName = "Mechanic Two", Email = "m2@test.com", Role = UserRole.Mechanic };
        _context.StaffUsers.AddRange(mech1, mech2);
        await _context.SaveChangesAsync();

        var job = await _serviceJobService.CreateServiceJobAsync(new ServiceJobDto { VehicleId = vehicle.Id, JobType = JobType.Repair });

        // Assign mech1 as Lead
        await _serviceJobService.AssignMechanicAsync(job.Id, mech1.Id, RoleInJob.Lead);

        // Act & Assert: Assigning mech2 as Lead must fail with BR-003
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _serviceJobService.AssignMechanicAsync(job.Id, mech2.Id, RoleInJob.Lead));

        Assert.Equal("BR-003", ex.RuleCode);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
