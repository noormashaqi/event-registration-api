using System;
using System.Collections.Generic;
using System.Data;
using EventRegistration.Api.Interfaces;
namespace EventRegistration.Api.Features.Registrations;

/// <summary>
/// Shared types for registrations feature
/// </summary>

public class RegistrationListItem
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public long ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public string ParticipantPhone { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName => Status == 1 ? "Active" : "Cancelled";
    public string? Notes { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class RegistrationResponse
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public long ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public string ParticipantPhone { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName => Status == 1 ? "Active" : "Cancelled";
    public string? Notes { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

// Exceptions
public class ValidationException : Exception
{
    public List<string> Errors { get; set; } = new();

    public ValidationException(string message, List<string>? errors = null) : base(message)
    {
        if (errors != null) Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class DuplicateResourceException : Exception
{
    public DuplicateResourceException(string message) : base(message) { }
}