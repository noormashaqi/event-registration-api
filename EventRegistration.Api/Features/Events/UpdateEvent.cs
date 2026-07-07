using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using FluentValidation;
using MediatR;

namespace EventRegistration.Api.Features.Events;

public class UpdateEvent
{
    public class Command : IRequest
    {
        public long Id { get; set; }
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

    public class Handler : IRequestHandler<Command>
    {
        private readonly IEventRegistrationDatabase _db;

        public Handler(IEventRegistrationDatabase db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            using var connection = _db.Open();

            const string checkSql = @"
                SELECT Capacity,
                       (SELECT COUNT(*) FROM Registrations r WHERE r.EventId = e.Id AND r.Status = 'Active') AS ActiveRegistrations
                FROM Events e WHERE e.Id = @Id";

            var eventInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(checkSql, new { Id = request.Id });
            if (eventInfo == null)
                throw new NotFoundException($"Event with ID {request.Id} was not found.");

            if (request.Capacity < (int)eventInfo.ActiveRegistrations)
                throw new DuplicateResourceException($"Capacity cannot be reduced below the current number of active registrations ({eventInfo.ActiveRegistrations}).");

            const string checkCategorySql = "SELECT IsActive FROM Categories WHERE Id = @CategoryId";
            var categoryActive = await connection.QueryFirstOrDefaultAsync<bool?>(checkCategorySql, new { request.CategoryId });
            if (categoryActive == null)
                throw new NotFoundException($"Category with ID {request.CategoryId} does not exist.");

            const string updateSql = @"
                UPDATE Events 
                SET CategoryId = @CategoryId, Name = @Name, Description = @Description, Location = @Location, 
                    StartAt = @StartAt, EndAt = @EndAt, RegistrationDeadline = @RegistrationDeadline, Capacity = @Capacity, IsActive = @IsActive
                WHERE Id = @Id";

            await connection.ExecuteAsync(updateSql, request);
        }
    }
}
