namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed record SetItemRecipeLineRequest(Guid StockItemId, decimal Quantity);

internal sealed record SetItemRecipeRequest(IReadOnlyList<SetItemRecipeLineRequest> Lines);