using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Categories;

public record DeleteCategoryCommand(long Id) : IRequest;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IEventRegistrationDatabase _database;

    public DeleteCategoryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        const string existsSql = "SELECT COUNT(1) FROM Categories WHERE Id = @Id;";
        var exists = await connection.ExecuteScalarAsync<int>(existsSql, new { request.Id });
        if (exists == 0)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found");
        }

       /* const string usedSql = "SELECT COUNT(1) FROM Events WHERE CategoryId = @Id;";
        var used = await connection.ExecuteScalarAsync<int>(usedSql, new { request.Id });
        if (used > 0)
        {
            throw new BusinessException("This category is used by one or more events and cannot be deleted.");
        }*/ 

        const string deleteSql = "DELETE FROM Categories WHERE Id = @Id;";
        await connection.ExecuteAsync(deleteSql, new { request.Id });
    }
}