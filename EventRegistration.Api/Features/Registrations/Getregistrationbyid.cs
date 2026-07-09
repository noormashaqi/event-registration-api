using MediatR;
using Dapper;
using System.Data;
using EventRegistration.Api.Interfaces;
namespace EventRegistration.Api.Features.Registrations;

public class GetRegistrationByIdQuery : IRequest<RegistrationResponse>
{
    public long RegistrationId { get; set; }
}

public class GetRegistrationByIdQueryHandler : IRequestHandler<GetRegistrationByIdQuery, RegistrationResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public GetRegistrationByIdQueryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<RegistrationResponse> Handle(GetRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        const string sql = @"
            SELECT 
                r.Id, r.EventId, e.Name AS EventName, r.ParticipantId, p.FullName AS ParticipantName, 
                p.Email AS ParticipantEmail, p.Phone AS ParticipantPhone, r.Status, r.Notes, r.RegisteredAt, r.CancelledAt
            FROM `Registrations` r
            JOIN `Events` e ON r.EventId = e.Id
            JOIN `Participants` p ON r.ParticipantId = p.Id
            WHERE r.Id = @RegistrationId";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { RegistrationId = request.RegistrationId });

        if (result == null)
            throw new NotFoundException($"Registration with ID {request.RegistrationId} not found");

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