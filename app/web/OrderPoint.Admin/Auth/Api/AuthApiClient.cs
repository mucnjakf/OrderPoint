using OrderPoint.Admin.Auth.Api.Requests;
using OrderPoint.Admin.Auth.Api.Responses;
using OrderPoint.Admin.Auth.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Auth.Api;

internal sealed class AuthApiClient(HttpClient httpClient)
{
    internal async Task<AuthTokensDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        LoginResponse result =
            await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(LoginResponse)}");

        return result.Data;
    }

    internal async Task<AuthTokensDto> RefreshTokensAsync(
        RefreshTokensRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/auth/refresh", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        RefreshTokensResponse result =
            await response.Content.ReadFromJsonAsync<RefreshTokensResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(RefreshTokensResponse)}");

        return result.Data;
    }

    internal async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/auth/logout", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}