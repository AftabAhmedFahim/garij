using Garij.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Garij.Tests;

public class WebTests
{
    [Fact]
    public void DashboardController_Index_ReturnsView()
    {
        var controller = new DashboardController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }
}
