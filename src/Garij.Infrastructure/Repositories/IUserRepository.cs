using Garij.Domain.Entities;

namespace Garij.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdentityUserIdAsync(string identityUserId);
}
