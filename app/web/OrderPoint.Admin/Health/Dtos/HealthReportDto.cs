using OrderPoint.Admin.Health.Enumerations;

namespace OrderPoint.Admin.Health.Dtos;

public sealed record HealthReportDto(HealthStatus Status, double TotalDurationMs, IReadOnlyList<HealthCheckDto> Checks);