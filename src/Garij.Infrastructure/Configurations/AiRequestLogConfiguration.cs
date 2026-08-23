using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class AiRequestLogConfiguration : IEntityTypeConfiguration<AiRequestLog>
{
    public void Configure(EntityTypeBuilder<AiRequestLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FeatureName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PromptText).IsRequired();
        builder.Property(a => a.ResponseText).IsRequired();
        builder.Property(a => a.ErrorMessage).HasMaxLength(500);
    }
}
