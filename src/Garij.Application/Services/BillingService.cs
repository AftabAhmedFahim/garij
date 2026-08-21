using Garij.Application.DTOs;
using Garij.Application.Interfaces;

namespace Garij.Application.Services;

public class BillingService : IBillingService
{
    public Task<InvoiceDto> GenerateInvoiceAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<InvoiceDto?> GetInvoiceByIdAsync(int id) => throw new NotImplementedException();

    public Task<InvoiceDto?> GetInvoiceByServiceJobAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync() => throw new NotImplementedException();

    public Task<PaymentTransactionDto> RecordPaymentAsync(PaymentTransactionDto payment) => throw new NotImplementedException();

    public Task<IEnumerable<PaymentTransactionDto>> GetPaymentsByInvoiceAsync(int invoiceId) => throw new NotImplementedException();
}
