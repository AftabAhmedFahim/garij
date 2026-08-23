using Xunit;

namespace Garij.IntegrationTests;

public class WebApplicationTests
{
    [Fact]
    public void Architecture_Layers_ShouldBeReferencedCorrectly()
    {
        // Assert solution layer assembly types can be loaded
        var domainAssembly = typeof(Garij.Domain.Exceptions.NotFoundException).Assembly;
        var applicationAssembly = typeof(Garij.Application.DependencyInjection).Assembly;
        var infrastructureAssembly = typeof(Garij.Infrastructure.DependencyInjection).Assembly;

        Assert.NotNull(domainAssembly);
        Assert.NotNull(applicationAssembly);
        Assert.NotNull(infrastructureAssembly);
    }
}
