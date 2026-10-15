using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Items.Dtos;

public sealed record RecipeIngredientDto(
    Guid StockItemId,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    decimal PackSize,
    decimal PackCost)
{
    public decimal UnitCost => PackCost / PackSize;
}