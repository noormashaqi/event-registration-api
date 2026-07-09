using MediatR;
using Dapper;
using System.Data;
using EventRegistration.Api.Interfaces;
namespace EventRegistration.Api.Features.Registrations;

public class GetEventRegistrationsQuery : IRequest<PaginatedResult<RegistrationListItem>>
{
    public long EventId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? Status { get; set; }
}

public class GetEventRegistrationsQueryHandler : IRequestHandler<GetEventRegistrationsQuery, PaginatedResult<RegistrationListItem>>
{
    private readonly IEventRegistrationDatabase _database;

    public GetEventRegistrationsQueryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<PaginatedResult<RegistrationListItem>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
            throw new ValidationException("Invalid pagination parameters");

        using var connection = _database.Open();
        
        const string eventCheckSql = "SELECT COUNT(*) FROM `Events` WHERE Id = @EventId";
        int eventExists = await connection.ExecuteScalarAsync<int>(eventCheckSql, new { EventId = request.EventId });
        
        if (eventExists == 0)
            throw new NotFoundException($"Event with ID {request.EventId} not found");

        var whereConditions = new List<string> { "r.EventId = @EventId" };
        var parameters = new DynamicParameters();
        parameters.Add("@EventId", request.EventId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = $"%{request.Search.Trim()}%";
            whereConditions.Add("(p.FullName LIKE @Search OR p.Email LIKE @Search OR p.Phone LIKE @Search)");
            parameters.Add("@Search", searchTerm);
        }

        if (request.Status.HasValue)
        {
            whereConditions.Add("r.Status = @Status");
            parameters.Add("@Status", request.Status.Value);
        }

        string whereClause = string.Join(" AND ", whereConditions);
        string countSql = $"SELECT COUNT(*) FROM `Registrations` r JOIN `Participants` p ON r.ParticipantId = p.Id WHERE {whereClause}";

        int totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        int offset = (request.Page - 1) * request.PageSize;
        int totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        string listSql = $@"
            SELECT 
                r.Id, r.EventId, e.Name AS EventName, r.ParticipantId, p.FullName AS ParticipantName, 
                p.Email AS ParticipantEmail, p.Phone AS ParticipantPhone, r.Status, r.Notes, r.RegisteredAt, r.CancelledAt
            FROM `Registrations` r
            JOIN `Events` e ON r.EventId = e.Id
            JOIN `Participants` p ON r.ParticipantId = p.Id
            WHERE {whereClause}
            ORDER BY r.RegisteredAt DESC
            LIMIT @Offset, @PageSize";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var registrations = await connection.QueryAsync<dynamic>(listSql, parameters);

        var items = registrations.Select(r => new RegistrationListItem
        {
            Id = r.Id,
            EventId = r.EventId,
            EventName = r.EventName,
            ParticipantId = r.ParticipantId,
            ParticipantName = r.ParticipantName,
            ParticipantEmail = r.ParticipantEmail,
            ParticipantPhone = r.ParticipantPhone,
            Status = r.Status,
            Notes = r.Notes,
            RegisteredAt = r.RegisteredAt,
            CancelledAt = r.CancelledAt
        }).ToList();

        return new PaginatedResult<RegistrationListItem>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}