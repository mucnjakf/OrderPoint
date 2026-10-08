using OrderPoint.Admin.Auth.Dtos;

namespace OrderPoint.Admin.Auth.Api.Responses;

internal sealed record LoginResponse(AuthTokensDto Data);