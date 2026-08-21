using System.Diagnostics;
using Garij.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Garij.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Index()
    {
        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
