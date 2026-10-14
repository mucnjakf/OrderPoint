using OrderPoint.Shared.Account.Api;
using OrderPoint.Shared.Account.Dtos;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Services;

namespace OrderPoint.Shared.Account.Services;

// One instance per circuit; MainLayout loads it after sign-in, the account page shows and changes the image
internal sealed class AccountService(
    ApiService apiService,
    AccountApiClient accountApiClient,
    AuthService authService)
{
    private const string AccountDeletedCode = "Auth.AccountDeleted";

    internal event Action? Changed;

    internal string? ImageUrl { get; private set; }

    internal string? FirstName { get; private set; }

    internal string? LastName { get; private set; }

    internal string? PhoneNumber { get; private set; }

    internal async Task LoadAsync()
    {
        AccountDto? account = await apiService.ExecuteAsync(() => GetAccountOrEndSessionAsync());

        ImageUrl = account?.ImageUrl;
        FirstName = account?.FirstName;
        LastName = account?.LastName;
        PhoneNumber = account?.PhoneNumber;

        Changed?.Invoke();
    }

    internal void Clear()
    {
        ImageUrl = null;
        FirstName = null;
        LastName = null;
        PhoneNumber = null;

        Changed?.Invoke();
    }

    // A bartender deleted while signed in still has a valid access token; the layout then shows the login form
    private async Task<AccountDto> GetAccountOrEndSessionAsync()
    {
        try
        {
            return await accountApiClient.GetAccountAsync();
        }
        catch (ApiException exception) when (exception.HasErrorCode(AccountDeletedCode))
        {
            await authService.EndSessionAsync();

            throw;
        }
    }
}