using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OrderPoint.Customer.Cart.Dtos;

namespace OrderPoint.Customer.Cart.Services;

// One per circuit, remembered per browser so a reload keeps the cart
internal sealed class CartService(ProtectedLocalStorage protectedLocalStorage)
{
    // Enough for a round; a bigger order is easier at the bar
    internal const int MaxQuantity = 20;

    private const string LinesStorageKey = "cart-lines";

    private const string NoteStorageKey = "cart-note";

    private List<CartLineDto> _lines = [];

    internal event Action? Changed;

    internal IReadOnlyList<CartLineDto> Lines => _lines;

    internal string? Note { get; private set; }

    internal int ItemsCount => _lines.Sum(line => line.Quantity);

    internal async Task LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<List<CartLineDto>> lines =
                await protectedLocalStorage.GetAsync<List<CartLineDto>>(LinesStorageKey);

            ProtectedBrowserStorageResult<string> note = await protectedLocalStorage.GetAsync<string>(NoteStorageKey);

            _lines = lines.Success ? lines.Value ?? [] : [];
            Note = note.Success && !string.IsNullOrWhiteSpace(note.Value) ? note.Value : null;
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(LinesStorageKey);
            await protectedLocalStorage.DeleteAsync(NoteStorageKey);
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

        await protectedLocalStorage.SetAsync(LinesStorageKey, _lines);
        await protectedLocalStorage.SetAsync(NoteStorageKey, Note ?? string.Empty);
    }
}