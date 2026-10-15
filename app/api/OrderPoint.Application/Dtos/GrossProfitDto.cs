namespace OrderPoint.Application.Dtos;

/// <summary>The period's gross profit: what was paid minus what the sold drinks cost, plus the stock lost.</summary>
/// <param name="Revenue">What was paid in the period, in euros (the summary's revenue).</param>
/// <param name="CostOfGoods">What the stock taken off by completed orders cost, in euros, at the unit cost of the
/// moment it was taken off.</param>
/// <param name="GrossProfit">`Revenue` minus `CostOfGoods`, in euros.</param>
/// <param name="PreviousGrossProfit">The same for the previous period of the same length, in euros.</param>
/// <param name="CostedRevenue">The part of `Revenue` from items that have a recipe, so their cost is known, in
/// euros.</param>
/// <param name="WasteValue">What the recorded waste was worth, in euros (a positive amount is a loss).</param>
/// <param name="CountDifferenceValue">What stock counts and corrections changed, in euros (negative for a loss,
/// positive for a gain).</param>
public sealed record GrossProfitDto(
    decimal Revenue,
    decimal CostOfGoods,
    decimal GrossProfit,
    decimal PreviousGrossProfit,
    decimal CostedRevenue,
    decimal WasteValue,
    decimal CountDifferenceValue);