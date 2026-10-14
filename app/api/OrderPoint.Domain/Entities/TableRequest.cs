using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// A guest asks for a bartender or the bill from their table; a bartender marks it done
public sealed class TableRequest : Entity
{
    public Guid TableId { get; private set; }

    public Table Table { get; private set; } = null!;

    public TableRequestType Type { get; private set; }

    public TableRequestStatus Status { get; private set; }

    public DateTimeOffset? DoneAtUtc { get; private set; }

    private TableRequest(
        Guid id,
        Guid tableId,
        TableRequestType type,
        TableRequestStatus status,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        TableId = tableId;
        Type = type;
        Status = status;
    }

    public static Result<TableRequest> Create(Table table, TableRequestType type)
    {
        if (table.Status == TableStatus.Inactive)
        {
            return Result.Failure<TableRequest>(TableErrors.IsInactive);
        }

        TableRequest request = new(
            Guid.CreateVersion7(),
            table.Id,
            type,
            TableRequestStatus.Open,
            DateTimeOffset.UtcNow);

        return Result.Success(request);
    }

    public Result Complete()
    {
        if (Status == TableRequestStatus.Done)
        {
            return Result.Failure(TableRequestErrors.AlreadyDone);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = TableRequestStatus.Done;
        DoneAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }
}