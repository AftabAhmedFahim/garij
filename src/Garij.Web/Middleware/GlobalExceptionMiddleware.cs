using System.Net;
using System.Text.Json;
using Garij.Domain.Exceptions;

namespace Garij.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var isApiOrJsonRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                 context.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

        var statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.BadRequest,
            BusinessRuleException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        if (isApiOrJsonRequest)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            object responsePayload = exception switch
            {
                ValidationException valEx => new
                {
                    success = false,
                    message = valEx.Message,
                    errors = valEx.Errors,
                    statusCode = (int)statusCode
                },
                BusinessRuleException bizEx => new
                {
                    success = false,
                    message = bizEx.Message,
                    ruleCode = bizEx.RuleCode,
                    statusCode = (int)statusCode
                },
                NotFoundException nfEx => new
                {
                    success = false,
                    message = nfEx.Message,
                    entity = nfEx.EntityName,
                    key = nfEx.Key,
                    statusCode = (int)statusCode
                },
                _ => new
                {
                    success = false,
                    message = "An unexpected server error occurred.",
                    statusCode = (int)statusCode
                }
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(responsePayload, jsonOptions));
        }
        else
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.Redirect($"/Error?statusCode={(int)statusCode}");
        }
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
