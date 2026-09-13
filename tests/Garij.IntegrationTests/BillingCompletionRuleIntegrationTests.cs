using Garij.Application.Configuration;
using Garij.Application.Services;
using Garij.Domain.Entities;
using Garij.Domain.Enums;
using Garij.Domain.Exceptions;
using Garij.Infrastructure.Persistence;
using Garij.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Garij.IntegrationTests;

/// <summary>
/// BR-008 guards the transition to Completed, and generating an invoice is the second
/// door to that transition - GenerateInvoiceAsync marks the job Completed through the
/// real ServiceJobService. These tests wire both real services together so the rule is
/// exercised through the invoicing path as well as the direct status update.
///
/// BR-008 is implemented as "at least one logged part OR one recorded service" rather
/// than the literally documented "at least one part": a labour-only job (a diagnostic,
/// an inspection) carries real work and is already invoiceable under BR-011, so a
/// parts-only reading would make such a job permanently un-invoiceable.
/// </summary>
public class BillingCompletionRuleIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<GarijDbContext> _options;

    public BillingCompletionRuleIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<GarijDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new GarijDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private static (BillingService billing, ServiceJobService jobs) CreateServices(GarijDbContext context)
    {
        var jobRepo = new ServiceJobRepository(context);
        var notificationService = new NotificationService(new NotificationRepository(context));

        var serviceJobService = new ServiceJobService(
            jobRepo,
            new VehicleRepository(context),
            new UserRepository(context),
            new MechanicAssignmentRepository(context),
            notificationService);

        var billingService = new BillingService(
            context,
            new InvoiceRepository(context),
            new PaymentTransactionRepository(context),
            jobRepo,
            serviceJobService,
            Options.Create(new BillingSettings { TaxRatePercent = 15m }));

        return (billingService, serviceJobService);
    }

    /// <summary>Seeds an InProgress job, optionally with a labour line and/or a parts line.</summary>
    private static async Task<int> SeedJobAsync(GarijDbContext context, string reference, bool withLabour, bool withParts)
    {
        var customer = new Customer { FullName = "Nadia Islam", Email = $"{reference}@test.local", PhoneNumber = "+8801711000009", Address = "Dhaka", CreatedAt = DateTime.UtcNow };
        var vehicle = new Vehicle { Customer = customer, LicensePlateNumber = reference, Make = "Toyota", Model = "Allion", Year = 2020, Vin = $"VIN-{reference}", Color = "Silver" };

        var job = new ServiceJob
        {
            Customer = customer,
            Vehicle = vehicle,
            BookingReference = reference,
            JobType = JobType.RoutineService,
            Status = JobStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        if (withLabour)
        {
            var catalog = new ServiceCatalog { Name = "Diagnostic Inspection", Description = "Full diagnostic", BasePrice = 80.00m, EstimatedDurationMinutes = 60 };
            job.JobServiceDetails.Add(new JobServiceDetail { ServiceJob = job, ServiceCatalog = catalog, Quantity = 1, PriceAtBooking = 80.00m });
        }

        if (withParts)
        {
            var part = new Part { Name = "Engine Oil 5W-30", PartNumber = $"OIL-{reference}", UnitPrice = 20.00m, QuantityInStock = 100, ReorderLevel = 10 };
            job.JobPartsUsed.Add(new JobPartUsed { ServiceJob = job, Part = part, QuantityUsed = 1, PriceAtUsage = 20.00m });
        }

        context.ServiceJobs.Add(job);
        await context.SaveChangesAsync();

        return job.Id;
    }

    [Fact]
    public async Task GenerateInvoiceAsync_InvoicesAndCompletesJob_WhenPartsAreLogged()
    {
        // Arrange
        await using var context = new GarijDbContext(_options);
        var (billing, _) = CreateServices(context);
        var jobId = await SeedJobAsync(context, "BIL-PARTS", withLabour: true, withParts: true);

        // Act
        var invoice = await billing.GenerateInvoiceAsync(jobId);

        // Assert
        Assert.Equal(100.00m, invoice.SubTotal);

        var persisted = await context.ServiceJobs.FindAsync(jobId);
        Assert.NotNull(persisted);
        Assert.Equal(JobStatus.Completed, persisted.Status);
        Assert.NotNull(persisted.CompletedAt);
    }

    [Fact]
    public async Task GenerateInvoiceAsync_InvoicesAndCompletesJob_WhenOnlyLabourIsLogged()
    {
        // Arrange: no parts at all - the case a literal "parts only" BR-008 would deadlock.
        await using var context = new GarijDbContext(_options);
        var (billing, _) = CreateServices(context);
        var jobId = await SeedJobAsync(context, "BIL-LABOUR", withLabour: true, withParts: false);

        // Act
        var invoice = await billing.GenerateInvoiceAsync(jobId);

        // Assert
        Assert.Equal(80.00m, invoice.SubTotal);
        Assert.Empty(invoice.PartLines);

        var persisted = await context.ServiceJobs.FindAsync(jobId);
        Assert.NotNull(persisted);
        Assert.Equal(JobStatus.Completed, persisted.Status);
        Assert.NotNull(persisted.CompletedAt);
    }

    [Fact]
    public async Task EmptyJob_IsRefusedByBothDoorsToCompleted_AndStaysInProgress()
    {
        // Arrange: nothing logged - no labour, no parts.
        await using var context = new GarijDbContext(_options);
        var (billing, jobs) = CreateServices(context);
        var jobId = await SeedJobAsync(context, "BIL-EMPTY", withLabour: false, withParts: false);

        // Act & Assert - door 1, the direct status update: BR-008 refuses it.
        var statusEx = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            jobs.UpdateServiceJobStatusAsync(jobId, JobStatus.Completed));
        Assert.Equal("BR-008", statusEx.RuleCode);

        // Act & Assert - door 2, invoicing: BR-011 screens the same empty job out before
        // the transaction opens, so BR-008 never has to fire on this path. Either way the
        // job cannot reach Completed with nothing logged against it.
        var invoiceEx = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            billing.GenerateInvoiceAsync(jobId));
        Assert.Equal("BR-011", invoiceEx.RuleCode);

        // The job is untouched and no invoice was written.
        var persisted = await context.ServiceJobs.FindAsync(jobId);
        Assert.NotNull(persisted);
        Assert.Equal(JobStatus.InProgress, persisted.Status);
        Assert.Null(persisted.CompletedAt);
        Assert.False(await context.Invoices.AnyAsync(i => i.ServiceJobId == jobId));
    }
}
