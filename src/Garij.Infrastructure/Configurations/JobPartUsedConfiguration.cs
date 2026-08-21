using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class JobPartUsedConfiguration : IEntityTypeConfiguration<JobPartUsed>
{
    public void Configure(EntityTypeBuilder<JobPartUsed> builder)
    {
        builder.HasKey(jpu => jpu.Id);

        builder.Property(jpu => jpu.PriceAtUsage).HasColumnType("decimal(18,2)");
    }
}
