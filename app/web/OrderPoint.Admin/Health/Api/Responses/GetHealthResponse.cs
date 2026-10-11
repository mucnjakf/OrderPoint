using OrderPoint.Admin.Health.Dtos;

namespace OrderPoint.Admin.Health.Api.Responses;

internal sealed record GetHealthResponse(HealthReportDto Data);