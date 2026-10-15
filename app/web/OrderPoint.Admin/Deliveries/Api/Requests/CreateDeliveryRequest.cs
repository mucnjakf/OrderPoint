namespace OrderPoint.Admin.Deliveries.Api.Requests;

internal sealed record CreateDeliveryLineRequest(Guid StockItemId, decimal Packs, decimal PackCost);

internal sealed record CreateDeliveryRequest(
    Guid SupplierId,
    DateOnly DeliveredOn,
    string? Reference,
    string? Note,
    IReadOnlyList<CreateDeliveryLineRequest> Lines);