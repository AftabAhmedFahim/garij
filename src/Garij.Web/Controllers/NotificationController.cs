using Garij.Application.Interfaces;
using Garij.Domain.Enums;
using Garij.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garij.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.FrontDesk))]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var notifications = await _notificationService.GetPendingNotificationsAsync();
        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification is null)
        {
            return NotFound();
        }

        return View(notification);
    }

    [HttpGet]
    public async Task<IActionResult> Respond(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification is null)
        {
            return NotFound();
        }

        return View(notification);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(int id, NotificationStatus status)
    {
        if (status != NotificationStatus.Approved && status != NotificationStatus.Rejected)
        {
            const string message = "Status must be either Approved or Rejected.";

            // Shown through the app's usual validation-summary pattern on the Respond form,
            // with the 400 kept on the response, instead of a bare 400 body with no layout.
            var rejected = await _notificationService.GetNotificationByIdAsync(id);
            if (rejected is null)
            {
                // No form to redisplay, but the submitted status is invalid either way, so the
                // refusal stays a 400 and the message is carried by the shared error view.
                return this.BadRequestView(message);
            }

            ModelState.AddModelError(string.Empty, message);
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View(rejected);
        }

        await _notificationService.RespondToNotificationAsync(id, status);
        return RedirectToAction(nameof(Index));
    }
}
