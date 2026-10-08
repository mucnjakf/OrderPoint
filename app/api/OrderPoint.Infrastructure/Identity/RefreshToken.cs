namespace OrderPoint.Infrastructure.Identity;

internal sealed class RefreshToken
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string TokenHash { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}