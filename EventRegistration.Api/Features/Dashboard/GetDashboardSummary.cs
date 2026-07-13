using Dapper;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Dashboard;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IEventRegistrationDatabase _database;

    public GetDashboardSummaryHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();
        const string categoriesSql = "SELECT COUNT(1) FROM Categories WHERE IsActive = 1;";
        const string participantsSql = "SELECT COUNT(1) FROM Participants WHERE IsActive = 1;";
        const string upcomingEventsSql = "SELECT COUNT(1) FROM Events WHERE IsActive = 1 AND StartAt > UTC_TIMESTAMP();";
        const string activeRegistrationsSql = "SELECT COUNT(1) FROM Registrations WHERE Status = 1;";
        const string upcomingListSql = @"
            SELECT
                e.Id, e.Name, c.Name AS CategoryName, e.StartAt, e.Location, e.Capacity,
                COALESCE(r.ActiveCount, 0) AS ActiveRegistrationCount,
                e.Capacity - COALESCE(r.ActiveCount, 0) AS AvailableSeats
            FROM Events e
            INNER JOIN Categories c ON c.Id = e.CategoryId
            LEFT JOIN (
                SELECT EventId, COUNT(1) AS ActiveCount
                FROM Registrations
                WHERE Status = 1
                GROUP BY EventId
            ) r ON r.EventId = e.Id
            WHERE e.IsActive = 1 AND e.StartAt > UTC_TIMESTAMP()
            ORDER BY e.StartAt ASC
            LIMIT 5;";

        var totalActiveCategories = await connection.ExecuteScalarAsync<int>(categoriesSql);
        var totalActiveParticipants = await connection.ExecuteScalarAsync<int>(participantsSql);
        var totalUpcomingActiveEvents = await connection.ExecuteScalarAsync<int>(upcomingEventsSql);
        var totalActiveRegistrations = await connection.ExecuteScalarAsync<int>(activeRegistrationsSql);
        var upcomingEvents = (await connection.QueryAsync<UpcomingEventDto>(upcomingListSql)).ToList();

        return new DashboardSummaryDto
        {
            TotalActiveCategories = totalActiveCategories,
            TotalActiveParticipants = totalActiveParticipants,
            TotalUpcomingActiveEvents = totalUpcomingActiveEvents,
            TotalActiveRegistrations = totalActiveRegistrations,
            UpcomingEvents = upcomingEvents
        };
    }
}
