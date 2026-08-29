using Garij.Application.Configuration;
using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Entities;
using Garij.Domain.Exceptions;
using Garij.Infrastructure.Persistence;
using Garij.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Garij.Application.Services;

public class BillingService : IBillingService
{
    private readonly GarijDbContext _context;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly decimal _taxRatePercent;

    public BillingService(GarijDbContext context, IInvoiceRepository invoiceRepository, IOptions<BillingSettings> billingSettings)
    {
        _context = context;
        _invoiceRepository = invoiceRepository;
        _taxRatePercent = billingSettings.Value.TaxRatePercent;
    }

    /// <summary>
    /// Opens a transaction on the same scoped DbContext every repository in this request
    /// shares, so writes made through any repository (including ServiceJobService's own
    /// SaveChangesAsync call) enlist in it and roll back together on failure.
    /// </summary>
    private Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();

    /// <summary>
    /// Line totals (PriceAtBooking × Quantity, PriceAtUsage × QuantityUsed) are already at
    /// 2-decimal money precision, so they are summed as-is with no intermediate rounding.
    /// Only the resulting SubTotal is rounded (per line rounding first can drift the total
    /// by a cent versus rounding once at the end), and TaxAmount is rounded off that
    /// already-rounded SubTotal.
    /// </summary>
    private (decimal SubTotal, decimal TaxAmount, decimal TotalAmount) CalculateTotals(
        IEnumerable<JobServiceDetail> serviceDetails,
        IEnumerable<JobPartUsed> partsUsed)
    {
        var labourTotal = serviceDetails.Sum(jsd => jsd.PriceAtBooking * jsd.Quantity);
        var partsTotal = partsUsed.Sum(jpu => jpu.PriceAtUsage * jpu.QuantityUsed);

        var subTotal = Math.Round(labourTotal + partsTotal, 2, MidpointRounding.AwayFromZero);
        var taxAmount = Math.Round(subTotal * (_taxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
        var totalAmount = subTotal + taxAmount;

        return (subTotal, taxAmount, totalAmount);
    }

    /// <summary>Mirrors ServiceJobService.GenerateUniqueBookingReferenceAsync's candidate-loop shape.</summary>
    private async Task<string> GenerateUniqueInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var existingNumbers = (await _invoiceRepository.GetAllAsync())
            .Select(i => i.InvoiceNumber)
            .ToHashSet();

        var count = existingNumbers.Count + 1;
        string candidate;
        do
        {
            candidate = $"INV-{year}-{count:D4}";
            count++;
        }
        while (existingNumbers.Contains(candidate));

        return candidate;
    }

    private async Task EnsureNotAlreadyInvoicedAsync(int serviceJobId)
    {
        var existing = await _invoiceRepository.GetByServiceJobIdAsync(serviceJobId);
        if (existing is not null)
        {
            throw new BusinessRuleException("BR-010", $"Service job {serviceJobId} already has invoice '{existing.InvoiceNumber}'.");
        }
    }

    private static void EnsureHasLineItems(ServiceJob job)
    {
        if (job.JobServiceDetails.Count == 0 && job.JobPartsUsed.Count == 0)
        {
            throw new BusinessRuleException("BR-011", $"Service job {job.Id} has no billable line items — log services or parts before generating an invoice.");
        }
    }

    /// <summary>
    /// Stage 1: structure only. Stage 2 will implement this to:
    /// - Compute SubTotal as the sum of JobServiceDetail (PriceAtBooking × Quantity)
    ///   plus JobPartUsed (PriceAtUsage × QuantityUsed) for the given service job.
    /// - Compute TaxAmount and TotalAmount from that SubTotal.
    /// - Generate a unique InvoiceNumber.
    /// - Wrap invoice creation and the job's transition to Completed status in a
    ///   single EF Core transaction with rollback, so a mid-operation failure cannot
    ///   deduct stock without producing an invoice (see ROADMAP.md Stage 2, Risk Watch).
    /// </summary>
    public Task<InvoiceDto> GenerateInvoiceAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<InvoiceDto?> GetInvoiceByIdAsync(int id) => throw new NotImplementedException();

    public Task<InvoiceDto?> GetInvoiceByServiceJobAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync() => throw new NotImplementedException();

    public Task<PaymentTransactionDto> RecordPaymentAsync(PaymentTransactionDto payment) => throw new NotImplementedException();

    public Task<IEnumerable<PaymentTransactionDto>> GetPaymentsByInvoiceAsync(int invoiceId) => throw new NotImplementedException();
}
