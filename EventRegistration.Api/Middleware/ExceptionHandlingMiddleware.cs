using System.Text.Json;
using EventRegistration.Api.Exceptions;

namespace EventRegistration.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
            await WriteErrorResponse(context, ex.StatusCode, ex.Message, ex is ValidationException validationEx ? validationEx.Errors : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message, List<string>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new Dictionary<string, object> { ["message"] = message };
        if (errors is { Count: > 0 })
            payload["errors"] = errors;

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
