using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Shared.Auth.Api;
using OrderPoint.Shared.Auth.Api.Requests;
using OrderPoint.Shared.Auth.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Shared.Auth.Services;

// One instance per circuit (browser tab); the tokens are kept encrypted in the browser's local storage
internal sealed class AuthService(ProtectedLocalStorage protectedLocalStorage, AuthApiClient authApiClient)
{
    private const string StorageKey = "auth-tokens";

    private const string PasswordChangeRequiredCode = "Auth.PasswordChangeRequired";

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

    // True when the account still has the temporary password the admin set and must choose its own first;
    // any other failure throws, so ApiService's default (false) never asks for a new password
    internal async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        AuthTokensDto tokens;

        try
        {
            tokens = await authApiClient.LoginAsync(request, cancellationToken);
        }
        catch (ApiException exception) when (exception.HasErrorCode(PasswordChangeRequiredCode))
        {
            return true;
        }

        await StartSessionAsync(tokens);

        return false;
    }

    // First sign-in of a bartender: replaces the temporary password and signs in
    internal async Task ChangeInitialPasswordAsync(
        ChangeInitialPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        AuthTokensDto tokens = await authApiClient.ChangeInitialPasswordAsync(request, cancellationToken);

        await StartSessionAsync(tokens);
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

    // After a password change the API revokes every session and returns a new token pair for this one
    internal async Task ReplaceTokensAsync(AuthTokensDto tokens)
    {
        await _tokensLock.WaitAsync();

        try
        {
            await SetTokensAsync(tokens);
        }
        finally
        {
            _tokensLock.Release();
        }
    }

    // After "sign out everywhere" the refresh token is already revoked, so there is nothing to call
    internal async Task EndSessionAsync()
    {
        await ClearTokensAsync();

        SessionChanged?.Invoke();
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

    private async Task StartSessionAsync(AuthTokensDto tokens)
    {
        await SetTokensAsync(tokens);
        IsSessionExpired = false;

        SessionChanged?.Invoke();
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