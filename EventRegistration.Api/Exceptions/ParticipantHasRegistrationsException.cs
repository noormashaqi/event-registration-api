namespace EventRegistration.Api.Exceptions;

public class ParticipantHasRegistrationsException : BusinessException
{
    public ParticipantHasRegistrationsException()
        : base("This participant has registration history and cannot be deleted.", 400)
    {
    }
}
