using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

public sealed class Bartender : Entity
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public BartenderStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public string? ImageUrl { get; private set; }

    private Bartender(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        BartenderStatus status,
        string? notes,
        string? imageUrl,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = status;
        Notes = notes;
        ImageUrl = imageUrl;
    }

    public static Result<Bartender> Create(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        BartenderStatus status,
        string? notes,
        string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<Bartender>(BartenderErrors.FirstNameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<Bartender>(BartenderErrors.LastNameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Bartender>(BartenderErrors.EmailIsRequired);
        }

        Bartender bartender = new(
            Guid.CreateVersion7(),
            firstName,
            lastName,
            email,
            phoneNumber,
            status,
            notes,
            imageUrl,
            DateTimeOffset.UtcNow);

        return Result.Success(bartender);
    }

    public Result Update(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        BartenderStatus status,
        string? notes,
        string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(BartenderErrors.FirstNameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(BartenderErrors.LastNameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(BartenderErrors.EmailIsRequired);
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = status;
        Notes = notes;
        ImageUrl = imageUrl;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}