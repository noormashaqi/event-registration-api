using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

public class DuplicateResourceException : BusinessException
{
    public DuplicateResourceException(string message)
        : base(message, StatusCodes.Status409Conflict)
    {
    }
}