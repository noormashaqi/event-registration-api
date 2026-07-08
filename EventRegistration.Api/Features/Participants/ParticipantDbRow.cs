namespace EventRegistration.Api.Features.Participants;

internal class ParticipantDbRow
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ParticipantResponse ToResponse() => new()
    {
        Id = Id,
        FullName = FullName,
        Email = Email,
        Phone = Phone,
        DateOfBirth = DateOfBirth,
        IsActive = IsActive,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };

    public ParticipantListItem ToListItem() => new()
    {
        Id = Id,
        FullName = FullName,
        Email = Email,
        Phone = Phone,
        DateOfBirth = DateOfBirth,
        IsActive = IsActive,
        CreatedAt = CreatedAt
    };
}
