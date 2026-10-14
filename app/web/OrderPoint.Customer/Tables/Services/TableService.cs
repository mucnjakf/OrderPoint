using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Customer.Shared.Constants;
using OrderPoint.Customer.Tables.Dtos;

namespace OrderPoint.Customer.Tables.Services;

// One per circuit; the table comes from the QR code's URL and is remembered per browser for one visit, so a reload
// keeps it but a guest coming back days later scans again instead of ordering to an old table
internal sealed class TableService(ProtectedLocalStorage protectedLocalStorage)
{
    // As long as the API accepts
    internal const int MaxTableCodeLength = 10;

    private const string StorageKey = "table";

    internal event Action? Changed;

    internal bool IsLoaded { get; private set; }

    internal string? TableCode { get; private set; }

    internal string? AreaName { get; private set; }

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<TableVisitDto> result =
                await protectedLocalStorage.GetAsync<TableVisitDto>(StorageKey);

            bool isThisVisit = result.Success && result.Value!.ScannedAtUtc >= DateTimeOffset.UtcNow - Visit.Length;

            TableCode = isThisVisit ? result.Value!.TableCode : null;
            AreaName = isThisVisit ? result.Value!.AreaName : null;
        }
        catch (CryptographicException)
        {
            // Stored with data protection keys this app no longer has, so the guest scans the QR code again
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }

        IsLoaded = true;
    }

    internal async Task SetAsync(string tableCode, string areaName)
    {
        TableCode = tableCode;
        AreaName = areaName;

        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(
            StorageKey,
            new TableVisitDto(tableCode, areaName, DateTimeOffset.UtcNow));
    }

    // The table was removed or closed since it was scanned, so the guest scans again
    internal async Task ClearAsync()
    {
        TableCode = null;
        AreaName = null;

        Changed?.Invoke();

        await protectedLocalStorage.DeleteAsync(StorageKey);
    }
}