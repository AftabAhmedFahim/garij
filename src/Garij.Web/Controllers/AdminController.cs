using Garij.Domain.Entities;
using Garij.Domain.Enums;
using Garij.Infrastructure.Persistence;
using Garij.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garij.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly GarijDbContext _context;

    public AdminController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        GarijDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _context.StaffUsers
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateStaffUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateStaffUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), $"An account with email '{model.Email}' already exists.");
            return View(model);
        }

        var identityUser = new IdentityUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(identityUser, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Ensure role exists in Identity
        var roleName = model.Role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(identityUser, roleName);

        // Add to StaffUsers table
        _context.StaffUsers.Add(new User
        {
            IdentityUserId = identityUser.Id,
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = model.PhoneNumber.Trim(),
            Role = model.Role,
            CreatedAt = DateTime.UtcNow
        });

        // Grant the workshop staff member license access under the workshop's ownership
        var licenseSlug = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        _context.ProjectPurchases.Add(new ProjectPurchase
        {
            LicenseKey = $"GRJ-LIC-STAFF-{licenseSlug}",
            IdentityUserId = identityUser.Id,
            BuyerName = model.FullName.Trim(),
            BuyerEmail = normalizedEmail,
            WorkshopName = "Workshop Staff Member",
            Amount = 0m,
            Currency = "USD",
            PaymentMethod = "AdminCreatedStaff",
            TransactionReference = $"TXN-STAFF-{licenseSlug}",
            PurchasedAt = DateTime.UtcNow,
            Status = LicenseStatus.Active,
            IsActive = true,
            Notes = $"Staff account created by Administrator for role '{model.Role}'."
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Staff account for '{model.FullName}' has been created successfully with role '{model.Role}'.";
        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpGet]
    public async Task<IActionResult> ManageRoles()
    {
        var users = await _context.StaffUsers
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int userId, UserRole newRole)
    {
        var staffUser = await _context.StaffUsers.FindAsync(userId);
        if (staffUser == null)
        {
            TempData["ErrorMessage"] = "Staff user not found.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var identityUser = await _userManager.FindByIdAsync(staffUser.IdentityUserId);
        if (identityUser != null)
        {
            var roleName = newRole.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var currentRoles = await _userManager.GetRolesAsync(identityUser);
            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
            await _userManager.AddToRoleAsync(identityUser, roleName);
        }

        staffUser.Role = newRole;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Role for '{staffUser.FullName}' updated to '{newRole}'.";
        return RedirectToAction(nameof(ManageRoles));
    }
}
