using Garij.Domain.Enums;

namespace Garij.Application.DTOs;

public class PaymentTransactionDto
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public DateTime PaidAt { get; set; }
}
