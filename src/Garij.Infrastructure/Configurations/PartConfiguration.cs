using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PartNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasMany(p => p.JobPartsUsed)
            .WithOne(jpu => jpu.Part)
            .HasForeignKey(jpu => jpu.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
