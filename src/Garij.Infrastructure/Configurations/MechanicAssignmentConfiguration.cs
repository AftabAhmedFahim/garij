using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class MechanicAssignmentConfiguration : IEntityTypeConfiguration<MechanicAssignment>
{
    public void Configure(EntityTypeBuilder<MechanicAssignment> builder)
    {
        builder.HasKey(ma => ma.Id);
    }
}
