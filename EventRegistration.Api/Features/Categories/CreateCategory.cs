using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using FluentValidation;
using MediatR;

namespace EventRegistration.Api.Features.Categories;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<CategoryDto>;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IEventRegistrationDatabase _database;

    public CreateCategoryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var description = request.Description?.Trim();

        using var connection = _database.Open();

        const string duplicateSql = "SELECT COUNT(1) FROM Categories WHERE LOWER(TRIM(Name)) = LOWER(@Name);";
        var exists = await connection.ExecuteScalarAsync<int>(duplicateSql, new { Name = name });
        if (exists > 0)
        {
            throw new DuplicateResourceException($"A category named '{name}' already exists.");
        }

        const string insertSql = @"
            INSERT INTO Categories (Name, Description, IsActive, CreatedAt)
            VALUES (@Name, @Description, 1, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();";

        var newId = await connection.ExecuteScalarAsync<long>(insertSql, new { Name = name, Description = description });

        const string selectSql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt, UpdatedAt
            FROM Categories WHERE Id = @Id;";

        return await connection.QuerySingleAsync<CategoryDto>(selectSql, new { Id = newId });
    }
}