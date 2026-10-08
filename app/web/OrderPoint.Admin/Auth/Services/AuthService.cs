using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Admin.Auth.Api;
using OrderPoint.Admin.Auth.Api.Requests;
using OrderPoint.Admin.Auth.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Auth.Services;

// One instance per circuit (browser tab); the tokens are kept encrypted in the browser's local storage
internal sealed class AuthService(ProtectedLocalStorage protectedLocalStorage, AuthApiClient authApiClient)
{
    private const string StorageKey = "auth-tokens";

    // Refreshing a little early means a token never expires while a request is on its way
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(1);

    // Parallel requests wait for each other, so an expiring token is refreshed only once
    private readonly SemaphoreSlim _tokensLock = new(1, 1);

    private AuthTokensDto? _tokens;

    internal event Action? SessionChanged;

    internal bool IsLoaded { get; private set; }

    internal bool IsSignedIn => _tokens is not null;

    internal bool IsSessionExpired { get; private set; }

    internal string? Email => _tokens?.Email;

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<AuthTokensDto> result =
                await protectedLocalStorage.GetAsync<AuthTokensDto>(StorageKey);

            _tokens = result.Success ? result.Value : null;
        }
        catch (CryptographicException)
        {
            // Stored with data protection keys this app no longer has, so the user signs in again
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }

        IsLoaded = true;
    }

    internal async Task LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        AuthTokensDto tokens = await authApiClient.LoginAsync(request, cancellationToken);

        await SetTokensAsync(tokens);
        IsSessionExpired = false;

        SessionChanged?.Invoke();
    }

    internal async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        AuthTokensDto? tokens = _tokens;

        // Signed out locally first, so a failed API call cannot keep the user signed in
        await ClearTokensAsync();

        SessionChanged?.Invoke();

        if (tokens is not null)
        {
            await authApiClient.LogoutAsync(new LogoutRequest(tokens.RefreshToken), cancellationToken);
        }
    }

    internal async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokensLock.WaitAsync(cancellationToken);

        try
        {
            if (_tokens is null)
            {
                throw new SessionExpiredException();
            }

            if (_tokens.AccessTokenExpiresAtUtc - RefreshMargin > DateTimeOffset.UtcNow)
            {
                return _tokens.AccessToken;
            }

            try
            {
                AuthTokensDto tokens = await authApiClient.RefreshTokensAsync(
                    new RefreshTokensRequest(_tokens.RefreshToken),
                    cancellationToken);

                await SetTokensAsync(tokens);

                return tokens.AccessToken;
            }
            catch (ApiException)
            {
                await ClearTokensAsync();
                IsSessionExpired = true;

                SessionChanged?.Invoke();

                throw new SessionExpiredException();
            }
        }
        finally
        {
            _tokensLock.Release();
        }
    }

    private async Task SetTokensAsync(AuthTokensDto tokens)
    {
        _tokens = tokens;

        await protectedLocalStorage.SetAsync(StorageKey, tokens);
    }

    private async Task ClearTokensAsync()
    {
        _tokens = null;

        await protectedLocalStorage.DeleteAsync(StorageKey);
    }
}