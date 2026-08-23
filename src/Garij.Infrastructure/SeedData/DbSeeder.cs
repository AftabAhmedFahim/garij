using Garij.Domain.Entities;
using Garij.Domain.Enums;
using Garij.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Garij.Infrastructure.SeedData;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GarijDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GarijDbContext>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = Enum.GetNames<UserRole>();
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Seeded Identity role: {Role}", role);
                }
            }

            // 2. Seed Default Admin Account
            const string adminEmail = "admin@garij.com";
            const string adminPassword = "Admin@12345";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, nameof(UserRole.Admin));
                    logger.LogInformation("Seeded Default Admin User: {AdminEmail}", adminEmail);

                    if (!await context.StaffUsers.AnyAsync(u => u.Email == adminEmail))
                    {
                        context.StaffUsers.Add(new User
                        {
                            IdentityUserId = adminUser.Id,
                            FullName = "System Administrator",
                            Email = adminEmail,
                            PhoneNumber = "+1234567890",
                            Role = UserRole.Admin,
                            CreatedAt = DateTime.UtcNow
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 3. Seed Service Catalog Items
            if (!await context.ServiceCatalogs.AnyAsync())
            {
                context.ServiceCatalogs.AddRange(
                    new ServiceCatalog { Name = "Oil Change & Filter Replacement", Description = "Full synthetic oil change including filter replacement", BasePrice = 50.00m, EstimatedDurationMinutes = 45 },
                    new ServiceCatalog { Name = "Brake Pad Replacement", Description = "Front or rear brake pad replacement and inspection", BasePrice = 120.00m, EstimatedDurationMinutes = 90 },
                    new ServiceCatalog { Name = "Tire Rotation & Balancing", Description = "Rotate 4 tires and balance wheels", BasePrice = 40.00m, EstimatedDurationMinutes = 40 },
                    new ServiceCatalog { Name = "Engine Computer Diagnostic", Description = "Full OBD-II diagnostic scan and troubleshooting report", BasePrice = 80.00m, EstimatedDurationMinutes = 60 },
                    new ServiceCatalog { Name = "AC Service & Gas Refill", Description = "Air conditioning pressure check, leak check, and R134a refill", BasePrice = 95.00m, EstimatedDurationMinutes = 60 },
                    new ServiceCatalog { Name = "Wheel Alignment", Description = "4-wheel laser alignment adjustment", BasePrice = 65.00m, EstimatedDurationMinutes = 50 }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded initial Service Catalog items.");
            }

            // 4. Seed Stock Parts
            if (!await context.Parts.AnyAsync())
            {
                context.Parts.AddRange(
                    new Part { Name = "Synthetic Engine Oil 5W-30 (1L)", PartNumber = "OIL-5W30", UnitPrice = 25.00m, QuantityInStock = 50, ReorderLevel = 10 },
                    new Part { Name = "Premium Brake Pads Front Set", PartNumber = "BRK-PAD-F", UnitPrice = 60.00m, QuantityInStock = 30, ReorderLevel = 5 },
                    new Part { Name = "Oil Filter Type-A", PartNumber = "FLT-OIL-A", UnitPrice = 12.00m, QuantityInStock = 40, ReorderLevel = 8 },
                    new Part { Name = "Air Filter Universal", PartNumber = "FLT-AIR-U", UnitPrice = 18.00m, QuantityInStock = 25, ReorderLevel = 5 },
                    new Part { Name = "Spark Plug Set Platinum", PartNumber = "SPK-PLG-P", UnitPrice = 45.00m, QuantityInStock = 20, ReorderLevel = 5 },
                    new Part { Name = "R134a Refrigerant Gas Canister", PartNumber = "REF-134A", UnitPrice = 35.00m, QuantityInStock = 15, ReorderLevel = 3 }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded initial Stock Parts.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
