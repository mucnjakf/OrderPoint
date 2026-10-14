using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Customer.Cart.Dtos;
using OrderPoint.Customer.Shared.Constants;

namespace OrderPoint.Customer.Cart.Services;

// One per circuit, remembered per browser so a reload keeps the cart; it is forgotten one visit after its last change
internal sealed class CartService(ProtectedLocalStorage protectedLocalStorage)
{
    // Enough for a round; a bigger order is easier at the bar
    internal const int MaxQuantity = 20;

    private const string StorageKey = "cart";

    private List<CartLineDto> _lines = [];

    internal event Action? Changed;

    internal IReadOnlyList<CartLineDto> Lines => _lines;

    internal string? Note { get; private set; }

    internal int ItemsCount => _lines.Sum(line => line.Quantity);

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<CartDto> result = await protectedLocalStorage.GetAsync<CartDto>(StorageKey);

            if (result.Success && result.Value!.UpdatedAtUtc >= DateTimeOffset.UtcNow - Visit.Length)
            {
                _lines = result.Value.Lines.ToList();
                Note = result.Value.Note;
            }
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(StorageKey);
        }
    }

    internal int GetQuantity(Guid itemId)
    {
        return _lines.FirstOrDefault(line => line.ItemId == itemId)?.Quantity ?? 0;
    }

    internal async Task AddAsync(Guid itemId, Guid categoryId, string name, int quantity)
    {
        int newQuantity = Math.Min(GetQuantity(itemId) + quantity, MaxQuantity);

        _lines.RemoveAll(line => line.ItemId == itemId);
        _lines.Add(new CartLineDto(itemId, categoryId, name, newQuantity));

        await SaveAsync();
    }

    // 0 removes the line
    internal async Task SetQuantityAsync(Guid itemId, int quantity)
    {
        int index = _lines.FindIndex(line => line.ItemId == itemId);

        if (index < 0)
        {
            return;
        }

        if (quantity <= 0)
        {
            _lines.RemoveAt(index);
        }
        else
        {
            _lines[index] = _lines[index] with { Quantity = Math.Min(quantity, MaxQuantity) };
        }

        await SaveAsync();
    }

    internal async Task SetNoteAsync(string? note)
    {
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        await SaveAsync();
    }

    internal async Task ClearAsync()
    {
        _lines = [];
        Note = null;

        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        Changed?.Invoke();

        await protectedLocalStorage.SetAsync(StorageKey, new CartDto(_lines, Note, DateTimeOffset.UtcNow));
    }
}