using System.Net;
using System.Text.Json;
using EventRegistration.Api.Common;
using EventRegistration.Api.Exceptions;
using ValidationException = EventRegistration.Api.Exceptions.ValidationException;

namespace EventRegistration.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiErrorResponse();
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest; // 400
                response.Message = "One or more validation errors occurred.";
                response.Errors = validationEx.Errors.ToList();
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound; // 404
                response.Message = notFoundEx.Message;
                break;

            case DuplicateResourceException duplicateEx:
                statusCode = HttpStatusCode.Conflict; // 409
                response.Message = duplicateEx.Message;
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.Conflict; // 409
                response.Message = businessEx.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError; // 500
                response.Message = "An unexpected error occurred.";
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}