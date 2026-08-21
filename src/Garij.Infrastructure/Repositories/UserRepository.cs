using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Garij.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(GarijDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByIdentityUserIdAsync(string identityUserId) =>
        await DbSet.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
}
