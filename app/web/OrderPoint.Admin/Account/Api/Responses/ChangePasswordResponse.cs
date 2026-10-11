using OrderPoint.Admin.Auth.Dtos;

namespace OrderPoint.Admin.Account.Api.Responses;

internal sealed record ChangePasswordResponse(AuthTokensDto Data);