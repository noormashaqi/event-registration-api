using Dapper;
using EventRegistration.Api.Exceptions;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Events;

public class GetEventById
{
    public class Query : IRequest<Result>
    {
        public long Id { get; set; }
    }

    public class Result
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public int Capacity { get; set; }
        public int ActiveRegistrationCount { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsActive { get; set; }
        public string EventStatus
        {
            get
            {
                var now = DateTime.UtcNow;
                if (now < StartAt) return "Upcoming";
                if (now >= StartAt && now <= EndAt) return "Ongoing";
                return "Completed";
            }
        }
    }

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IEventRegistrationDatabase _db;

        public Handler(IEventRegistrationDatabase db) => _db = db;

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _db.Open();
            const string sql = @"
                SELECT e.Id, e.Name, e.Description, e.CategoryId, c.Name AS CategoryName, e.Location, e.StartAt, e.EndAt, e.RegistrationDeadline, e.Capacity, e.IsActive,
                       (SELECT COUNT(*) FROM Registrations r WHERE r.EventId = e.Id AND r.Status = 1) AS ActiveRegistrationCount,
                       e.Capacity - (SELECT COUNT(*) FROM Registrations r WHERE r.EventId = e.Id AND r.Status = 1) AS AvailableSeats
                FROM Events e
                INNER JOIN Categories c ON e.CategoryId = c.Id
                WHERE e.Id = @Id";

            var result = await connection.QuerySingleOrDefaultAsync<Result>(sql, new { Id = request.Id });
            if (result == null)
                throw new NotFoundException($"Event with ID {request.Id} was not found.");

            return result;
        }
    }
}
