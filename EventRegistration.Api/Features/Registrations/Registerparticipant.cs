using MediatR;
using Dapper;
using FluentValidation;
using System.Data;
using EventRegistration.Api.Interfaces;

namespace EventRegistration.Api.Features.Registrations;

public class RegisterParticipantCommand : IRequest<RegistrationResponse>
{
    public long EventId { get; set; }
    public long ParticipantId { get; set; }
    public string? Notes { get; set; }
}

public class RegisterParticipantValidator : AbstractValidator<RegisterParticipantCommand>
{
    public RegisterParticipantValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0)
            .WithMessage("Event ID must be greater than 0");

        RuleFor(x => x.ParticipantId)
            .GreaterThan(0)
            .WithMessage("Participant ID must be greater than 0");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}

public class RegisterParticipantHandler : IRequestHandler<RegisterParticipantCommand, RegistrationResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public RegisterParticipantHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<RegistrationResponse> Handle(RegisterParticipantCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        const string eventCheckSql = @"
            SELECT Id, IsActive, StartAt, RegistrationDeadline, Capacity
            FROM `Events`
            WHERE Id = @EventId";

        var eventData = await connection.QueryFirstOrDefaultAsync<dynamic>(eventCheckSql, new { EventId = request.EventId });

        if (eventData == null)
            throw new NotFoundException($"Event with ID {request.EventId} not found");

        if (eventData.IsActive == 0)
            throw new BusinessException("Event is not active");

        const string participantCheckSql = @"
            SELECT Id, IsActive
            FROM `Participants`
            WHERE Id = @ParticipantId";

        var participantData = await connection.QueryFirstOrDefaultAsync<dynamic>(participantCheckSql, new { ParticipantId = request.ParticipantId });

        if (participantData == null)
            throw new NotFoundException($"Participant with ID {request.ParticipantId} not found");

        if (participantData.IsActive == 0)
            throw new BusinessException("Participant is not active");

        var currentTime = DateTime.UtcNow;
        if (currentTime > (DateTime)eventData.RegistrationDeadline)
            throw new BusinessException("Event registration deadline has passed");

        if (currentTime >= (DateTime)eventData.StartAt)
            throw new BusinessException("Event has already started");

        const string activeCountSql = @"
            SELECT COUNT(*) 
            FROM `Registrations`
            WHERE EventId = @EventId AND Status = 1";

        int activeRegistrationCount = await connection.ExecuteScalarAsync<int>(activeCountSql, new { EventId = request.EventId });
        int availableSeats = (int)eventData.Capacity - activeRegistrationCount;

        if (availableSeats <= 0)
            throw new BusinessException("Event is full");

        const string existingRegistrationSql = @"
            SELECT Id, Status
            FROM `Registrations`
            WHERE EventId = @EventId AND ParticipantId = @ParticipantId";

        var existingRegistration = await connection.QueryFirstOrDefaultAsync<dynamic>(
            existingRegistrationSql,
            new { EventId = request.EventId, ParticipantId = request.ParticipantId });

        if (existingRegistration != null && (int)existingRegistration.Status == 1)
            throw new DuplicateResourceException("Participant is already registered in this event");

        if (existingRegistration != null && (int)existingRegistration.Status == 2)
        {
            const string reactivateSql = @"
                UPDATE `Registrations`
                SET 
                    Status = 1,
                    CancelledAt = NULL,
                    Notes = @Notes
                WHERE Id = @RegistrationId";

            await connection.ExecuteAsync(reactivateSql, new
            {
                RegistrationId = existingRegistration.Id,
                Notes = request.Notes?.Trim()
            });

            return await GetRegistrationDetails(connection, existingRegistration.Id);
        }

        const string insertSql = @"
            INSERT INTO `Registrations`
            (EventId, ParticipantId, Status, Notes, RegisteredAt)
            VALUES
            (@EventId, @ParticipantId, 1, @Notes, UTC_TIMESTAMP());
            
            SELECT LAST_INSERT_ID()";

        long registrationId = await connection.ExecuteScalarAsync<long>(insertSql, new
        {
            EventId = request.EventId,
            ParticipantId = request.ParticipantId,
            Notes = request.Notes?.Trim()
        });

        return await GetRegistrationDetails(connection, registrationId);
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