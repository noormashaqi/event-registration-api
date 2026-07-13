using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

/// <summary>
/// Generic business-rule violation with a custom message
/// (e.g. "Event is full", "Event has already started").
/// For rules that warrant their own dedicated exception type
/// (not-found, duplicate, category-in-use, etc.) use a more
/// specific BusinessException subclass instead.
/// </summary>
public class BusinessRuleException : BusinessException
{
    public BusinessRuleException(string message)
        : base(message, StatusCodes.Status409Conflict)
    {
    }
}
