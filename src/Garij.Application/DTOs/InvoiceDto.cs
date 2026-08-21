using Garij.Domain.Enums;

namespace Garij.Application.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime IssuedAt { get; set; }
}
