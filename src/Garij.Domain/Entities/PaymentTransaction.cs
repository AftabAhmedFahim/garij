using Garij.Domain.Enums;

namespace Garij.Domain.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public DateTime PaidAt { get; set; }
}
