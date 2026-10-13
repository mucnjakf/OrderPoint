using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace OrderPoint.Shared.Services;

// One per circuit; the layout applies it and the account page changes it, remembered per browser
internal sealed class DarkModeService(ProtectedLocalStorage protectedLocalStorage)
{
    private const string StorageKey = "dark-mode";

    internal event Action? Changed;

    internal bool IsDarkMode { get; private set; } = true;

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<bool> result = await protectedLocalStorage.GetAsync<bool>(StorageKey);

            IsDarkMode = !result.Success || result.Value;
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }
    }

    internal async Task SetAsync(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;

        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(StorageKey, isDarkMode);
    }
}