using Garij.Application.Services;
using Xunit;

namespace Garij.Tests;

public class ApplicationTests
{
    [Fact]
    public void CustomerVehicleService_CanBeInstantiated()
    {
        var service = new CustomerVehicleService();

        Assert.NotNull(service);
    }
}
