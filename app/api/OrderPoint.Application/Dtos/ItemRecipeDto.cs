namespace OrderPoint.Application.Dtos;

/// <summary>A menu item's recipe, with what one serving costs and earns.</summary>
/// <param name="Lines">The ingredients; empty when the item's stock isn't tracked.</param>
/// <param name="Cost">What one serving costs in stock, in euros, or `null` without a recipe.</param>
/// <param name="Price">The item's price, in euros.</param>
/// <param name="Margin">The price minus the cost, in euros, or `null` without a recipe.</param>
public sealed record ItemRecipeDto(
    IReadOnlyList<RecipeLineDto> Lines,
    decimal? Cost,
    decimal Price,
    decimal? Margin);