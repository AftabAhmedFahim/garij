using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Garij.Application.Services;

public class BillingService : IBillingService
{
    private readonly GarijDbContext _context;

    public BillingService(GarijDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Opens a transaction on the same scoped DbContext every repository in this request
    /// shares, so writes made through any repository (including ServiceJobService's own
    /// SaveChangesAsync call) enlist in it and roll back together on failure.
    /// </summary>
    private Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();

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
