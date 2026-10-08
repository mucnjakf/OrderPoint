using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Api.Responses;

internal sealed record GetRevenueTrendResponse(IReadOnlyList<RevenueTrendPointDto> Data);