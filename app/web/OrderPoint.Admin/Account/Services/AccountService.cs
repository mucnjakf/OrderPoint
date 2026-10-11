using OrderPoint.Admin.Account.Api;
using OrderPoint.Admin.Account.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Account.Services;

// One instance per circuit; MainLayout loads it after sign-in, the account page shows and changes the image
internal sealed class AccountService(ApiService apiService, AccountApiClient accountApiClient)
{
    internal event Action? Changed;

    internal string? ImageUrl { get; private set; }

    internal async Task LoadAsync()
    {
        AccountDto? account = await apiService.ExecuteAsync(() => accountApiClient.GetAccountAsync());

        ImageUrl = account?.ImageUrl;

        Changed?.Invoke();
    }

    internal void Clear()
    {
        ImageUrl = null;

        Changed?.Invoke();
    }
}