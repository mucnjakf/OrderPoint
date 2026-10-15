namespace OrderPoint.Admin.Items.Dtos;

public sealed record ItemRecipeDto(
    IReadOnlyList<RecipeLineDto> Lines,
    decimal? Cost,
    decimal Price,
    decimal? Margin);