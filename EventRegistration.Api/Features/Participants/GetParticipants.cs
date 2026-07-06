using MediatR;
using Dapper;
using EventRegistration.Api.Interfaces;
using EventRegistration.Api.Exceptions;

namespace EventRegistration.Api.Features.Participants;

/// <summary>
/// Query: Get paginated list of participants with search and filtering
/// Supports: search by name/email/phone, filter by active state, pagination
/// </summary>
public class GetParticipantsQuery : IRequest<PaginatedResult<ParticipantListItem>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class ParticipantListItem
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class GetParticipantsQueryHandler : IRequestHandler<GetParticipantsQuery, PaginatedResult<ParticipantListItem>>
{
    private readonly IEventRegistrationDatabase _database;

    public GetParticipantsQueryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<PaginatedResult<ParticipantListItem>> Handle(GetParticipantsQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
            throw new ValidationException("Invalid pagination parameters");

        using var connection = _database.Open();

        // Build dynamic WHERE clause
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        whereConditions.Add("1=1");

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = $"%{request.Search.Trim()}%";
            whereConditions.Add("(FullName LIKE @Search OR Email LIKE @Search OR Phone LIKE @Search)");
            parameters.Add("@Search", searchTerm);
        }

        if (request.IsActive.HasValue)
        {
            whereConditions.Add("IsActive = @IsActive");
            parameters.Add("@IsActive", request.IsActive.Value ? 1 : 0);
        }

        string whereClause = string.Join(" AND ", whereConditions);

        // Get total count
        string countSql = $@"
            SELECT COUNT(*) 
            FROM `Participants`
            WHERE {whereClause}";

        int totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Calculate pagination
        int offset = (request.Page - 1) * request.PageSize;
        int totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        // Get paginated participants
        string listSql = $@"
            SELECT 
                Id,
                FullName,
                Email,
                Phone,
                DateOfBirth,
                IsActive,
                CreatedAt
            FROM `Participants`
            WHERE {whereClause}
            ORDER BY CreatedAt DESC
            LIMIT @Offset, @PageSize";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var participants = await connection.QueryAsync<dynamic>(listSql, parameters);

        var items = participants.Select(p => new ParticipantListItem
        {
            Id = p.Id,
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            DateOfBirth = p.DateOfBirth,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new PaginatedResult<ParticipantListItem>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}