namespace EventRegistration.Api.Exceptions;

public class BusinessException : Exception
{
    public int StatusCode { get; }

    public BusinessException(string message) : base(message)
    {
        StatusCode = 400; 
    }

    public BusinessException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}