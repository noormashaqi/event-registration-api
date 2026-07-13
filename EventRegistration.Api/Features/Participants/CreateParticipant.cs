using MediatR;
using Dapper;
using FluentValidation;
using EventRegistration.Api.Interfaces;
using EventRegistration.Api.Exceptions;


namespace EventRegistration.Api.Features.Participants;

/// <summary>
/// Command: Create a new participant
/// Validation: full name required (1-150), email required/unique (valid email), phone required (max 30), date of birth optional (not future)
/// </summary>
public class CreateParticipantCommand : IRequest<ParticipantResponse>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateParticipantValidator : AbstractValidator<CreateParticipantCommand>
{
    public CreateParticipantValidator()
    {
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

public class CreateParticipantHandler : IRequestHandler<CreateParticipantCommand, ParticipantResponse>
{
    private readonly IEventRegistrationDatabase _database;

    public CreateParticipantHandler(IEventRegistrationDatabase database)
    {
        _database = database;
    }

    public async Task<ParticipantResponse> Handle(CreateParticipantCommand request, CancellationToken cancellationToken)
    {
        using var connection = _database.Open();

        // Check if email already exists
        const string emailCheckSql = @"
            SELECT COUNT(*) 
            FROM `Participants` 
            WHERE LOWER(Email) = LOWER(@Email)";

        int emailExists = await connection.ExecuteScalarAsync<int>(emailCheckSql, new { Email = request.Email.Trim() });

        if (emailExists > 0)
            throw new DuplicateResourceException("A participant with this email already exists");

        // Insert participant
        const string insertSql = @"
            INSERT INTO `Participants` 
            (FullName, Email, Phone, DateOfBirth, IsActive, CreatedAt)
            VALUES 
            (@FullName, @Email, @Phone, @DateOfBirth, @IsActive, UTC_TIMESTAMP());
            
            SELECT LAST_INSERT_ID()";

        var trimmedFullName = request.FullName.Trim();
        var trimmedEmail = request.Email.Trim();
        var trimmedPhone = request.Phone.Trim();

        long participantId = await connection.ExecuteScalarAsync<long>(insertSql, new
        {
            FullName = trimmedFullName,
            Email = trimmedEmail,
            Phone = trimmedPhone,
            DateOfBirth = request.DateOfBirth,
            IsActive = request.IsActive ? 1 : 0
        });

        // Retrieve and return the created participant
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

        var result = await connection.QueryFirstAsync<dynamic>(selectSql, new { ParticipantId = participantId });

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