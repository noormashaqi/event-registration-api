using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using FluentValidation;
using MediatR;

namespace EventRegistration.Api.Features.Categories;

public record UpdateCategoryCommand(long Id, string Name, string? Description, bool IsActive) : IRequest<CategoryDto>;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IEventRegistrationDatabase _database;

    public UpdateCategoryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var description = request.Description?.Trim();

        using var connection = _database.Open();

        const string existsSql = "SELECT COUNT(1) FROM Categories WHERE Id = @Id;";
        var exists = await connection.ExecuteScalarAsync<int>(existsSql, new { request.Id });
        if (exists == 0)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found");
        }

        const string duplicateSql = @"
            SELECT COUNT(1) FROM Categories
            WHERE LOWER(TRIM(Name)) = LOWER(@Name) AND Id <> @Id;";
        var duplicate = await connection.ExecuteScalarAsync<int>(duplicateSql, new { Name = name, request.Id });
        if (duplicate > 0)
        {
            throw new DuplicateResourceException($"A category named '{name}' already exists.");
        }

        const string updateSql = @"
            UPDATE Categories
            SET Name = @Name, Description = @Description, IsActive = @IsActive, UpdatedAt = UTC_TIMESTAMP()
            WHERE Id = @Id;";

        await connection.ExecuteAsync(updateSql, new { Name = name, Description = description, request.IsActive, request.Id });

        const string selectSql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt, UpdatedAt
            FROM Categories WHERE Id = @Id;";

        return await connection.QuerySingleAsync<CategoryDto>(selectSql, new { request.Id });
    }
}