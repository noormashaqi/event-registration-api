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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation exception: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.", ex.Errors?.ToList());
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found exception: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (DuplicateResourceException ex)
        {
            _logger.LogWarning(ex, "Duplicate resource exception: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status409Conflict, ex.Message);
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

        var payload = new
        {
            success = false,
            timestamp = DateTime.UtcNow,
            message,
            errors = errors ?? new List<string>()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}