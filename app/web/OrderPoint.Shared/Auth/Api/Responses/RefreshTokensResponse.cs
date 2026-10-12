using OrderPoint.Shared.Auth.Dtos;

namespace OrderPoint.Shared.Auth.Api.Responses;

internal sealed record RefreshTokensResponse(AuthTokensDto Data);