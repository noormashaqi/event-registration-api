using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} with id '{id}' was not found.",
               StatusCodes.Status404NotFound)
    {
    }
}