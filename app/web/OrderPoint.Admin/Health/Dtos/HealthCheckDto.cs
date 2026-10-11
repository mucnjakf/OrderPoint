using OrderPoint.Admin.Health.Enumerations;

namespace OrderPoint.Admin.Health.Dtos;

public sealed record HealthCheckDto(
    string Name,
    HealthStatus Status,
    double DurationMs,
    string? Description,
    string? Error);