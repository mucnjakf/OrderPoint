using OrderPoint.Shared.Account.Api;
using OrderPoint.Shared.Account.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Shared.Account.Services;

// One instance per circuit; MainLayout loads it after sign-in, the account page shows and changes the image
internal sealed class AccountService(ApiService apiService, AccountApiClient accountApiClient)
{
    internal event Action? Changed;

    internal string? ImageUrl { get; private set; }

    internal string? FirstName { get; private set; }

    internal string? LastName { get; private set; }

    internal string? PhoneNumber { get; private set; }

    internal async Task LoadAsync()
    {
        AccountDto? account = await apiService.ExecuteAsync(() => accountApiClient.GetAccountAsync());

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
}