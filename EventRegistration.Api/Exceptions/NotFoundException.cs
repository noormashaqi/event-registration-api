namespace EventRegistration.Api.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}