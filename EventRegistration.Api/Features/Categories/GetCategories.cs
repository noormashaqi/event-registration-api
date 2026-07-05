using Dapper;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Categories;

public record GetCategoriesQuery(bool IncludeInactive) : IRequest<List<CategoryDto>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IEventRegistrationDatabase _database;

    public GetCategoriesHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt, UpdatedAt
            FROM Categories
            WHERE (@IncludeInactive = 1 OR IsActive = 1)
            ORDER BY Name ASC;";

        using var connection = _database.Open();
        var result = await connection.QueryAsync<CategoryDto>(sql, new { request.IncludeInactive });
        return result.ToList();
    }
}