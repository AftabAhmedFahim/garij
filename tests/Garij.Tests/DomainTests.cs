using Garij.Domain.Entities;
using Xunit;

namespace Garij.Tests;

public class DomainTests
{
    [Fact]
    public void Customer_CanBeInstantiated()
    {
        var customer = new Customer();

        Assert.NotNull(customer);
    }
}
