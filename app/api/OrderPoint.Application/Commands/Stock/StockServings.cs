using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Commands.Stock;

internal static class StockServings
{
    // The lines come with their (changed) stock items; quantitiesBefore holds each stock item's quantity before
    internal static bool HaveChanged(
        IReadOnlyList<RecipeLine> recipeLines,
        IReadOnlyDictionary<Guid, decimal> quantitiesBefore)
    {
        return recipeLines.Any(line =>
            quantitiesBefore.TryGetValue(line.StockItemId, out decimal quantityBefore)
            && quantityBefore >= line.Quantity != line.HasServing);
    }
}