using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Customer.Shared.Constants;

namespace OrderPoint.Customer.Orders.Services;

// One per circuit: the orders placed on this phone, remembered per browser for one visit
internal sealed class MyOrdersService(ProtectedLocalStorage protectedLocalStorage)
{
    private const string StorageKey = "my-orders";

    private List<MyOrderDto> _orders = [];

    internal event Action? Changed;

    // Newest first
    internal IReadOnlyList<Guid> OrderIds => _orders
        .OrderByDescending(order => order.PlacedAtUtc)
        .Select(order => order.Id)
        .ToList();

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<List<MyOrderDto>> result =
                await protectedLocalStorage.GetAsync<List<MyOrderDto>>(StorageKey);

            DateTimeOffset visitStartUtc = DateTimeOffset.UtcNow - Visit.Length;

            _orders = (result.Success ? result.Value ?? [] : [])
                .Where(order => order.PlacedAtUtc >= visitStartUtc)
                .ToList();
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }
    }

    internal async Task AddAsync(Guid orderId, DateTimeOffset placedAtUtc)
    {
        _orders.Add(new MyOrderDto(orderId, placedAtUtc));

        await SaveAsync();
    }

    internal async Task ClearAsync()
    {
        _orders = [];

        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(StorageKey, _orders);
    }
}