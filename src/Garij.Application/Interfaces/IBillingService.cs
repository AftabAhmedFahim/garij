using Garij.Application.DTOs;

namespace Garij.Application.Interfaces;

public interface IBillingService
{
    Task<InvoiceDto> GenerateInvoiceAsync(int serviceJobId);

    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);

    Task<InvoiceDto?> GetInvoiceByServiceJobAsync(int serviceJobId);

    Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();

    Task<PaymentTransactionDto> RecordPaymentAsync(PaymentTransactionDto payment);

    Task<IEnumerable<PaymentTransactionDto>> GetPaymentsByInvoiceAsync(int invoiceId);
}
