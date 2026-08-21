using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class JobServiceDetailConfiguration : IEntityTypeConfiguration<JobServiceDetail>
{
    public void Configure(EntityTypeBuilder<JobServiceDetail> builder)
    {
        builder.HasKey(jsd => jsd.Id);

        builder.Property(jsd => jsd.PriceAtBooking).HasColumnType("decimal(18,2)");
    }
}
