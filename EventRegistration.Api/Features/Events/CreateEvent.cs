using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using FluentValidation;
using MediatR;
using ValidationException = EventRegistration.Api.Exceptions.ValidationException;

namespace EventRegistration.Api.Features.Events;

public class CreateEvent
{
    public class Command : IRequest<long>
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Capacity).InclusiveBetween(1, 10000);
            RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("EndAt must be later than StartAt.");
            RuleFor(x => x.RegistrationDeadline).LessThanOrEqualTo(x => x.StartAt).WithMessage("RegistrationDeadline must not be later than StartAt.");
        }
    }

    public class Handler : IRequestHandler<Command, long>
    {
        private readonly IEventRegistrationDatabase _db;

        public Handler(IEventRegistrationDatabase db) => _db = db;

        public async Task<long> Handle(Command request, CancellationToken cancellationToken)
        {
            using var connection = _db.Open();

            const string checkCategorySql = "SELECT IsActive FROM Categories WHERE Id = @CategoryId";
            var categoryActive = await connection.QueryFirstOrDefaultAsync<bool?>(checkCategorySql, new { request.CategoryId });

            if (categoryActive == null)
                throw new NotFoundException($"Category with ID {request.CategoryId} does not exist.");
            if (!categoryActive.Value)
                throw new ValidationException("Cannot link event to an inactive category.");

            const string insertSql = @"
                INSERT INTO Events (CategoryId, Name, Description, Location, StartAt, EndAt, RegistrationDeadline, Capacity, IsActive)
                VALUES (@CategoryId, @Name, @Description, @Location, @StartAt, @EndAt, @RegistrationDeadline, @Capacity, @IsActive);
                SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<long>(insertSql, request);
        }
    }
}
