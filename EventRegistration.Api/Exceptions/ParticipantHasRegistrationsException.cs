using Microsoft.AspNetCore.Http;

namespace EventRegistration.Api.Exceptions;

public class ParticipantHasRegistrationsException : BusinessException
{
    public ParticipantHasRegistrationsException()
        : base("This participant has registration history and cannot be deleted.", StatusCodes.Status409Conflict)
    {
    }
}
