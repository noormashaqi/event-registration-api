using MediatR;
using Dapper;
using FluentValidation;
using EventRegistration.Api.Interfaces;
using EventRegistration.Api.Exceptions;

namespace EventRegistration.Api.Features.Participants;

/// <summary>
/// Command: Update an existing participant
/// Same validation as CreateParticipant
/// </summary>
public class UpdateParticipantCommand : IRequest<ParticipantResponse>
{
    public long ParticipantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateParticipantValidator : AbstractValidator<UpdateParticipantCommand>
{
    public UpdateParticipantValidator()
    {
        RuleFor(x => x.ParticipantId)
            .GreaterThan(0)
            .WithMessage("Participant ID must be greater than 0");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .Length(1, 150)
            .WithMessage("Full name must be between 1 and 150 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .Length(1, 255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .Length(1, 30)
            .WithMessage("Phone must not exceed 30 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth cannot be in the future")
            .When(x => x.DateOfBirth.HasValue);
    }
}

public class UpdateParticipantHandler : IRequestHandler<UpdateParticipantCommand, ParticipantResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public UpdateParticipantHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<ParticipantResponse> Handle(UpdateParticipantCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        // Check if participant exists
        const string participantCheckSql = @"
            SELECT COUNT(*) 
            FROM `Participants` 
            WHERE Id = @ParticipantId";

        int participantExists = await connection.ExecuteScalarAsync<int>(participantCheckSql, new { ParticipantId = request.ParticipantId });

        if (participantExists == 0)
            throw new NotFoundException($"Participant with ID {request.ParticipantId} not found");

        // Check if email already exists (and belongs to different participant)
        const string emailCheckSql = @"
            SELECT COUNT(*) 
            FROM `Participants` 
            WHERE LOWER(Email) = LOWER(@Email) AND Id != @ParticipantId";

        int emailExists = await connection.ExecuteScalarAsync<int>(emailCheckSql, new { Email = request.Email.Trim(), ParticipantId = request.ParticipantId });

        if (emailExists > 0)
            throw new DuplicateResourceException("A participant with this email already exists");

        // Update participant
        const string updateSql = @"
            UPDATE `Participants`
            SET 
                FullName = @FullName,
                Email = @Email,
                Phone = @Phone,
                DateOfBirth = @DateOfBirth,
                IsActive = @IsActive,
                UpdatedAt = UTC_TIMESTAMP()
            WHERE Id = @ParticipantId";

        var trimmedFullName = request.FullName.Trim();
        var trimmedEmail = request.Email.Trim();
        var trimmedPhone = request.Phone.Trim();

        await connection.ExecuteAsync(updateSql, new
        {
            ParticipantId = request.ParticipantId,
            FullName = trimmedFullName,
            Email = trimmedEmail,
            Phone = trimmedPhone,
            DateOfBirth = request.DateOfBirth,
            IsActive = request.IsActive ? 1 : 0
        });

        // Retrieve and return the updated participant
        const string selectSql = @"
            SELECT 
                Id,
                FullName,
                Email,
                Phone,
                DateOfBirth,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM `Participants`
            WHERE Id = @ParticipantId";

        var result = await connection.QueryFirstAsync<dynamic>(selectSql, new { ParticipantId = request.ParticipantId });

        return new ParticipantResponse
        {
            Id = result.Id,
            FullName = result.FullName,
            Email = result.Email,
            Phone = result.Phone,
            DateOfBirth = result.DateOfBirth,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }
}