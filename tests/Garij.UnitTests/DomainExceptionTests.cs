using Garij.Domain.Exceptions;
using Xunit;

namespace Garij.UnitTests;

public class DomainExceptionTests
{
    [Fact]
    public void NotFoundException_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var ex = new NotFoundException("Customer", 42);

        // Assert
        Assert.Equal("Customer", ex.EntityName);
        Assert.Equal(42, ex.Key);
        Assert.Contains("Customer", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    [Fact]
    public void ValidationException_ShouldStoreFieldErrors()
    {
        // Arrange & Act
        var ex = new ValidationException("LicensePlateNumber", "Plate is invalid.");

        // Assert
        Assert.True(ex.Errors.ContainsKey("LicensePlateNumber"));
        Assert.Equal("Plate is invalid.", ex.Errors["LicensePlateNumber"][0]);
    }

    [Fact]
    public void BusinessRuleException_ShouldStoreRuleCode()
    {
        // Arrange & Act
        var ex = new BusinessRuleException("BR-003", "Lead mechanic is required.");

        // Assert
        Assert.Equal("BR-003", ex.RuleCode);
        Assert.Equal("Lead mechanic is required.", ex.Message);
    }
}
