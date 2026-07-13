using MediatR;
using Dapper;
using System.Data;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;

namespace EventRegistration.Api.Features.Registrations;

public class CancelRegistrationCommand : IRequest<RegistrationResponse>
{
    public long RegistrationId { get; set; }
}

public class CancelRegistrationHandler : IRequestHandler<CancelRegistrationCommand, RegistrationResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public CancelRegistrationHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<RegistrationResponse> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        const string registrationCheckSql = @"
            SELECT r.Id, r.Status, e.StartAt
            FROM `Registrations` r
            JOIN `Events` e ON r.EventId = e.Id
            WHERE r.Id = @RegistrationId";

        var registration = await connection.QueryFirstOrDefaultAsync<dynamic>(registrationCheckSql, new { RegistrationId = request.RegistrationId });

        if (registration == null)
            throw new NotFoundException($"Registration with ID {request.RegistrationId} not found");

        if ((int)registration.Status == 2)
        {
            return await GetRegistrationDetails(connection, request.RegistrationId);
        }

        var currentTime = DateTime.UtcNow;
        if (currentTime >= (DateTime)registration.StartAt)
            throw new BusinessRuleException("Cannot cancel registration after event has started");

        const string cancelSql = @"
            UPDATE `Registrations`
            SET 
                Status = 2,
                CancelledAt = UTC_TIMESTAMP()
            WHERE Id = @RegistrationId";

        await connection.ExecuteAsync(cancelSql, new { RegistrationId = request.RegistrationId });

        return await GetRegistrationDetails(connection, request.RegistrationId);
    }

    private async Task<RegistrationResponse> GetRegistrationDetails(IDbConnection connection, long registrationId)
    {
        const string selectSql = @"
            SELECT 
                r.Id, r.EventId, e.Name AS EventName, r.ParticipantId, p.FullName AS ParticipantName, 
                p.Email AS ParticipantEmail, p.Phone AS ParticipantPhone, r.Status, r.Notes, r.RegisteredAt, r.CancelledAt
            FROM `Registrations` r
            JOIN `Events` e ON r.EventId = e.Id
            JOIN `Participants` p ON r.ParticipantId = p.Id
            WHERE r.Id = @RegistrationId";

        var result = await connection.QueryFirstAsync<dynamic>(selectSql, new { RegistrationId = registrationId });

        return new RegistrationResponse
        {
            Id = result.Id,
            EventId = result.EventId,
            EventName = result.EventName,
            ParticipantId = result.ParticipantId,
            ParticipantName = result.ParticipantName,
            ParticipantEmail = result.ParticipantEmail,
            ParticipantPhone = result.ParticipantPhone,
            Status = result.Status,
            Notes = result.Notes,
            RegisteredAt = result.RegisteredAt,
            CancelledAt = result.CancelledAt
        };
    }
}