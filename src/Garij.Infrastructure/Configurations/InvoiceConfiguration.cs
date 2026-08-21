using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(i => i.ServiceJobId).IsUnique();

        builder.HasMany(i => i.PaymentTransactions)
            .WithOne(pt => pt.Invoice)
            .HasForeignKey(pt => pt.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
