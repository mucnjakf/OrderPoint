using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// Who the bar buys its stock from
public sealed class Supplier : Entity
{
    public string Name { get; private set; }

    public string? ContactName { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Note { get; private set; }

    public SupplierStatus Status { get; private set; }

    private Supplier(
        Guid id,
        string name,
        string? contactName,
        string? phone,
        string? email,
        string? note,
        SupplierStatus status,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Name = name;
        ContactName = contactName;
        Phone = phone;
        Email = email;
        Note = note;
        Status = status;
    }

    public static Result<Supplier> Create(
        string name,
        string? contactName,
        string? phone,
        string? email,
        string? note,
        SupplierStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Supplier>(SupplierErrors.NameIsRequired);
        }

        Supplier supplier = new(
            Guid.CreateVersion7(),
            name,
            contactName,
            phone,
            email,
            note,
            status,
            DateTimeOffset.UtcNow);

        return Result.Success(supplier);
    }

    public Result Update(
        string name,
        string? contactName,
        string? phone,
        string? email,
        string? note,
        SupplierStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(SupplierErrors.NameIsRequired);
        }

        Name = name;
        ContactName = contactName;
        Phone = phone;
        Email = email;
        Note = note;
        Status = status;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}