using OrderPoint.Shared.Auth.Dtos;

namespace OrderPoint.Shared.Account.Api.Responses;

internal sealed record ChangePasswordResponse(AuthTokensDto Data);