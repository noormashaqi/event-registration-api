namespace EventRegistration.Api.Exceptions;

public class ValidationException : BusinessException
{
    public List<string> Errors { get; }

    public ValidationException(string message, List<string>? errors = null)
        : base(message, StatusCodes.Status400BadRequest)
    {
        Errors = errors ?? new List<string>();
    }
}