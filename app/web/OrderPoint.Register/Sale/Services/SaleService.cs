using OrderPoint.Register.Sale.Dtos;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Register.Sale.Services;

// One per circuit: the sale on the till, kept while the bartender looks at another page
internal sealed class SaleService
{
    // As many pieces of one item as the API accepts
    internal const int MaxQuantity = 20;

    private readonly List<SaleLineDto> _lines = [];

    internal IReadOnlyList<SaleLineDto> Lines => _lines;

    internal string? Note { get; set; }

    // Whom the bar calls when it is ready
    internal string? CustomerName { get; set; }

    internal int GetQuantity(Guid itemId)
    {
        return _lines.FirstOrDefault(line => line.ItemId == itemId)?.Quantity ?? 0;
    }

    // 0 removes the line
    internal void SetQuantity(Guid itemId, string name, int quantity)
    {
        int index = _lines.FindIndex(line => line.ItemId == itemId);
        int clampedQuantity = Math.Clamp(quantity, 0, MaxQuantity);

        if (clampedQuantity == 0)
        {
            if (index >= 0)
            {
                _lines.RemoveAt(index);
            }

            return;
        }

        SaleLineDto line = new(itemId, name, clampedQuantity);

        if (index >= 0)
        {
            _lines[index] = line;
        }
        else
        {
            _lines.Add(line);
        }
    }

    // Today's prices; a line whose item left the menu counts nothing
    internal decimal GetTotal(IReadOnlyList<MenuCategoryDto> menu)
    {
        Dictionary<Guid, decimal> prices = menu
            .SelectMany(category => category.Items)
            .ToDictionary(item => item.Id, item => item.Price);

        return _lines.Sum(line => prices.GetValueOrDefault(line.ItemId) * line.Quantity);
    }

    // The lines, the note and the name, for the next guest
    internal void Clear()
    {
        _lines.Clear();
        Note = null;
        CustomerName = null;
    }
}