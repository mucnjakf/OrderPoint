using OrderPoint.Bartender.Orders.Dtos;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Bartender.Orders.Services;

// The lines, note and name of one order being taken
internal sealed class OrderDraft
{
    // As many pieces of one item as the API accepts
    internal const int MaxQuantity = 20;

    private readonly List<OrderDraftLineDto> _lines = [];

    internal IReadOnlyList<OrderDraftLineDto> Lines => _lines;

    internal string? Note { get; set; }

    // Only for the counter: whom the bar calls when it is ready
    internal string? CustomerName { get; set; }

    internal int ItemsCount => _lines.Sum(line => line.Quantity);

    // Today's prices; a line whose item left the menu counts nothing
    internal decimal GetTotal(IReadOnlyList<MenuCategoryDto> menu)
    {
        Dictionary<Guid, decimal> prices = menu
            .SelectMany(category => category.Items)
            .ToDictionary(item => item.Id, item => item.Price);

        return _lines.Sum(line => prices.GetValueOrDefault(line.ItemId) * line.Quantity);
    }

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

        OrderDraftLineDto line = new(itemId, name, clampedQuantity);

        if (index >= 0)
        {
            _lines[index] = line;
        }
        else
        {
            _lines.Add(line);
        }
    }
}