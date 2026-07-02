namespace OrderPoint.Domain.Entities.Base;

public abstract class Entity(Guid id, DateTimeOffset createdAtUtc)
{
    public Guid Id { get; private set; } = id;

    public DateTimeOffset CreatedAtUtc { get; private set; } = createdAtUtc;

    public DateTimeOffset? UpdatedAtUtc { get; protected set; }
}