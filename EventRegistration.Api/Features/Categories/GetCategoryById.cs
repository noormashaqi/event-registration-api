using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Categories;

public record GetCategoryByIdQuery(long Id) : IRequest<CategoryDto>;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IEventRegistrationDatabase _database;

    public GetCategoryByIdHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt, UpdatedAt
            FROM Categories
            WHERE Id = @Id;";

        using var connection = _database.Open();
        var category = await connection.QuerySingleOrDefaultAsync<CategoryDto>(sql, new { request.Id });

        return category ?? throw new NotFoundException($"Category with ID {request.Id} not found");
    }
}