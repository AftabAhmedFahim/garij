using Garij.Domain.Entities;
using Garij.Infrastructure.Persistence;

namespace Garij.Infrastructure.Repositories;

public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(GarijDbContext context) : base(context)
    {
    }
}
