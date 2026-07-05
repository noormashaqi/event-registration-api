namespace EventRegistration.Api.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resource, object id)
        : base($"{resource} with id '{id}' was not found.") { }
}