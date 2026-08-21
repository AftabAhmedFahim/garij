using Garij.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Garij.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.IdentityUserId).IsRequired();
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        builder.HasMany(u => u.MechanicAssignments)
            .WithOne(ma => ma.User)
            .HasForeignKey(ma => ma.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
