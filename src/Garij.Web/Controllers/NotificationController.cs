using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garij.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        return View();
    }

    [HttpGet]
    public IActionResult Respond(int id)
    {
        return View();
    }
}
