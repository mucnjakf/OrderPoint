using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class ItemMapper
{
    private const int MoneyDecimals = 2;

    internal static ItemDto ToItemDto(this Item item, int ordersCount) => new(
        item.Id,
        item.Name,
        item.Description,
        item.Portion,
        item.Price,
        item.Status,
        item.ImageUrl,
        item.Category.ToItemCategoryDto(),
        ordersCount,
        RoundMoney(item.Cost),
        item.IsSoldOut,
        item.CreatedAtUtc,
        item.UpdatedAtUtc);

    internal static MenuItemDto ToMenuItemDto(this Item item) => new(
        item.Id,
        item.Name,
        item.Description,
        item.Portion,
        item.Price,
        item.ImageUrl);

    internal static ItemRecipeDto ToItemRecipeDto(this Item item, IReadOnlyList<RecipeLine> recipeLines)
    {
        decimal? cost = recipeLines.Count == 0 ? null : recipeLines.Sum(line => line.Cost);

        return new ItemRecipeDto(
            recipeLines
                .OrderBy(line => line.StockItem.Name)
                .Select(line => line.ToRecipeLineDto())
                .ToList(),
            RoundMoney(cost),
            item.Price,
            RoundMoney(item.Price - cost));
    }

    internal static ItemCategoryDto ToItemCategoryDto(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Status,
        category.ImageUrl,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);

    private static RecipeLineDto ToRecipeLineDto(this RecipeLine recipeLine) => new(
        recipeLine.StockItemId,
        recipeLine.StockItem.Name,
        recipeLine.StockItem.Group,
        recipeLine.StockItem.Unit,
        recipeLine.StockItem.PackName,
        recipeLine.StockItem.PackSize,
        recipeLine.StockItem.PackCost,
        recipeLine.Quantity,
        Math.Round(recipeLine.Cost, MoneyDecimals, MidpointRounding.AwayFromZero),
        recipeLine.HasServing);

    private static decimal? RoundMoney(decimal? amount)
        => amount is { } value ? Math.Round(value, MoneyDecimals, MidpointRounding.AwayFromZero) : null;
}