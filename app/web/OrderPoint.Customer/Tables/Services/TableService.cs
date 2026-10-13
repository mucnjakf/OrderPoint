using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace OrderPoint.Customer.Tables.Services;

// One per circuit; the table comes from the QR code's URL and is remembered per browser, so a reload keeps it
internal sealed class TableService(ProtectedLocalStorage protectedLocalStorage)
{
    // As long as the API accepts
    internal const int MaxTableCodeLength = 10;

    private const string StorageKey = "table-code";

    internal event Action? Changed;

    internal bool IsLoaded { get; private set; }

    internal string? TableCode { get; private set; }

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<string> result = await protectedLocalStorage.GetAsync<string>(StorageKey);

            TableCode = result.Success ? result.Value : null;
        }
        catch (CryptographicException)
        {
            // Stored with data protection keys this app no longer has, so the guest scans the QR code again
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }

        IsLoaded = true;
    }

    internal async Task SetAsync(string tableCode)
    {
        TableCode = tableCode;

        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(StorageKey, tableCode);
    }
}