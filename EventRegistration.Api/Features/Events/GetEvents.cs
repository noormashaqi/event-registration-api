using Dapper;
using EventRegistration.Api.Interfaces;
using MediatR;

namespace EventRegistration.Api.Features.Events;

public class GetEvents
{
    public class Query : IRequest<PaginatedResult<ResultItem>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public long? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PaginatedResult<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }

    public class ResultItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
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

    public class Handler : IRequestHandler<Query, PaginatedResult<ResultItem>>
    {
        private readonly IEventRegistrationDatabase _db;

        public Handler(IEventRegistrationDatabase db) => _db = db;

        public async Task<PaginatedResult<ResultItem>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _db.Open();

            var sqlBuilder = new System.Text.StringBuilder();
            var countBuilder = new System.Text.StringBuilder();
            var parameters = new DynamicParameters();

            string baseWhere = " WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                baseWhere += " AND (e.Name LIKE @Search OR e.Location LIKE @Search) ";
                parameters.Add("Search", $"%{request.Search}%");
            }
            if (request.CategoryId.HasValue)
            {
                baseWhere += " AND e.CategoryId = @CategoryId ";
                parameters.Add("CategoryId", request.CategoryId.Value);
            }
            if (request.FromDate.HasValue)
            {
                baseWhere += " AND e.StartAt >= @FromDate ";
                parameters.Add("FromDate", request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                baseWhere += " AND e.StartAt <= @ToDate ";
                parameters.Add("ToDate", request.ToDate.Value);
            }
            if (request.IsActive.HasValue)
            {
                baseWhere += " AND e.IsActive = @IsActive ";
                parameters.Add("IsActive", request.IsActive.Value);
            }

            countBuilder.Append($"SELECT COUNT(*) FROM Events e {baseWhere}");
            int totalCount = await connection.ExecuteScalarAsync<int>(countBuilder.ToString(), parameters);

            sqlBuilder.Append($@"
                SELECT e.Id, e.Name, e.CategoryId, c.Name AS CategoryName, e.Location, e.StartAt, e.EndAt, e.Capacity,
                       COALESCE(r.ActiveCount, 0) AS ActiveRegistrationCount,
                       e.Capacity - COALESCE(r.ActiveCount, 0) AS AvailableSeats,
                       e.IsActive
                FROM Events e
                INNER JOIN Categories c ON e.CategoryId = c.Id
                LEFT JOIN (
                    SELECT EventId, COUNT(1) AS ActiveCount
                    FROM Registrations
                    WHERE Status = 1
                    GROUP BY EventId
                ) r ON r.EventId = e.Id
                {baseWhere}
                ORDER BY e.StartAt ASC
                LIMIT @Offset, @Limit");

            parameters.Add("Offset", (request.Page - 1) * request.PageSize);
            parameters.Add("Limit", request.PageSize);

            var items = await connection.QueryAsync<ResultItem>(sqlBuilder.ToString(), parameters);

            return new PaginatedResult<ResultItem>
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            };
        }
    }
}
