namespace EventRegistration.Api.Features.Dashboard;

public class UpcomingEventDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int ActiveRegistrationCount { get; set; }
    public int AvailableSeats { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalActiveCategories { get; set; }
    public int TotalActiveParticipants { get; set; }
    public int TotalUpcomingActiveEvents { get; set; }
    public int TotalActiveRegistrations { get; set; }
    public List<UpcomingEventDto> UpcomingEvents { get; set; } = new();
}
