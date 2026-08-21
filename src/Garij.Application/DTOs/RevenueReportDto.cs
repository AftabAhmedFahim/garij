namespace Garij.Application.DTOs;

public class RevenueReportDto
{
    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public decimal TotalRevenue { get; set; }

    public int InvoiceCount { get; set; }
}
