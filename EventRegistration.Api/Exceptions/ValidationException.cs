using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

public class ValidationException : BusinessException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Validation failed.", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }
}