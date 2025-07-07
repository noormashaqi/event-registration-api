using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Events;

public class DeleteEvent
{
    public class Command : IRequest
    {
        public long Id { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly IEventRegistrationDatabase _db;

        public Handler(IEventRegistrationDatabase db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            using var connection = _db.Open();

            const string checkEventSql = "SELECT Id FROM Events WHERE Id = @Id";
            var eventExists = await connection.QueryFirstOrDefaultAsync<long?>(checkEventSql, new { Id = request.Id });
            if (eventExists == null)
                throw new NotFoundException($"Event with ID {request.Id} was not found.");

            const string checkRegistrationsSql = "SELECT COUNT(*) FROM Registrations WHERE EventId = @Id";
            var registrationCount = await connection.ExecuteScalarAsync<int>(checkRegistrationsSql, new { Id = request.Id });

            if (registrationCount > 0)
                throw new DuplicateResourceException("An event with any registration record cannot be hard deleted.");

            const string deleteSql = "DELETE FROM Events WHERE Id = @Id";
            await connection.ExecuteAsync(deleteSql, new { Id = request.Id });
        }
    }
}
