using MediatR;
using Dapper;
using EventRegistration.Api.Database;
using EventRegistration.Api.Exceptions;

namespace EventRegistration.Api.Features.Participants;

/// <summary>
/// Query: Get single participant by ID
/// </summary>
public class GetParticipantByIdQuery : IRequest<ParticipantResponse>
{
    public long ParticipantId { get; set; }
}

public class ParticipantResponse
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetParticipantByIdQueryHandler : IRequestHandler<GetParticipantByIdQuery, ParticipantResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public GetParticipantByIdQueryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<ParticipantResponse> Handle(GetParticipantByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        const string sql = @"
            SELECT 
                Id,
                FullName,
                Email,
                Phone,
                DateOfBirth,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM `Participants`
            WHERE Id = @ParticipantId";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { ParticipantId = request.ParticipantId });

        if (result == null)
            throw new NotFoundException($"Participant with ID {request.ParticipantId} not found");

        return new ParticipantResponse
        {
            Id = result.Id,
            FullName = result.FullName,
            Email = result.Email,
            Phone = result.Phone,
            DateOfBirth = result.DateOfBirth,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }
}