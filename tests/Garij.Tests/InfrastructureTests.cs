using Garij.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Garij.Tests;

public class InfrastructureTests
{
    [Fact]
    public void GarijDbContext_IsADbContext()
    {
        Assert.True(typeof(DbContext).IsAssignableFrom(typeof(GarijDbContext)));
    }
}
