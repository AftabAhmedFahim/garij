namespace Garij.Domain.Exceptions;

public class BusinessRuleException : Exception
{
    public string? RuleCode { get; }

    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(string ruleCode, string message) : base(message)
    {
        RuleCode = ruleCode;
    }

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
