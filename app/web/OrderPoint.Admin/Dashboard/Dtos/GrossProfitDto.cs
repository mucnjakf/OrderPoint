namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record GrossProfitDto(
    decimal Revenue,
    decimal CostOfGoods,
    decimal GrossProfit,
    decimal PreviousGrossProfit,
    decimal CostedRevenue,
    decimal WasteValue,
    decimal CountDifferenceValue);