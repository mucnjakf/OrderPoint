using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Customer.Settings.Enumerations;

namespace OrderPoint.Customer.Settings.Services;

// One per circuit; Auto follows the phone until the guest picks light or dark, remembered per browser
internal sealed class AppearanceService(ProtectedLocalStorage protectedLocalStorage)
{
    private const string StorageKey = "appearance-mode";

    internal event Action? Changed;

    internal AppearanceMode Mode { get; private set; }

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<AppearanceMode> result =
                await protectedLocalStorage.GetAsync<AppearanceMode>(StorageKey);

            Mode = result.Success ? result.Value : AppearanceMode.Auto;
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }
    }

    internal async Task SetAsync(AppearanceMode mode)
    {
        Mode = mode;

        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(StorageKey, mode);
    }
}