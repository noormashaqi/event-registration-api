using MediatR;
using Dapper;
using EventRegistration.Api.Interfaces;
using EventRegistration.Api.Exceptions;

namespace EventRegistration.Api.Features.Participants;

/// <summary>
/// Command: Delete a participant (only if no registration history)
/// Business rule: can't delete if has registrations
/// </summary>
public class DeleteParticipantCommand : IRequest<Unit>
{
    public long ParticipantId { get; set; }
}

public class DeleteParticipantHandler : IRequestHandler<DeleteParticipantCommand, Unit>
{
    private readonly IEventRegistrationDatabase _database;

    public DeleteParticipantHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<Unit> Handle(DeleteParticipantCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        // Check if participant exists
        const string participantCheckSql = @"
            SELECT COUNT(*) 
            FROM `Participants` 
            WHERE Id = @ParticipantId";

        int participantExists = await connection.ExecuteScalarAsync<int>(participantCheckSql, new { ParticipantId = request.ParticipantId });

        if (participantExists == 0)
            throw new NotFoundException($"Participant with ID {request.ParticipantId} not found");

        // Block deletion if the participant has any registration history
        const string registrationCheckSql = @"
            SELECT COUNT(*)
            FROM `Registrations`
            WHERE ParticipantId = @ParticipantId";

        int registrationCount = await connection.ExecuteScalarAsync<int>(registrationCheckSql, new { ParticipantId = request.ParticipantId });

        if (registrationCount > 0)
            throw new ParticipantHasRegistrationsException();

        // Delete the participant
        const string deleteSql = @"
            DELETE FROM `Participants`
            WHERE Id = @ParticipantId";

        await connection.ExecuteAsync(deleteSql, new { ParticipantId = request.ParticipantId });

        return Unit.Value;
    }
}