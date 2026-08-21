using Garij.Infrastructure.Persistence;
using Garij.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Garij.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<GarijDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceCatalogRepository, ServiceCatalogRepository>();
        services.AddScoped<IServiceJobRepository, ServiceJobRepository>();
        services.AddScoped<IJobServiceDetailRepository, JobServiceDetailRepository>();
        services.AddScoped<IMechanicAssignmentRepository, MechanicAssignmentRepository>();
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<IJobPartUsedRepository, JobPartUsedRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
