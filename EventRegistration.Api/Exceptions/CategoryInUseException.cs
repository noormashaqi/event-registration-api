using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

public class CategoryInUseException : BusinessException
{
    public CategoryInUseException()
        : base("This category is used by one or more events and cannot be deleted.", StatusCodes.Status409Conflict)
    {
    }
}